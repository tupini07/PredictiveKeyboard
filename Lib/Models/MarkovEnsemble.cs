using Lib.Entities;

namespace Lib.Models
{
    public class MarkovEnsemble : BaseModel<MarkovEnsemble>
    {
        private const int MIN_NUMBER_SOLUTION = 18;

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
            var predictions = new List<Prediction>();
            foreach (var submodel in subModels)
            {
                var modelPreds = submodel.PredictNextOptions(currentText);
                modelPreds.OrderByDescending(pred => pred.Score);

                predictions.AddRange(modelPreds);

                if (predictions.Count >= MIN_NUMBER_SOLUTION) break;
            }

            return predictions.Take(MIN_NUMBER_SOLUTION).ToList();
        }
    }
}
