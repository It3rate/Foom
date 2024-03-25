using System;
using System.Collections.Generic;
using System.Linq;
using Numbers;
using Numbers.Agent;
using Numbers.Commands;
using Numbers.Drawing;
using Numbers.Mappers;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace MathDemo;

public class Demos : DemoBase
{
	    private Brain Brain { get; }

    public Demos(Brain brain)
    {
	        Brain = brain;
        Pages.AddRange(new PageCreator[]
        {
            test2DMult,
            testOneLine,
            test2,
            test3,
            testMult,
        });
    }

    private SKWorkspaceMapper test2DMult()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 300, 0, 600, 600);
        var hDomain = Domain.CreateDomain("test2DMult", 100, 10);
        var vDomain = Domain.CreateDomain("test2DMult", 100, 10);
        var hNum = hDomain.CreateNumberFromFloats(2, 9);
        var vNum = vDomain.CreateNumberFromFloats(3, 6);

        Transform transform = Brain.AddTransform(hNum, vNum, OperationKind.Multiply);

        wm.AddHorizontal(hDomain);
        wm.AddVertical(vDomain);

        CreateSimilarDomain(hDomain, 1f, 20, hNum.Focal);
        CreateSimilarDomain(hDomain, 1.08f, 20, vNum.Focal);
        CreateSimilarDomain(hDomain, 1.2f, 100, transform.Result.Focal);

        return wm;
    }

    private SKWorkspaceMapper testMult()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);

        string[] txt = new string[] {
         "All things are random, ordered or a combination.",
        "Many things have order we can't distinguish, unseen patterns, or differences to small or large to measure.",
        "Random is simple, ordered is math. Distinguishing is precision."
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hDomain = CreateLowResDomain(8, 0);
        hDomain.ShowNumbersOffset = true;

        var hNum = hDomain.CreateNumberFromFloats(0, -1);
        hNum.Number.Polarity = Polarity.Inverted;

        var vNum = hDomain.CreateNumberFromFloats(0, 1.25f);

        Transform transform = Brain.AddTransform(hNum.Number, vNum.Number, OperationKind.Multiply);
        var tm = wm.GetOrCreateTransformMapper(transform, false);
        tm.Guideline = wm.TopSegment;
        hDomain.Domain.AddNumber(transform.Result);

        for (int i = 0; i < 15; i++)
        {
            transform = Brain.AddTransform(hNum.Number, transform.Result, OperationKind.Multiply);
            tm = wm.GetOrCreateTransformMapper(transform, false);
            tm.Guideline = wm.TopSegment;
            hDomain.Domain.AddNumber(transform.Result);
        }
        wm.Workspace.AddDomains(true, hDomain.Domain);
        return wm;
    }
    private SKWorkspaceMapper testMultX()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);

        string[] txt = new string[] {
         "All things are random, ordered or a combination.",
        "Many things have order we can't distinguish, unseen patterns, or differences to small or large to measure.",
        "Random is simple, ordered is math. Distinguishing is precision."
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hDomain = CreateLowResDomain(4, 0);// Domain.CreateDomain("test0", unitSize, 16);
        var vDomain = CreateLowResDomain(4, .1f);// Domain.CreateDomain("test0", unitSize, 16);
        var mDomain = CreateLowResDomain(4, .3f);
        mDomain.ShowNumbersOffset = true;

        vDomain.BasisNumber.Focal = hDomain.BasisNumber.Focal;
        mDomain.BasisNumber.Focal = hDomain.BasisNumber.Focal;
        var hNum = hDomain.CreateNumberFromFloats(0, -1);
        hNum.Number.Polarity = Polarity.Inverted;

        var vNum = vDomain.CreateNumberFromFloats(0, 1.25f);

        Transform transform = Brain.AddTransform(hNum.Number, vNum.Number, OperationKind.Add);
        var tm = wm.GetOrCreateTransformMapper(transform);
        tm.Do2DRender = false;
        var mNum = mDomain.Domain.AddNumber(transform.Result);

        wm.Workspace.AddDomains(true, hDomain.Domain, vDomain.Domain, mDomain.Domain);
        return wm;
    }

    private SKWorkspaceMapper test3()
    {
	        Trait trait = Trait.CreateIn(Brain, "test3");
	        long unitSize = 64;
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 20, 20, 1000, 400);
        var guideline = new SKSegment(100,100,700,100);
        var unitSeg = guideline.SegmentAlongLine(0.4f, 0.6f);
        var dc = new AddSKDomainCommand(trait, 0, unitSize, -800, 800, guideline, unitSeg, "demoTest3_1");
        _currentMouseAgent.Stack.Do(dc);
        var num = new AddSKNumberCommand(dc.DomainMapper, new PRange(-1.5, 2.4));
        _currentMouseAgent.Stack.Do(num);
        num = new AddSKNumberCommand(dc.DomainMapper, new PRange(.5, -1.2));
        _currentMouseAgent.Stack.Do(num);

        var guideline2 = new SKSegment(100, 200, 700, 200);
        var dc2 = new AddSKDomainCommand(trait, 0, unitSize, -800, 800, guideline2, unitSeg, "demoTest3_2");
        _currentMouseAgent.Stack.Do(dc2);
        var num2 = new AddSKNumberCommand(dc2.DomainMapper, new PRange(-1.2, -1.4));
        num2.DefaultDelay = -600;
        _currentMouseAgent.Stack.Do(num2);

        var guideline3 = new SKSegment(100, 300, 700, 300);
        var dc3 = new AddSKDomainCommand(trait, 0, unitSize, -800, 800, guideline3, unitSeg, "demoTest3_3");
        _currentMouseAgent.Stack.Do(dc3);
        var numSet = new NumberChain(dc3.Domain.MinMaxNumber, new[] { new Focal(5, 20), new Focal(-20, -10), new Focal(-40, -30) });
        dc3.CreatedDomain.AddNumberSet(numSet);

        return wm;
    }

    private SKWorkspaceMapper test2()
    {
	        Trait trait = Trait.CreateIn(Brain, "test2");
        var unitSize = 10;
        var unit = new Focal(0, unitSize);
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 10, 50, 1200, 600);
        var domains = CreateDomainLines((MouseAgent)_currentMouseAgent, trait, unit, 15, 10, -40, -30, 35, 24, 4, -13);
        var d2 = domains[2];
        var d1 = domains[1];
        var d1n2 = d1.GetNumber(d1.NumberIds()[2]);
        var nn =d2.CreateNumber(d1n2.Focal);
        _currentMouseAgent.Workspace.AddElements(nn);
        return wm;
    }
    private SKWorkspaceMapper testOneLine()
    {
	        Trait trait = Trait.CreateIn(Brain, "testOneLine");
        var unitSize = 4;
        var basis = new Focal(3, 3 + unitSize);
        var range = new Focal(-40, 40);
        var domain = trait.AddDomain(basis, range, "oneLineTest");
        domain.BasisNumber.Polarity = Polarity.Inverted;
        //var domain2 = t0.AddDomain(unit.Id, range.Id);
        var val2 = new Focal(-15, 30);
        //var val3 = new Focal(t0, -40, 60);
        //var val2 = new Focal(t0, unitSize, unitSize);
        //var val3 = new Focal(t0, unitSize, unitSize);

        var num2 = domain.CreateNumber(val2);
        //var num3 = new Number(domain2, val3.Id);
        //var sel = new Source(num2);
        //var transform = t0.AddTransform(sel, num3, OperationKind.Blend);

        _currentMouseAgent.Workspace.AddDomains(true, domain);//, domain2);
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 20, 20, 800, 800);
        var dm = wm.GetOrCreateDomainMapper(domain, wm.GetHorizontalSegment(.3f, 100));
        dm.ShowGradientNumberLine = true;
        dm.ShowNumbersOffset = true;
        dm.ShowBasisMarkers = true;
        dm.ShowBasis = true;
        wm.EnsureRenderers();
        //var nm = dm.NumberMapper(num2.Id);
        //dm.EndPoint += new SKPoint(0, -50);
        return wm;
    }

    private SKDomainMapper CreateLowResDomain(int rangeSize, float offset, params Focal[] focals)
    {
        var newDomain = Domain.CreateDomain("lowres", 4, rangeSize);
        foreach (var focal in focals)
        {
            newDomain.CreateNumber(focal);
        }
        var wm = _currentMouseAgent.WorkspaceMapper;
        var result = wm.AddDomain(newDomain, offset, true);
        result.ShowGradientNumberLine = true;
        result.ShowBasis = true;
        result.ShowBasisMarkers = true;
        result.ShowMinorTicks = true;
        return result;
    }
    private SKDomainMapper CreateSimilarDomain(Domain domain, float offset, int rangeSize, params Focal[] focals)
    {
        var newDomain = Domain.CreateDomain(domain.Trait.Name, (int)domain.BasisFocal.LengthInTicks, rangeSize);
        foreach (var focal in focals)
        {
            newDomain.CreateNumber(focal);
        }
        var result = _currentMouseAgent.WorkspaceMapper.AddDomain(newDomain, offset, true, -200);
        result.ShowBasis = true;
        result.ShowBasisMarkers = true;
        result.ShowMinorTicks = false;
        return result;
    }
    /// <summary>
    /// Create lines with focal pairs.
    /// </summary>
    private List<Domain> CreateDomainLines(MouseAgent mouseAgent, Trait trait, Focal basisFocal, params long[] focalPositions)
    {
	        var result = new List<Domain>();
	        var wm = mouseAgent.WorkspaceMapper;
	        var padding = 1.4;
	        long maxPos = (long)Math.Max((focalPositions.Max() * padding), basisFocal.AbsLengthInTicks * padding);
	        long minPos = (long)Math.Min((focalPositions.Min() * padding), -basisFocal.AbsLengthInTicks * padding);
	        var range = new Focal(minPos, maxPos);
	        var rangeLen = (double)range.LengthInTicks;
	        var yt = 0.1f;
	        var ytStep = (float)(0.8 / Math.Floor(focalPositions.Length / 2.0));
		    //var domain = trait.AddDomain(basisFocal, range);
		    ////domain.BasisIsReciprocal = true;
		    //result.Add(domain);
	        for (int i = 1; i < focalPositions.Length; i += 2)
	        {
		        var focal = new Focal(focalPositions[i - 1], focalPositions[i]);

		        var domain = trait.AddDomain(basisFocal.Clone(), focal, "demoDomain"); // clone to avoid duplicate focals.
		        //domain.BasisIsReciprocal = true;
		        result.Add(domain);

		        var num = domain.CreateNumber(focal);
		        mouseAgent.Workspace.AddDomains(true, domain);
		        var displaySeg = wm.GetHorizontalSegment(yt, 100);
		        var y = displaySeg.StartPoint.Y;

		        var sz = domain.BasisIsReciprocal ? 0.01f : 0.05f;
		        var unitSeg = displaySeg.SegmentAlongLine(0.5f - sz, 0.5f + sz);
		        //var unitSeg = new SKSegment((float)unitStart, y, (float)unitStart + 20f, y);
            var dm = wm.GetOrCreateDomainMapper(domain, displaySeg, unitSeg);
		        dm.ShowGradientNumberLine = true;
		        dm.ShowNumbersOffset = true;
		        dm.ShowBasisMarkers = true;
		        dm.ShowBasis = true;
		        yt += ytStep;
	        }

	        return result;
    }

}
