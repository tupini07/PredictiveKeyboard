using Lib.Extensions;
using System.Text.RegularExpressions;

namespace Lib
{
    internal class NGram
    {
        private readonly HashSet<string> PUNCTUATIONS = new HashSet<string> { "!", "?", "." };

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

            foreach (var word in words)
            {
                var wordHash = word.GetHashCode();
                id2word[wordHash] = word;
                word2id[word] = wordHash;
            }

#if DEBUG
            Console.WriteLine($"Created NGram structure with size {Size} and {this.word2id.Keys.Count} words");
#endif

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
            // remove newlines and other useless characters
            text = Regex.Replace(text.ToLower(), @"\n|\t|\r", " ");

            // collapse all spaces to 1 space
            text = Regex.Replace(text, @"\s+", " ");

            // normalize quotes
            text = Regex.Replace(text, "“", "\"");
            text = Regex.Replace(text, "”", "\"");
            text = Regex.Replace(text, "’", "'");

            // normalize dashes
            text = Regex.Replace(text, "—", "-");

            // not really necessary, but some people put periods before closed parenthesis
            text = text.Replace(".)", ").");

            // split on relevant characters
            var words = from w in Regex.Split(text, @"(\s|'|,|;|:|""|!|\?|\.|-|@|{|}|\[|\])") where w != "" && w != " " select w;
            return words;
        }

        private List<int> CreateEmptyNgram()
        {
            var emptyGram = new List<int>();
            for (int i = 0; i < this.Size; i++)
            {
                emptyGram.Add(UNKNOWN_ID);
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
                currentGram.Add(word2id[word]);

                allGramsInInput.Add(currentGram.CopyList());

                // if it's a sentence end then new gram should start from empty
                var poppedWord = id2word[popped];
                if (PUNCTUATIONS.Contains(poppedWord))
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
