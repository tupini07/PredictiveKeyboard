using Lib.Extensions;

namespace Lib.Structures
{
    internal class NGram
    {
        public VocabularyManager vocabulary { get; private set; } = new VocabularyManager();

        public List<List<int>> AllGrams { get; private set; } = new List<List<int>>();

        private int Size;

        public NGram(int Size = 4, VocabularyManager? _vocabulary = null)
        {
            this.vocabulary = _vocabulary ?? this.vocabulary;
            this.Size = Size;
        }

        public void AddContent(string corpus)
        {
            var words = VocabularyManager.SplitTextIntoWords(corpus);

            foreach (var word in words)
            {
                if (!vocabulary.ContainsWord(word))
                {
                    var wordHash = word.GetHashCode();
                    vocabulary.AddVocabularyItem(wordHash, word);
                }
            }

#if DEBUG
            Console.WriteLine($"Created NGram structure with size {Size} and {vocabulary.Count} words");
#endif

            this.AllGrams.AddRange(GenerateNGrams(words));
        }


        private List<int> CreateEmptyNgram()
        {
            var emptyGram = new List<int>();
            for (int i = 0; i < this.Size; i++)
            {
                emptyGram.Add(VocabularyManager.UNKNOWN_ID);
            }

#if DEBUG
            if (emptyGram.Count != this.Size)
            {
                throw new Exception("Invalid size for ngram");
            }
#endif
            return emptyGram;
        }

        public List<List<int>> GenerateNGrams(IEnumerable<string> words)
        {
            var allGramsInInput = new List<List<int>>();

            var currentGram = CreateEmptyNgram();

            foreach (var word in words)
            {
                var popped = currentGram.PopLeft();
                currentGram.Add(vocabulary.GetIdFromWord(word));

                allGramsInInput.Add(currentGram.CopyList());

                // if it's a sentence end then new gram should start from empty
                var poppedWord = vocabulary.GetWordFromId(popped);
                if (VocabularyManager.PUNCTUATIONS.Contains(poppedWord))
                {
                    var subNgram = CreateEmptyNgram();
                    foreach (var tokenInNgram in currentGram)
                    {
                        subNgram.PopLeft();
                        subNgram.Add(tokenInNgram);
                        allGramsInInput.Add(subNgram.CopyList());
                    }
                }
            }

            return allGramsInInput;
        }
    }
}
