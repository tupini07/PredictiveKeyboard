using Lib.Entities;
using Lib.Models;
using Newtonsoft.Json;

namespace Utils
{
    internal class GenerateModels
    {
        const string BAKED_MODELS_DIR = "Web/wwwroot/api/data/baked-models";

        public static void RecreateBakedModelsDir()
        {
            if (Directory.Exists(BAKED_MODELS_DIR)) Directory.Delete(BAKED_MODELS_DIR, true);
            Directory.CreateDirectory(BAKED_MODELS_DIR);
        }

        public static ModelIndex BakeMarkovModels(ModelIndex? modelIndex = null)
        {
            if (modelIndex == null)
            {
                modelIndex = new ModelIndex();
            }

            foreach (var ngramSize in new List<int> { 2, 3, 4 })
            {
                foreach (var filePath in Directory.EnumerateFiles(@"Tests/TestData"))
                {
                    var dataSourceName = Path.GetFileNameWithoutExtension(filePath);
                    var modelFileName = $"{dataSourceName}_markov_{ngramSize}gram.mdl";

                    Console.WriteLine($"\nGenerating model: {modelFileName}");

                    var data = File.ReadAllText(filePath);

                    var model = new MarkovApproximation(ngramSize: ngramSize);
                    model.Hydrate(data);

                    if (!modelIndex.sourceFiles.Contains(dataSourceName))
                        modelIndex.sourceFiles.Add(dataSourceName);

                    modelIndex.modelFiles.Add(modelFileName);

                    File.WriteAllBytes($"{BAKED_MODELS_DIR}/{modelFileName}", model.ToCompressedData());
                }
            }


            File.WriteAllText($"{BAKED_MODELS_DIR}/index.json", JsonConvert.SerializeObject(modelIndex));

            return modelIndex;
        }


        public static ModelIndex BakeEnsembleMarkovModels(ModelIndex? modelIndex = null)
        {
            const int ensembleSize = 5;

            if (modelIndex == null)
            {
                modelIndex = new ModelIndex();
            }

            foreach (var filePath in Directory.EnumerateFiles(@"Tests/TestData"))
            {
                var dataSourceName = Path.GetFileNameWithoutExtension(filePath);
                var modelFileName = $"{dataSourceName}_markov_ensemble_size{ensembleSize}.mdl";

                Console.WriteLine($"\nGenerating model: {modelFileName}");

                var data = File.ReadAllText(filePath);

                var model = new MarkovEnsemble(ensembleSize);
                model.Hydrate(data);

                if (!modelIndex.sourceFiles.Contains(dataSourceName))
                    modelIndex.sourceFiles.Add(dataSourceName);

                modelIndex.modelFiles.Add(modelFileName);

                File.WriteAllBytes($"{BAKED_MODELS_DIR}/{modelFileName}", model.ToCompressedData());
            }


            File.WriteAllText($"{BAKED_MODELS_DIR}/index.json", JsonConvert.SerializeObject(modelIndex));

            return modelIndex;
        }
    }
}
