using Lib.Entities;

namespace Lib.Models
{
    public class MarkovEnsemble : BaseModel<MarkovEnsemble>
    {
        private const int MAX_NUMBER_PREDICTIONS = 18;

        private List<MarkovApproximation> subModels;
        private int numberSubmodels;

        public MarkovEnsemble(int submodels = 4)
        {
            subModels = new List<MarkovApproximation>();
            numberSubmodels = submodels;
            InitModels();
        }

        private void InitModels()
        {
            subModels.Clear();
            // minimum useful ngram size is a bigram (relation between previous and current word)
            for (var i = 2; i <= numberSubmodels; i++)
            {
                subModels.Add(new MarkovApproximation(ngramSize: i));
            }

            subModels.Reverse();
        }

        public override void Clear()
        {
            InitModels();
        }

        public override void Hydrate(string corpus)
        {
            foreach (var submodel in subModels)
            {
                submodel.Hydrate(corpus);
            }
        }

        public override List<Prediction> PredictNextOptions(string currentText)
        {
            // only one model can predict a certain word
            var predictions = new List<Prediction>();
            var seenWords = new HashSet<string>();

            // get tiered predictions
            for (var i = 0; i < subModels.Count && predictions.Count <= MAX_NUMBER_PREDICTIONS; i++)
            {
                var submodel = subModels[i];
                var modelPreds = submodel.PredictNextOptions(currentText)
                    .OrderByDescending(pred => pred.Score)
                    .Select(pred =>
                    {
                        // The oldest models have lower scores ( /10 for every model)
                        pred.Score = pred.Score / (i == 0 ? 1 : 10 * i);
                        return pred;
                    }); ;

                foreach (var mp in modelPreds)
                {
                    if (!seenWords.Contains(mp.Word))
                    {
                        seenWords.Add(mp.Word);
                        predictions.Add(mp);
                    }

                    if (predictions.Count == MAX_NUMBER_PREDICTIONS)
                        break;
                }
            }

            var cutPreds = predictions.Take(MAX_NUMBER_PREDICTIONS);

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
