using PredictiveKeyboardLib.Extensions;

namespace PredictiveKeyboardLib
{
    internal class NGram
    {
        public const int UNKNOWN_ID = -1;
        public const string UNKNOWN_TOKEN = "UNK";

        private Dictionary<int, string> id2word = new Dictionary<int, string>();
        private Dictionary<string, int> word2id = new Dictionary<string, int>();
        public List<List<int>> AllGrams { get; private set; } = new List<List<int>>();

        private int Size;

        public NGram(int Size = 4)
        {
            this.id2word.Add(UNKNOWN_ID, UNKNOWN_TOKEN);
            this.word2id.Add(UNKNOWN_TOKEN, UNKNOWN_ID);

            this.Size = Size;
        }

        public void AddContent(string corpus)
        {
            var words = SplitTextIntoWords(corpus);

            var knownWords = new HashSet<string>();
            foreach (var word in words) knownWords.Add(word);

            // also consider any other words we might already know (if any)
            foreach (var previouslyKnownWord in this.word2id.Keys)
            {
                knownWords.Add(previouslyKnownWord);
            }

#if DEBUG
            Console.WriteLine($"Created NGram structure with size {Size} and {knownWords.Count} words");
#endif

            foreach (var word in knownWords)
            {
                var wordHash = word.GetHashCode();
                id2word[wordHash] = word;
                word2id[word] = wordHash;
            }

            this.AllGrams.AddRange(GenerateNGrams(words));
        }

        public int GetIdFromWord(string word)
        {
#if DEBUG
            if (SplitTextIntoWords(word).Count() != 1)
            {
                throw new Exception($"{nameof(GetIdFromWord)} should be provided only one word! Provided: {word}");
            }
#endif
            if (word2id.ContainsKey(word))
            {
                return word2id[word];
            }
            else
            {
                return UNKNOWN_ID;
            }
        }

        public string GetWordFromId(int id)
        {
            if (id2word.ContainsKey(id))
            {
                return id2word[id];
            }
            else
            {
                return UNKNOWN_TOKEN;
            }
        }

        public static IEnumerable<string> SplitTextIntoWords(string text)
        {
            text = text.ToLower();
            var words = from w in text.Split(' ') where w != "" select w;
            return words;
        }

        public List<List<int>> GenerateNGrams(IEnumerable<string> words)
        {
            var allGramsInInput = new List<List<int>>();

            var currentGram = new List<int>();
            for (int i = 0; i < this.Size; i++)
            {
                currentGram.Add(-1);
            }

#if DEBUG
            if (currentGram.Count != this.Size)
            {
                throw new Exception("Invalid size for ngram");
            }
#endif

            foreach (var word in words)
            {
                currentGram.PopLeft();
                currentGram.Add(word2id[word]);

                allGramsInInput.Add(currentGram.CopyList());
            }
            return allGramsInInput;
        }
    }
}
