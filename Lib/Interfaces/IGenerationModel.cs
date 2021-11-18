using Lib.Entities;

namespace Lib.Interfaces
{
    public interface IGenerationModel
    {
        public abstract void Hydrate(string corpus);
        public abstract void Clear();

        public abstract List<Prediction> PredictNextOptions(string currentText, int maxResults = 20);
    }
}
