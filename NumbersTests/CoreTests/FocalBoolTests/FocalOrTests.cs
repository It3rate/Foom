using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;

namespace NumbersTests.CoreTests.FocalBoolTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class FocalOrTests
    {
        [TestMethod]
        public void NoOverlapTest()
        {
            Focal p = new Focal(10, 20);
            Focal q = new Focal(30, 40);
            Focal[] result = Focal.Or(p, q);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(p, result[0]);
            Assert.AreEqual(q, result[1]);
        }

        [TestMethod]
        public void OverlapTest()
        {
            Focal p = new Focal(10, 20);
            Focal q = new Focal(15, 25);
            Focal[] result = Focal.Or(p, q);
            Assert.AreEqual(1, result.Length);
            CollectionAssert.Contains(result, new Focal(10, 25));
        }
        [TestMethod]
        public void OverlapTestOrder1()
        {
            Focal p = new Focal(15, 25);
            Focal q = new Focal(10, 20);
            Focal[] result = Focal.Or(p, q);
            Assert.AreEqual(1, result.Length);
            CollectionAssert.Contains(result, new Focal(10, 25));
        }
        [TestMethod]
        public void OverlapTestOrder2()
        {
            Focal p = new Focal(25, 15);
            Focal q = new Focal(10, 20);
            Focal[] result = Focal.Or(p, q);
            Assert.AreEqual(1, result.Length);
            CollectionAssert.Contains(result, new Focal(10, 25));
        }
        [TestMethod]
        public void OverlapTestOrder3()
        {
            Focal p = new Focal(25, 15);
            Focal q = new Focal(20, 10);
            Focal[] result = Focal.Or(p, q);
            Assert.AreEqual(1, result.Length);
            CollectionAssert.Contains(result, new Focal(10, 25));
        }
        [TestMethod]
        public void OverlapTestDirection()
        {
            Focal p = new Focal(15, 25);
            Focal q = new Focal(20, 10);
            Focal[] result = Focal.Or(p, q);
            Assert.AreEqual(1, result.Length);
            CollectionAssert.Contains(result, new Focal(10, 25));
        }

    }

}
