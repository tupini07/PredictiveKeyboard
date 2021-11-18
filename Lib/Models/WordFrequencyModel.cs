using Lib.Entities;
using Lib.Extensions;
using Lib.Interfaces;
using Lib.Structures;

namespace Lib.Models
{
    internal class WordFrequencyModel : BaseModel<WordFrequencyModel>, IGenerationModel
    {
        public VocabularyManager vocabulary { get; private set; } = new VocabularyManager();

        public List<Prediction> topWords = new List<Prediction>();


        public WordFrequencyModel(VocabularyManager? _vocabulary = null)
        {
            this.vocabulary = _vocabulary ?? this.vocabulary;
        }

        public void Clear()
        {
            topWords.Clear();
        }

        public void Hydrate(string corpus)
        {
            var words = VocabularyManager.SplitTextIntoWords(corpus);
            var wordCounts = new Dictionary<int, int>();

            foreach (var word in words)
            {
                var wordHash = word.GetHashCode();
                if (!vocabulary.ContainsWordId(wordHash))
                {
                    vocabulary.AddVocabularyItem(wordHash, word);
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
                Word = vocabulary.GetWordFromId(kvp.Key),
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
