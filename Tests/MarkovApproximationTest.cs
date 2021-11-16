using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.Models;
using System.Linq;

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
            Assert.AreEqual("potato", pred.Keys.ToList()[0]);
            Assert.AreEqual(1, pred["potato"]);

            model.Hydrate("this is some cereal for breakfast");

            pred = model.PredictNextOptions("this is some");
            Assert.AreEqual(2, pred.Count);
            Assert.IsTrue(pred.Keys.ToList().Contains("potato"));
            Assert.IsTrue(pred.Keys.ToList().Contains("cereal"));


            Assert.AreEqual(0.5, pred["potato"]);
            Assert.AreEqual(0.5, pred["cereal"]);
        }
    }
}