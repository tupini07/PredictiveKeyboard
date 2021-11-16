using Lib.Extensions;

namespace Lib.Models
{
    public class MarkovApproximation
    {
        private const int NGRAM_SIZE = 4;

        private NGram ngramModel;
        private Dictionary<string, Dictionary<int, int>> ngramCounts = new Dictionary<string, Dictionary<int, int>>();

        public MarkovApproximation()
        {
            this.ngramModel = new NGram(NGRAM_SIZE);
        }

        public void Hydrate(string corpus)
        {
            this.ngramModel.AddContent(corpus);

            // clear ngramCounts if we already have data because we'll recompute it from scratch
            // it seems wasteful but it shouldn't be a very common operation
            if (this.ngramCounts.Count > 0)
            {
                this.ngramCounts.Clear();
            }

            foreach (var ngram in this.ngramModel.AllGrams)
            {
                var ngramBase = ngram.GetRange(0, NGRAM_SIZE - 1);
                var ngramBaseId = ngramBase.GetListId();
                var ngramEnd = ngram.Last();

                if (!ngramCounts.ContainsKey(ngramBaseId))
                {
                    ngramCounts[ngramBaseId] = new Dictionary<int, int>();
                }

                var countsForSuffix = ngramCounts[ngramBaseId];
                countsForSuffix[ngramEnd] = countsForSuffix.ContainsKey(ngramEnd) ? countsForSuffix[ngramEnd] + 1 : 1;
            }

#if DEBUG
            Console.WriteLine($"Hydrated Markov approximation model, wutin {ngramCounts.Count} states");
#endif
        }

        public Dictionary<string, float> PredictNextOptions(string currentText)
        {
            if (ngramModel == null)
            {
                throw new ArgumentNullException($"Expected {nameof(ngramModel)} not to be null by now");
            }

            var words = NGram.SplitTextIntoWords(currentText).ToList();
            List<string> lastWords = new List<string>();

            var ngramBaseSize = NGRAM_SIZE - 1;

            if (words.Count >= ngramBaseSize)
            {
                lastWords = words.GetRange(words.Count - ngramBaseSize, ngramBaseSize);
            }
            else
            {
                lastWords = new List<string>();
                var difference = ngramBaseSize - words.Count;
                for (int i = 0; i < difference; i++)
                {
                    lastWords.Add(NGram.UNKNOWN_TOKEN);
                }
                lastWords.AddRange(words);
            }

            var convertedToIds = from w in lastWords select ngramModel.GetIdFromWord(w);

#if DEBUG
            if (convertedToIds.Count() != ngramBaseSize)
            {
                throw new Exception($"Base ngram size does not match expected: {convertedToIds.Count()}");
            }
#endif

            var utteranceId = convertedToIds.ToList().GetListId();
            if (ngramCounts.ContainsKey(utteranceId))
            {
                var matches = ngramCounts[utteranceId];
                var allScore = matches.Select(kvp => kvp.Value).Aggregate((a, b) => a + b);
                return matches.Select(kvp =>
                    new KeyValuePair<string, float>(this.ngramModel.GetWordFromId(kvp.Key), (float)kvp.Value / allScore))
                    .ToDictionary(k => k.Key, v => v.Value);
            }
            else
            {
                return new Dictionary<string, float>();
            }
        }
    }
}
