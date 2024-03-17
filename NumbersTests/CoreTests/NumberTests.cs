using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace NumbersTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class NumberTests
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
		    _maxMin = new Focal(-1000, 1010);
		    _domain = new Domain(_trait, _unitFocal, _maxMin, "NumberTests");
	    }
	    [TestMethod]
	    public void UnitChangePositionTests()
	    {
		    var n0 = _domain.CreateNumber(0, 20, true);
		    var n1 = _domain.CreateNumber(20, 0, true);
		    var n2 = _domain.CreateNumber(-30, 20, true);
		    var n3 = _domain.CreateNumber(-20, -30, true);
		    Assert.AreEqual(0, n0.StartValue);
		    Assert.AreEqual(2, n0.EndValue);
		    Assert.AreEqual(-2, n1.StartValue);
		    Assert.AreEqual(0, n1.EndValue);
		    Assert.AreEqual(3, n2.StartValue);
		    Assert.AreEqual(2, n2.EndValue);
		    Assert.AreEqual(2, n3.StartValue);
		    Assert.AreEqual(-3, n3.EndValue);
		    _unitFocal.EndPosition = 20;
		    Assert.AreEqual(0, n0.StartValue);
		    Assert.AreEqual(1, n0.EndValue);
		    Assert.AreEqual(-1, n1.StartValue);
		    Assert.AreEqual(0, n1.EndValue);
		    Assert.AreEqual(1.5, n2.StartValue);
		    Assert.AreEqual(1, n2.EndValue);
		    Assert.AreEqual(1, n3.StartValue);
		    Assert.AreEqual(-1.5, n3.EndValue);
		    _unitFocal.EndPosition = -20;
		    Assert.AreEqual(0, n0.StartValue);
		    Assert.AreEqual(-1, n0.EndValue);
		    Assert.AreEqual(1, n1.StartValue);
		    Assert.AreEqual(0, n1.EndValue);
		    Assert.AreEqual(-1.5, n2.StartValue);
		    Assert.AreEqual(-1, n2.EndValue);
		    Assert.AreEqual(-1, n3.StartValue);
		    Assert.AreEqual(1.5, n3.EndValue);
		    _unitFocal.StartPosition = -10; // unot perspective
		    Assert.AreEqual(1, n0.StartValue);
		    Assert.AreEqual(-3, n0.EndValue);
		    Assert.AreEqual(3, n1.StartValue);
		    Assert.AreEqual(-1, n1.EndValue);
		    Assert.AreEqual(-2, n2.StartValue);
		    Assert.AreEqual(-3, n2.EndValue);
		    Assert.AreEqual(-1, n3.StartValue);
		    Assert.AreEqual(2, n3.EndValue);
		    _unitFocal.StartPosition = 2000; // unot perspective
            _unitFocal.EndPosition = -2000; // forces things to about the middle
            Assert.AreEqual(-0.5, n0.StartValue);
            Assert.AreEqual(0.495, n0.EndValue);
            Assert.AreEqual(-0.495, n1.StartValue);
            Assert.AreEqual(0.5, n1.EndValue);
            Assert.AreEqual(-0.5075, n2.StartValue);
            Assert.AreEqual(0.495, n2.EndValue);
            Assert.AreEqual(-0.505, n3.StartValue);
            Assert.AreEqual(0.5075, n3.EndValue);
        }
	    [TestMethod]
	    public void UnitChangeValueTests()
	    {
		    var n0 = _domain.CreateNumber(new PRange(0, 20), true);
		    var n1 = _domain.CreateNumber(new PRange(20, 0), true);
		    var n2 = _domain.CreateNumber(new PRange(-30, 20), true);
		    var n3 = _domain.CreateNumber(new PRange(-20, -30), true);
		    Assert.AreEqual(0, n0.StartValue);
		    Assert.AreEqual(20, n0.EndValue);
		    Assert.AreEqual(20, n1.StartValue);
		    Assert.AreEqual(0, n1.EndValue);
		    Assert.AreEqual(-30, n2.StartValue);
		    Assert.AreEqual(20, n2.EndValue);
		    Assert.AreEqual(-20, n3.StartValue); 
		    Assert.AreEqual(-30, n3.EndValue);
		    _unitFocal.StartPosition = 100; // unot perspective
		    _unitFocal.EndPosition = 0; 
		    Assert.AreEqual(-1, n0.StartValue);
		    Assert.AreEqual(-1, n0.EndValue);
		    Assert.AreEqual(-3, n1.StartValue);
		    Assert.AreEqual(1, n1.EndValue);
		    Assert.AreEqual(2, n2.StartValue);
		    Assert.AreEqual(-1, n2.EndValue);
		    Assert.AreEqual(1, n3.StartValue);
		    Assert.AreEqual(4, n3.EndValue);
        }

        [TestMethod]
	    public void CoreNumberTests()
	    {
		    var f0 = new Focal(0, 20);
		    var n0 = _domain.CreateNumber(f0);
		    var f1 = new Focal(0, 30);
		    var n1 = _domain.CreateNumber(f1);
		    var f2 = new Focal(-32, 0);
		    var n2 = _domain.CreateNumber(f2);
		    var f3 = new Focal(-50, 45);
		    var n3 = _domain.CreateNumber(f3);
		    var f4 = new Focal(50, -45);
		    var n4 = _domain.CreateNumber(f4);
		    var f5 = new Focal(53, 69);
		    var n5 = _domain.CreateNumber(f5);

		    Assert.AreEqual(MathElementKind.Number, n0.Kind);
		    var n0b = n0.Clone();
		    Assert.AreEqual(n0b, n0);
		    n0b.AddValue(n3);
		    Assert.AreNotEqual(n0, n0b);

		    Assert.AreEqual(_unitFocal.AbsLengthInTicks, n0.AbsBasisTicks);
		    Assert.AreEqual(1, n0.Direction);
		    Assert.AreEqual(1, n4.Direction);
		    Assert.AreEqual(n1.DomainId, n0.Domain.Id);
		    Assert.AreEqual(n0.StartValue, n1.StartValue, Utils.Tolerance);
		    Assert.AreEqual(n1.EndValue, n1.Focal.LengthInTicks / (double) n1.BasisTicks);
		    Assert.AreEqual(0, n3.RemainderStartValue, Utils.Tolerance);
		    Assert.AreEqual(5, n3.RemainderEndValue, Utils.Tolerance);
		    Assert.AreEqual(2, n2.RemainderStartValue, Utils.Tolerance);
		    Assert.AreEqual(0, n2.RemainderEndValue, Utils.Tolerance);

		    Assert.AreEqual(3, n2.CeilingRange.Start, Utils.Tolerance);
		    Assert.AreEqual(0, n2.CeilingRange.End, Utils.Tolerance);
		    Assert.AreEqual(5, n3.FloorRange.Start, Utils.Tolerance);
		    Assert.AreEqual(4, n3.FloorRange.End, Utils.Tolerance);
		    Assert.AreEqual(-5, n5.RoundedRange.Start, Utils.Tolerance);
		    Assert.AreEqual(7, n5.RoundedRange.End, Utils.Tolerance);
		    Assert.AreEqual(-0.3, n5.RemainderRange.Start, Utils.Tolerance);
		    Assert.AreEqual(0.9, n5.RemainderRange.End, Utils.Tolerance);

		    Assert.IsTrue(n3.IsAligned);
		    Assert.IsTrue(_domain.BasisNumber.IsAligned);
		    Assert.IsTrue(n4.IsAligned);
		    Assert.IsFalse(n4.IsInverted);
		    Assert.AreEqual(0.472636, n3.RangeInMinMax.Start, Utils.Tolerance);
		    Assert.AreEqual(0.519900, n3.RangeInMinMax.End, Utils.Tolerance);

		    n1.SubtractValue(n2);
		    Assert.AreEqual(new PRange(-3.2, 3), n1.Value);
		    n2.AddValue(n3);
		    Assert.AreEqual(new PRange(5, 4.5), n3.Value);
		    n3.MultiplyValue(n4);
		    Assert.AreEqual(new PRange(-45, 4.8), n3.Value);
		    n4.DivideValue(n5);
		    Assert.AreEqual(new PRange(-0.8, -0.1), n4.Value);
	    }
    }

}
