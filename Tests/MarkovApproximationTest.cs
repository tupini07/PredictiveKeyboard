using Lib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
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
            Assert.AreEqual("potato", pred.First().Word);
            Assert.AreEqual(1, pred.First().Score);

            model.Hydrate("this is some cereal for breakfast");

            pred = model.PredictNextOptions("this is some");
            Assert.AreEqual(2, pred.Count);

            var predictedWords = pred.Select(p => p.Word);
            Assert.IsTrue(predictedWords.Contains("potato"));
            Assert.IsTrue(predictedWords.Contains("cereal"));


            Assert.AreEqual(0.5, pred.Single(p => p.Word == "potato").Score);
            Assert.AreEqual(0.5, pred.Single(p => p.Word == "cereal").Score);
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

            var pred = model.PredictNextOptions("text");
            Assert.AreEqual(0, pred.Count);
        }


        [TestMethod]
        public void GenerateNGrams_RealData()
        {
            var data = File.ReadAllText(@"TestData/Books/Pride-and-prejudice.txt");
            var model = new MarkovApproximation();
            model.Hydrate(data);

            var pred = model.PredictNextOptions("Darcy made no");

            Assert.AreEqual(1, pred.Count);
            Assert.AreEqual("answer", pred.First().Word);

            pred = model.PredictNextOptions("Darcy made no answer");

            Assert.AreEqual(3, pred.Count);
        }
    }

}