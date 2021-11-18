using Lib.Entities;
using Lib.Interfaces;
using Lib.Structures;

namespace Lib.Models
{
    public class MarkovEnsemble : BaseModel<MarkovEnsemble>, IGenerationModel
    {
        private List<IGenerationModel> children;
        private int numberSubmodels;

        public MarkovEnsemble(int submodels = 4)
        {
            children = new List<IGenerationModel>();
            numberSubmodels = submodels;
            InitModels();
        }

        private void InitModels()
        {
            children.Clear();

            var sharedVocab = new VocabularyManager();

            // minimum useful ngram size is a bigram (relation between previous and current word)
            for (var i = numberSubmodels; i > 1; i--)
            {
                children.Add(new MarkovApproximation(ngramSize: i, sharedVocab));
            }

            // finally add a word frequencyt model
            children.Add(new WordFrequencyModel(sharedVocab));
        }

        public void Clear()
        {
            InitModels();
        }

        public void Hydrate(string corpus)
        {
            foreach (var submodel in children)
            {
                submodel.Hydrate(corpus);
            }
        }

        public List<Prediction> PredictNextOptions(string currentText, int maxResults = 18)
        {
            // only one model can predict a certain word
            var predictions = new List<Prediction>();
            var seenWords = new HashSet<string>();

            // get tiered predictions
            for (var i = 0; i < children.Count && predictions.Count <= maxResults; i++)
            {
                var submodel = children[i];
                var modelPreds = submodel.PredictNextOptions(currentText)
                    .OrderByDescending(pred => pred.Score)
                    .Select(pred =>
                    {
                        // The oldest models have lower scores ( /10 for every model)
                        pred.Score = pred.Score / (float)Math.Pow(10, i);
                        return pred;
                    });

                foreach (var mp in modelPreds)
                {
                    if (!seenWords.Contains(mp.Word))
                    {
                        seenWords.Add(mp.Word);
                        predictions.Add(mp);
                    }

                    if (predictions.Count == maxResults)
                        break;
                }
            }

            var cutPreds = predictions.Take(maxResults);

            // normalize scores and return
            float allScores = cutPreds.Aggregate(0.0f, (acc, pred) => acc + pred.Score);
            return cutPreds.Select(pred =>
            {
                pred.Score = pred.Score / allScores;
                return pred;
            }).ToList();
        }
    }
}
