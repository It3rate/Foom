using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace NumbersTests;

[TestClass]
public class FocalTests
{
    private Brain _brain => Brain.ActiveBrain;
    private Trait _trait;

    [TestInitialize]
    public void Init()
    {
        _trait = Trait.CreateIn(_brain, "numbers tests");
    }

    [TestMethod]
    public void InvertedEndPositionTests()
    {
        Focal f0 = new Focal(0, 10);
        Assert.AreEqual(-10, f0.InvertedEndPosition);
        Focal f1 = new Focal(20, 30);
        Assert.AreEqual(10, f1.InvertedEndPosition);
        Focal f2 = new Focal(-20, -15);
        Assert.AreEqual(-25, f2.InvertedEndPosition);
        Focal f3 = new Focal(-20, -35);
        Assert.AreEqual(-5, f3.InvertedEndPosition);
        Focal f4 = new Focal(30, 15);
        Assert.AreEqual(45, f4.InvertedEndPosition);
        Focal f5 = new Focal(30, 45);
        Assert.AreEqual(15, f5.InvertedEndPosition);
    }

    [TestMethod]
    public void RangeWithBasisTests()
    {
        Focal bPos = new Focal(10, 20);
        Focal bNeg = new Focal(20, 10);
        Focal n0 = new Focal(10, 40);

        Assert.AreEqual(new PRange(0, 3), n0.GetRangeWithBasis(bPos, false, true));
        Assert.AreEqual(new PRange(0, -3, false), n0.GetRangeWithBasis(bPos, false, false));
        Assert.AreEqual(new PRange(-1, -2), n0.GetRangeWithBasis(bNeg, false, true));
        Assert.AreEqual(new PRange(1, 2, false), n0.GetRangeWithBasis(bNeg, false, false));
    }
    [TestMethod]
    public void SetWithRangeAndBasisTests()
    {
        Focal bPos = new Focal(10, 20);
        Focal bNeg = new Focal(20, 10);

        PRange r = new PRange(0, 3, true);
        Focal test = new Focal(0, 0);
        test.SetWithRangeAndBasis(r, bPos, false);
        Assert.AreEqual(new Focal(10, 40), test);

        r = new PRange(2, 5, true);
        test.SetWithRangeAndBasis(r, bPos, false);
        Assert.AreEqual(new Focal(-10, 60), test);

        r = new PRange(0, 3, true);
        test.SetWithRangeAndBasis(r, bNeg, false);
        Assert.AreEqual(new Focal(20, -10), test);

        r = new PRange(2, 5, true);
        test.SetWithRangeAndBasis(r, bNeg, false);
        Assert.AreEqual(new Focal(40, -30), test);

        r = new PRange(0, 3, false);
        test.SetWithRangeAndBasis(r, bPos, false);
        Assert.AreEqual(new Focal(10, -20), test);

        r = new PRange(2, 5, false);
        test.SetWithRangeAndBasis(r, bPos, false);
        Assert.AreEqual(new Focal(30, -40), test);

        r = new PRange(0, 3, false);
        test.SetWithRangeAndBasis(r, bNeg, false);
        Assert.AreEqual(new Focal(20, 50), test);

        r = new PRange(2, 5, false);
        test.SetWithRangeAndBasis(r, bNeg, false);
        Assert.AreEqual(new Focal(0, 70), test);

    }
    [TestMethod]
    public void InvertedBasisTests()
    {
        Focal bPos = new Focal(10, 20);
        Focal bNeg = new Focal(20, 10);

        PRange r = new PRange(0, 3, false);
        Focal test = new Focal(0, 0);
        test.SetWithRangeAndBasis(r, bPos, false);
        Assert.AreEqual(new Focal(10, -20), test);
    }

    [TestMethod]
    public void BasisSignTests()
    {
        Focal fb = new Focal(0, 10);
        Focal fn = new Focal(20, 30);
        Focal ftest = new Focal(0, 0);

        // focal [0,10]  fn [20, 30]
        Assert.AreEqual(new PRange(-2, 3), fn.GetRangeWithBasis(fb, false, true));
        ftest.SetWithRangeAndBasis(new PRange(-2, 3), fb, false);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);

        Assert.AreEqual(new PRange(-20, 30), fn.GetRangeWithBasis(fb, true, true));
        ftest.SetWithRangeAndBasis(new PRange(-20, 30), fb, true);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);


        fb.EndPosition = -10; // focal [0,-10]  fn [40, 30]

        Assert.AreEqual(new PRange(2, -3), fn.GetRangeWithBasis(fb, false, true));
        ftest.SetWithRangeAndBasis(new PRange(2, -3), fb, false);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);

        Assert.AreEqual(new PRange(20, -30, true), fn.GetRangeWithBasis(fb, true, true));
        ftest.SetWithRangeAndBasis(new PRange(20, -30, true), fb, true);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);


        fn.StartPosition = 40; // focal [0,-10],  fn [40, 30]

        Assert.AreEqual(new PRange(4, -3), fn.GetRangeWithBasis(fb, false, true));
        ftest.SetWithRangeAndBasis(new PRange(4, -3), fb, false);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);

        Assert.AreEqual(new PRange(40, -30), fn.GetRangeWithBasis(fb, true, true));
        ftest.SetWithRangeAndBasis(new PRange(40, -30), fb, true);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);


        fb.EndPosition = 10; // focal [0,10],  fn [40, 30]

        Assert.AreEqual(new PRange(-4, 3), fn.GetRangeWithBasis(fb, false, true));
        ftest.SetWithRangeAndBasis(new PRange(-4, 3), fb, false);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);

        Assert.AreEqual(new PRange(-40, 30), fn.GetRangeWithBasis(fb, true, true));
        ftest.SetWithRangeAndBasis(new PRange(-40, 30), fb, true);
        Assert.AreEqual(ftest.StartPosition, fn.StartPosition);
        Assert.AreEqual(ftest.EndPosition, fn.EndPosition);
    }
    [TestMethod]
    public void ReciprocalTests()
    {
        Focal f0 = new Focal(0, 10);
        Focal f1 = new Focal(132, 287);
        Assert.AreEqual(-13.2, f1.GetRangeWithBasis(f0, false, true).Start);
        Assert.AreEqual(28.7, f1.GetRangeWithBasis(f0, false, true).End);
        Assert.AreEqual(-130, f1.GetRangeWithBasis(f0, true, true).Start);
        Assert.AreEqual(290, f1.GetRangeWithBasis(f0, true, true).End);
    }

    [TestMethod]
    public void RangeToUnitTests()
    {
        Focal fb = new Focal(0, 10);
        Focal fn = new Focal(20, 30);
        Focal testFocal = new Focal(0, 0);
        var range = fn.GetRangeWithBasis(fb, false, true);
        testFocal.SetWithRangeAndBasis(range, fb, false);
        Assert.AreEqual(testFocal.StartPosition, fn.StartPosition);
        Assert.AreEqual(testFocal.EndPosition, fn.EndPosition);

    }

    [TestMethod]
    public void CoreFocalTests()
    {
        Focal f = new Focal(150, 250);
        Assert.AreEqual(MathElementKind.Focal, f.Kind);
        Assert.AreEqual(150, f.StartPosition);
        Assert.AreEqual(250, f.EndPosition);
        Assert.AreEqual(100, f.LengthInTicks);
        Assert.AreEqual(1, f.Direction);
        Assert.AreEqual(100, f.NonZeroLength);
        Assert.AreEqual(100, f.AbsLengthInTicks);
        var f2 = f.Clone();
        Assert.AreNotEqual(f.Id, f2.Id);
        Assert.AreEqual(f2.StartPosition, f.StartPosition);
        Assert.AreEqual(f.EndPosition, f2.EndPosition);

        Assert.AreEqual(new PRange(0, 1), f2.GetRangeWithBasis(f, false, true));

        f2.StartPosition = 450;
        Assert.AreEqual(450, f2.StartPosition);
        Assert.AreEqual(250, f2.EndPosition);
        Assert.AreEqual(-200, f2.LengthInTicks);
        Assert.AreEqual(-1, f2.Direction);
        Assert.AreEqual(-200, f2.NonZeroLength);
        Assert.AreEqual(200, f2.AbsLengthInTicks);

        // f:[150,250] f2[450,250]
        Assert.AreEqual(new PRange(-3, 1), f2.GetRangeWithBasis(f, false, true));
        Assert.AreEqual(new PRange(-3, 1), f.RangeAsBasis(f2));
        Assert.AreEqual(new PRange(-1.5, 1), f2.RangeAsBasis(f));
        Assert.AreEqual(new PRange(-1.5, 1), f.GetRangeWithBasis(f2, false, true));
        // f:[150,250] f2[500,250]
        f2.StartPosition = 500;
        Assert.AreEqual(new PRange(-3.5, 1), f2.GetRangeWithBasis(f, false, true));
        Assert.AreEqual(new PRange(-3.5, 1), f.RangeAsBasis(f2));
        Assert.AreEqual(new PRange(-1.4, 1), f2.RangeAsBasis(f));
        Assert.AreEqual(new PRange(-1.4, 1), f.GetRangeWithBasis(f2, false, true));

        // f:[150,250] f2[-150,250]
        f2.StartPosition = -150;
        Assert.AreEqual(new PRange(3, 1), f2.GetRangeWithBasis(f, false, true));
        Assert.AreEqual(new PRange(-0.75, 1), f.GetRangeWithBasis(f2, false, true));

        // f:[150,250] f2[-150,-150]
        f2.EndPosition = -150;
        Assert.AreEqual(0, f2.LengthInTicks);
        Assert.AreEqual(1, f2.NonZeroLength);
        Assert.AreEqual(0, f2.AbsLengthInTicks);

    }
}
