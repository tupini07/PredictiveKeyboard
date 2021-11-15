namespace PredictiveKeyboardLib.Interfaces
{
    public interface IGenerationModel
    {
        void Hydrate(string corpus);
        void PredictNextOptions(string currentText, bool learn = false);
    }
}
