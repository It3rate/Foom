using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersTests.CoreTests
{
    [TestClass]
    public class NumberChainTests
    {
        private Brain _brain;
        private Trait _trait;
        private Focal _unitFocal;
        private Focal _maxMin;
        private Domain _domain;

        [TestInitialize]
        public void Init()
        {
            _brain = Brain.ActiveBrain;
            _trait = Trait.CreateIn(_brain, "number tests");
            _unitFocal = new Focal(0, 10);
            _maxMin = new Focal(-1000, 1000);
            _domain = new Domain(_trait, _unitFocal, _maxMin, "NumberChainTests");
        }
        //[TestMethod]
        //public void NoOverlapTest()
        //{
        //    var result = new NumberChain(_domain, new Focal(10, 20), new Focal(30, 40));

        //    result.RemoveOverlaps();
        //    Assert.AreEqual(2, result.Count);
        //    CollectionAssert.AreEqual(new List<Focal>
        //        {
        //            new Focal(10, 20),
        //            new Focal(30, 40)
        //        }, result.GetFocals());
        //}

        //[TestMethod]
        //public void OverlapTest()
        //{
        //    var result = new NumberChain(_domain, new Focal[]
        //    {
        //            new Focal(10, 20),
        //            new Focal(15, 25),
        //            new Focal(30, 40)
        //    });
        //    result.RemoveOverlaps();
        //    Assert.AreEqual(2, result.Count);
        //    CollectionAssert.AreEqual(new List<Focal>
        //        {
        //            new Focal(10, 25),
        //            new Focal(30, 40)
        //        }, result.GetFocals());
        //}

        //[TestMethod]
        //public void MultipleOverlapTest()
        //{
        //    var result = new NumberChain(_domain, new Focal[]
        //    {
        //            new Focal(10, 20),
        //            new Focal(15, 25),
        //            new Focal(30, 40),
        //            new Focal(35, 45)
        //        });
        //    result.RemoveOverlaps();
        //    Assert.AreEqual(2, result.Count);
        //    CollectionAssert.AreEqual(new List<Focal> { new Focal(10, 25), new Focal(30, 45) }, result.GetFocals());
        //}

        //[TestMethod]
        //public void AllOverlapTest()
        //{
        //    var result = new NumberChain(_domain, new Focal[]
        //        {
        //            new Focal(10, 20),
        //            new Focal(15, 25),
        //            new Focal(20, 30)
        //        });
        //    result.RemoveOverlaps();
        //    Assert.AreEqual(1, result.Count);
        //    CollectionAssert.AreEqual(new List<Focal> { new Focal(10, 30) }, result.GetFocals());
        //}
    }

}
