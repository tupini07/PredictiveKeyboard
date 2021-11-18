using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Lib.Structures
{
    public class VocabularyManager
    {
        private Dictionary<int, string> id2word = new Dictionary<int, string>();
        private Dictionary<string, int> word2id = new Dictionary<string, int>();

        [JsonIgnore]
        public int Count
        {
            get { return word2id.Count; }
        }

        public const int UNKNOWN_ID = -1;
        public const string UNKNOWN_TOKEN = "UNK";
        public static readonly HashSet<string> PUNCTUATIONS = new HashSet<string> { "!", "?", "." };


        public VocabularyManager()
        {
            this.AddVocabularyItem(UNKNOWN_ID, UNKNOWN_TOKEN);
        }

        public bool ContainsWordId(int wordId)
        {
            return id2word.ContainsKey(wordId);
        }

        public bool ContainsWord(string word)
        {
            return word2id.ContainsKey(word);
        }

        public string GetWordFromId(int wordId)
        {
            return id2word[wordId];
        }

        public int GetIdFromWord(string word)
        {
#if DEBUG
            if (SplitTextIntoWords(word).Count() != 1)
            {
                throw new Exception($"{nameof(GetIdFromWord)} should be provided only one word! Provided: {word}");
            }
#endif
            if (ContainsWord(word))
            {
                return word2id[word]; ;
            }
            else
            {
                return VocabularyManager.UNKNOWN_ID;
            }
        }

        public void AddVocabularyItem(int wordId, string word)
        {
            id2word[wordId] = word;
            word2id[word] = wordId;
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
    }
}
