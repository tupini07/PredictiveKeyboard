using Lib.Entities;
using Lib.Extensions;
using Lib.Interfaces;

namespace Lib.Models
{
    internal class WordFrequencyModel : BaseModel<WordFrequencyModel>, IGenerationModel
    {
        private Dictionary<int, string> id2word = new Dictionary<int, string>();
        private Dictionary<string, int> word2id = new Dictionary<string, int>();

        public List<Prediction> topWords = new List<Prediction>();


        public void Clear()
        {
            topWords.Clear();
        }

        public void Hydrate(string corpus)
        {
            var words = NGram.SplitTextIntoWords(corpus);
            var wordCounts = new Dictionary<int, int>();

            foreach (var word in words)
            {
                var wordHash = word.GetHashCode();
                if (!id2word.ContainsKey(wordHash))
                {
                    id2word[wordHash] = word;
                    word2id[word] = wordHash;
                }

                var currentWordCound = wordCounts.GetOrAdd(wordHash, (word) => 0);
                wordCounts.Add(wordHash, currentWordCound + 1);
            }

            // once hydrated we set the top words in order
            var rawWopWords = wordCounts
                .ToList()
                .OrderByDescending(kvp => kvp.Value)
                // this can be HUGE so we consider only top ones to save some memory
                .Take(200);

            var sumCounts = rawWopWords.Aggregate(0, (acc, kvp) => acc + kvp.Value);

            topWords = rawWopWords.Select(kvp => new Prediction
            {
                Word = id2word[kvp.Key],
                Score = kvp.Value / sumCounts,
            })
                .ToList();
        }

        public List<Prediction> PredictNextOptions(string currentText, int maxResults = 30)
        {
            return topWords.Take(maxResults).ToList();
        }
    }
}
