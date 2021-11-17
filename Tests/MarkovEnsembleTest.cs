using Lib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            Assert.AreEqual(1, pred.Count);
            Assert.AreEqual("potato", pred.First().Word);
            Assert.AreEqual(1, pred.First().Score);

            model.Hydrate("this is some cereal which is for breakfast");

            pred = model.PredictNextOptions("this is");
            Assert.AreEqual(2, pred.Count);

            var predictedWords = pred.Select(p => p.Word);
            Assert.IsTrue(predictedWords.Contains("some"));
            Assert.IsTrue(predictedWords.Contains("for"));


            // prediction for "some" comes from 3-gram model, so it should have higher score thatn "for", which
            // comes from a 2-gram model
            Assert.IsTrue(
                pred.Single(p => p.Word == "some").Score > pred.Single(p => p.Word == "for").Score
            );
        }
    }
}
