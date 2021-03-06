using Lib.Entities;
using Lib.Extensions;
using Lib.Interfaces;
using Lib.Structures;

namespace Lib.Models
{
    public class MarkovApproximation : BaseModel<MarkovApproximation>, IGenerationModel
    {
        private int NgramSize = 4;

        private NGram ngramModel;
        private Dictionary<string, Dictionary<int, int>> ngramCounts = new Dictionary<string, Dictionary<int, int>>();

        public MarkovApproximation(int ngramSize = 4, VocabularyManager? _vocabulary = null)
        {
            this.NgramSize = ngramSize;
            this.ngramModel = new NGram(NgramSize, _vocabulary);
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
                var ngramBase = ngram.GetRange(0, NgramSize - 1);
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
            //var uniqueNGrams = new HashSet<string>();
            //foreach (var kvp in ngramCounts)
            //{
            //    var baseN = kvp.Key;
            //    foreach (var suffix in kvp.Value.Keys)
            //    {
            //        uniqueNGrams.Add($"{baseN}{suffix}");
            //    }
            //}
            //Console.WriteLine($"Hydrated Markov approximation model with size {NgramSize} and {uniqueNGrams.Count} unique ngrams");


            Console.WriteLine($"Hydrated Markov approximation model with size {NgramSize} and {ngramCounts.Count} base states");
#endif
        }

        public List<Prediction> PredictNextOptions(string currentText, int maxResults = 20)
        {
            if (ngramModel == null)
            {
                throw new ArgumentNullException($"Expected {nameof(ngramModel)} not to be null by now");
            }

            var words = VocabularyManager.SplitTextIntoWords(currentText).ToList();
            List<string> lastWords = new List<string>();

            var ngramBaseSize = NgramSize - 1;

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
                    lastWords.Add(VocabularyManager.UNKNOWN_TOKEN);
                }
                lastWords.AddRange(words);
            }

            var convertedToIds = from w in lastWords select ngramModel.vocabulary.GetIdFromWord(w);

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
                    new Prediction
                    {
                        Word = this.ngramModel.vocabulary.GetWordFromId(kvp.Key),
                        Score = (double)kvp.Value / allScore,
                    })
                    .Take(maxResults)
                    .ToList();
            }
            else
            {
                return new List<Prediction>();
            }
        }

        public void Clear()
        {
            this.ngramModel = new NGram(NgramSize);
            ngramCounts = new Dictionary<string, Dictionary<int, int>>();
        }
    }
}
