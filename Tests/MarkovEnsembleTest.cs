using Lib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class MarkovEnsembleTest
    {
        [TestMethod]
        public void MarkovEnsemble_Basic()
        {
            var model = new MarkovEnsemble();
            model.Hydrate("this is some potato text");

            var pred = model.PredictNextOptions("this is some");

            Assert.AreEqual("potato", pred.First().Word);

            model.Hydrate("this is some cereal which is for breakfast");

            pred = model.PredictNextOptions("this is");

            var predictedWords = pred.Select(p => p.Word);
            Assert.IsTrue(predictedWords.Contains("some"));
            Assert.IsTrue(predictedWords.Contains("for"));


            // prediction for "some" comes from 3-gram model, so it should have higher score thatn "for", which
            // comes from a 2-gram model
            Assert.IsTrue(
                pred.Single(p => p.Word == "some").Score > pred.Single(p => p.Word == "for").Score
            );
        }

        [TestMethod]
        public void MarkovEnsemble_RealData_SerializeDeserialize()
        {
            var data = File.ReadAllText(@"TestData/Pride-and-prejudice.txt");
            var model = new MarkovEnsemble();
            model.Hydrate(data);

            var pred = model.PredictNextOptions("Darcy made no");

            Assert.AreEqual("answer", pred.First().Word);


            var compressed = model.ToCompressedData();
            var decompressed = MarkovEnsemble.FromCompressedData(compressed);

            var decompPred = decompressed.PredictNextOptions("Darcy made no");
            Assert.AreEqual(JsonConvert.SerializeObject(pred), JsonConvert.SerializeObject(decompPred));
        }
    }
}
