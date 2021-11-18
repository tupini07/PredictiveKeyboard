﻿using Lib.Entities;
using Lib.Extensions;
using Lib.Interfaces;

namespace Lib.Models
{
    public class MarkovApproximation : BaseModel<MarkovApproximation>, IGenerationModel
    {
        private int NgramSize = 4;

        private NGram ngramModel;
        private Dictionary<string, Dictionary<int, int>> ngramCounts = new Dictionary<string, Dictionary<int, int>>();

        public MarkovApproximation(int ngramSize = 4, Dictionary<int, string>? _id2word = null, Dictionary<string, int>? _word2id = null)
        {
            this.NgramSize = ngramSize;
            this.ngramModel = new NGram(NgramSize, _id2word, _word2id);
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
            Console.WriteLine($"Hydrated Markov approximation model, with {ngramCounts.Count} states");
#endif
        }

        public List<Prediction> PredictNextOptions(string currentText, int maxResults = 20)
        {
            if (ngramModel == null)
            {
                throw new ArgumentNullException($"Expected {nameof(ngramModel)} not to be null by now");
            }

            var words = NGram.SplitTextIntoWords(currentText).ToList();
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
                    new Prediction
                    {
                        Word = this.ngramModel.GetWordFromId(kvp.Key),
                        Score = (float)kvp.Value / allScore,
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
