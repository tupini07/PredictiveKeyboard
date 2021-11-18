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


        public WordFrequencyModel(Dictionary<int, string>? _id2word = null, Dictionary<string, int>? _word2id = null)
        {
            this.id2word = _id2word ?? this.id2word;
            this.word2id = _word2id ?? this.word2id;
        }

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
                wordCounts[wordHash] = currentWordCound + 1;
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
                Score = (float)kvp.Value / sumCounts,
            })
                .ToList();
        }

        public List<Prediction> PredictNextOptions(string currentText, int maxResults = 30)
        {
            return topWords.Take(maxResults).ToList();
        }
    }
}
