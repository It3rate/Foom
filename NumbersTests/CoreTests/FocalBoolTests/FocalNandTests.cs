using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersTests.CoreTests.FocalBoolTests
{
    [TestClass]
    public class FocalNandTests
    {
        [TestMethod]
        public void NoOverlapTest()
        {
            Focal p = new Focal(10, 20);
            Focal q = new Focal(30, 40);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Focal.MinMaxFocal, result[0]);
        }

        [TestMethod]
        public void OverlapTest()
        {
            Focal p = new Focal(10, 20);
            Focal q = new Focal(15, 25);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.Contains(result, new Focal(long.MinValue, 15));
            CollectionAssert.Contains(result, new Focal(20, long.MaxValue));
        }
        [TestMethod]
        public void OverlapTestOrder1()
        {
            Focal p = new Focal(15, 25);
            Focal q = new Focal(10, 20);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.Contains(result, new Focal(long.MinValue, 15));
            CollectionAssert.Contains(result, new Focal(20, long.MaxValue));
        }
        [TestMethod]
        public void OverlapTestOrder2()
        {
            Focal p = new Focal(25, 15);
            Focal q = new Focal(10, 20);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.Contains(result, new Focal(long.MinValue, 15));
            CollectionAssert.Contains(result, new Focal(20, long.MaxValue));
        }
        [TestMethod]
        public void OverlapTestOrder3()
        {
            Focal p = new Focal(25, 15);
            Focal q = new Focal(20, 10);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.Contains(result, new Focal(long.MinValue, 15));
            CollectionAssert.Contains(result, new Focal(20, long.MaxValue));
        }
        [TestMethod]
        public void OverlapTestDirection()
        {
            Focal p = new Focal(15, 25);
            Focal q = new Focal(20, 10);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.Contains(result, new Focal(long.MinValue, 15));
            CollectionAssert.Contains(result, new Focal(20, long.MaxValue));
        }

        [TestMethod]
        public void qBeforePTest()
        {
            Focal p = new Focal(20, 30);
            Focal q = new Focal(10, 15);
            Focal[] result = Focal.Nand(p, q);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Focal.MinMaxFocal, result[0]);
        }
    }

}
