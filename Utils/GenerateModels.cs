using Lib.Entities;
using Lib.Models;
using Lib.Serialization;
using Lib.Utils;
using Newtonsoft.Json;

namespace Utils
{
    internal class GenerateModels
    {
        public static void BakeMarkovModels()
        {
            const string BAKED_MODELS_DIR = "Web/wwwroot/api/data/baked-models";
            if (Directory.Exists(BAKED_MODELS_DIR)) Directory.Delete(BAKED_MODELS_DIR, true);
            Directory.CreateDirectory(BAKED_MODELS_DIR);

            var indexData = new ModelIndex();

            foreach (var ngramSize in new List<int> { 2, 3, 4 })
            {
                foreach (var filePath in Directory.EnumerateFiles(@"Tests/TestData"))
                {
                    var dataSourceName = Path.GetFileNameWithoutExtension(filePath);
                    var modelFileName = $"{dataSourceName}_{ngramSize}gram.mdl";

                    Console.WriteLine($"\nGenerating model: {modelFileName}");

                    var data = File.ReadAllText(filePath);
                    var model = new MarkovApproximation(ngramSize: ngramSize);
                    model.Hydrate(data);

                    var serialized = JsonConvert.SerializeObject(model, new JsonSerializerSettings
                    {
                        ContractResolver = new ModelJsonContractResolver()
                    });


                    indexData.sourceFiles.Add(dataSourceName);
                    indexData.modelFiles.Add(modelFileName);

                    File.WriteAllBytes($"{BAKED_MODELS_DIR}/{modelFileName}", Compressor.Zip(serialized));
                }
            }


            File.WriteAllText($"{BAKED_MODELS_DIR}/index.json", JsonConvert.SerializeObject(indexData));
        }
    }
}
