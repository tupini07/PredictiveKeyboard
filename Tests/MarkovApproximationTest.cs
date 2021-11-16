using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.Models;
using System.Linq;
using System.IO;

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
    }

}