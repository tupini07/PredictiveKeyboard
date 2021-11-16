using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.Models;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Lib.Serialization;
using Lib.Utils;
using System.Collections.Generic;
using Web.Models;

namespace Tests
{
    [TestClass]
    public class MarkovApproximationTest
    {
        [TestMethod]
        public void GenerateNGrams_Basic()
        {
            var model = new MarkovApproximation();
            model.Hydrate("this is some potato text");

            var pred = model.PredictNextOptions("this is some");

            Assert.AreEqual(1, pred.Count);
            Assert.AreEqual("potato", pred.Keys.First());
            Assert.AreEqual(1, pred["potato"]);

            model.Hydrate("this is some cereal for breakfast");

            pred = model.PredictNextOptions("this is some");
            Assert.AreEqual(2, pred.Count);
            Assert.IsTrue(pred.Keys.ToList().Contains("potato"));
            Assert.IsTrue(pred.Keys.ToList().Contains("cereal"));


            Assert.AreEqual(0.5, pred["potato"]);
            Assert.AreEqual(0.5, pred["cereal"]);
        }

        [TestMethod]
        public void GenerateNGrams_MultipleSentencesEmptyStart()
        {
            var model = new MarkovApproximation();
            model.Hydrate("This is one sentence. And this is another! A here is a third. Hi this one ends with a question? Who thought that this will be an exclamation!");
            var pred = model.PredictNextOptions("");

            Assert.AreEqual(5, pred.Count);
        }

        [TestMethod]
        public void GenerateNGrams_PredictWithUnexistentNgram()
        {
            var model = new MarkovApproximation();
            model.Hydrate("this is some potato text");

            var pred = model.PredictNextOptions("something");
            Assert.AreEqual(0, pred.Count);
        }


        [TestMethod]
        public void GenerateNGrams_RealData()
        {
            var data = File.ReadAllText(@"TestData\Pride-and-prejudice.txt");
            var model = new MarkovApproximation();
            model.Hydrate(data);

            var pred = model.PredictNextOptions("Darcy made no");

            Assert.AreEqual(1, pred.Count);
            Assert.AreEqual("answer", pred.Keys.First());

            pred = model.PredictNextOptions("Darcy made no answer");

            Assert.AreEqual(3, pred.Count);
        }


        [TestMethod, TestCategory("Utility")]
        public void BakeModels()
        {
            const string BAKED_MODELS_DIR = "../../../../Web/wwwroot/api/data/baked-models";
            if (Directory.Exists(BAKED_MODELS_DIR)) Directory.Delete(BAKED_MODELS_DIR, true);
            Directory.CreateDirectory(BAKED_MODELS_DIR);

            var indexData = new ModelIndex();

            foreach (var ngramSize in new List<int> { 2, 3, 4 })
            {
                foreach (var filePath in Directory.EnumerateFiles(@"TestData"))
                {
                    var data = File.ReadAllText(filePath);
                    var model = new MarkovApproximation(ngramSize: ngramSize);
                    model.Hydrate(data);

                    var serialized = JsonConvert.SerializeObject(model, new JsonSerializerSettings
                    {
                        ContractResolver = new ModelJsonContractResolver()
                    });

                    var dataSourceName = Path.GetFileNameWithoutExtension(filePath);
                    var modelFileName = $"{dataSourceName}_{ngramSize}gram.mdl";

                    indexData.sourceFiles.Add(dataSourceName);
                    indexData.modelFiles.Add(modelFileName);

                    File.WriteAllBytes($"{BAKED_MODELS_DIR}/{modelFileName}", Compressor.Zip(serialized));
                }
            }


            File.WriteAllText($"{BAKED_MODELS_DIR}/index.json", JsonConvert.SerializeObject(indexData));
        }
    }

}