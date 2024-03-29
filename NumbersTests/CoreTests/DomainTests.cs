using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace NumbersTests;

using System.Collections.Generic;

[TestClass]
public class DomainTests
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
        _trait = Trait.CreateIn(_brain, "domain tests");
        _unitFocal = new Focal(-4, 6);
        _maxMin = new Focal(-54, 46);
        _domain = new Domain(_trait, _unitFocal, _maxMin, "domainTests");
    }

    [TestMethod]
    public void CoreDomainTests()
    {
        Assert.AreEqual(MathElementKind.Domain, _domain.Kind);
        Assert.AreEqual(100, _domain.MinMaxFocal.LengthInTicks);
        Assert.AreEqual(10, _domain.BasisFocal.LengthInTicks);
        Assert.AreEqual(_domain.BasisFocal.Id, _unitFocal.Id);
        Assert.AreEqual(_domain.MinMaxFocal.Id, _maxMin.Id);
        Assert.AreEqual(_domain.MinMaxRange.Length, 10.0, Utils.Tolerance);

        var f0 = new Focal(10, 20);
        var n0 = _domain.CreateNumber(f0);
        var n1 = _domain.CreateNumber(f0.Clone());
        var n2 = _domain.CreateNumber(f0.Clone());
        Assert.AreEqual(5, _domain.NumberStore.Count); // includes unit basis and minmax

        var dict = new Dictionary<int, PRange>();

        _domain.SaveNumberValues(dict, _domain.BasisNumber.Id);
        Assert.AreEqual(4, dict.Count); // does not include unit
        var saved = n1.Focal.EndPosition;
        n1.Focal.EndPosition = 22;
        Assert.AreNotEqual(n1.Focal.EndPosition, saved);
        Assert.AreEqual(22, n1.Focal.EndPosition);
        _domain.RestoreNumberValues(dict);
        Assert.AreEqual(saved, n1.Focal.EndPosition);
    }

    [TestMethod]
    public void DomainValueTests()
    {
        _unitFocal.Reset(0, 10);

        var num = _domain.CreateNumber(new Focal(30, 40));
        var r = num.Value;
        var ffr = _domain.CreateFocalFromRange(r);
        Assert.AreEqual(ffr, num.Focal);

        num = _domain.CreateNumber(new Focal(-30, 1));
        r = num.Value;
        ffr = _domain.CreateFocalFromRange(r);
        Assert.AreEqual(ffr, num.Focal);

        _unitFocal.Reset(10, -10);
        var testFocal = new Focal(30, 40);

        num = _domain.CreateNumber(new Focal(30, 40));
        r = num.Value;
        _domain.SetValueOf(num, r);

        Assert.AreEqual(num.Focal.StartPosition, testFocal.StartPosition);
        Assert.AreEqual(num.Focal.EndPosition, testFocal.EndPosition);
        ffr = _domain.CreateFocalFromRange(r);
        Assert.AreEqual(ffr, num.Focal);
    }
}
