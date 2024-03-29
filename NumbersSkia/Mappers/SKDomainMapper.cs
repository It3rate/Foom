using Numbers.Agent;
using Numbers.Drawing;
using NumbersCore.Primitives;
using NumbersCore.Utils;
using SkiaSharp;

namespace Numbers.Mappers;

public class SKDomainMapper : SKMapper
{
    public Domain Domain => (Domain)MathElement;
    public SKNumberMapper BasisNumberMapper => NumberMapperFor(Domain.BasisNumber);
    public Number BasisNumber => Domain.BasisNumber;
    public SKSegment BasisSegment => BasisNumberMapper.Guideline;
    public SKSegment InvertedBasisSegment => BasisNumberMapper.InvertedGuideline;
    public SKSegment BasisSegmentForNumber(Number num) => num.IsAligned ? BasisSegment : InvertedBasisSegment;
    public int BasisNumberSign => BasisNumber.Direction;

    public List<SKPoint> TickPoints = new List<SKPoint>();
    public List<int> ValidNumberIds = new List<int>();
    public PRange DisplayLineRange => BasisSegment.RatiosAsBasis(Guideline);
    public PRange UnitRangeOnDomainLine
    {
        get => Guideline.RatiosAsBasis(BasisSegment);
        set => BasisNumberMapper.Reset(SegmentAlongGuideline(value));
    }
    public int UnitDirectionOnDomainLine => BasisSegment.DirectionOnLine(Guideline);

    public bool ShowInfoOnTop = true;
    public bool ShowGradientNumberLine = true;
    public bool ShowPolarity = true;
    public bool ShowBasis = true;
    public bool ShowBasisMarkers;
    public bool ShowTicks = true;
    public bool ShowMinorTicks = true;
    public bool ShowValueMarkers = true;
    public bool ShowFractions = true;
    public bool ShowMaxMinValues = false;
    public bool ShowNumbersOffset = false;
    public bool ShowSeparatedSegment = false;
    public bool ShowTickMarkerValues = false;
    public int ShowNumberValuesMax = 5;

    public string Label { get; set; } = "";

    private int _numberOrderCounter = 0;

    public SKDomainMapper(MouseAgent agent, Domain domain, SKSegment guideline, SKSegment unitSegment) : base(agent, domain, guideline)
    {
        var unit = Domain.BasisNumber;
        var unitMapper = NumberMapperFor(unit);
        unitMapper.Guideline.Reset(guideline.ProjectPointOnto(unitSegment.StartPoint), guideline.ProjectPointOnto(unitSegment.EndPoint));
    }
    public SKDomainMapper ShowAll()
    {
        ShowInfoOnTop = true;
        ShowGradientNumberLine = true;
        ShowPolarity = true;
        ShowBasis = true;
        ShowBasisMarkers = true;
        ShowTicks = true;
        ShowMinorTicks = true;
        ShowValueMarkers = true;
        ShowFractions = true;
        //ShowMaxMinValues = true;
        //ShowNumbersOffset = true;

        return this;
    }
    public void FlipRenderPerspective()
    {
        Guideline.Reverse();
        BasisSegment.FlipAroundStartPoint();
    }

    #region NumberMappers
    protected Dictionary<int, SKNumberMapper> _numberMappers = new Dictionary<int, SKNumberMapper>();
    public IEnumerable<SKNumberMapper> NumberMappers()
    {
        foreach (var nm in _numberMappers.Values)
        {
            yield return nm;
        }
    }
    public IEnumerable<SKNumberMapper> OrderedValidNumbers()
    {
        var numbers = new List<SKNumberMapper>();
        foreach (var numberId in ValidNumberIds)
        {
            numbers.Add(NumberMapperFor(numberId));
        }
        numbers.Sort((x, y) => x.OrderIndex.CompareTo(y.OrderIndex));
        foreach (var nm in numbers)
        {
            yield return nm;
        }
    }
    public SKNumberMapper NumberMapperFor(Number number) => GetOrCreateNumberMapper(number);
    public SKNumberMapper NumberMapperFor(int numId) => GetOrCreateNumberMapper(Domain.GetNumber(numId));

    //protected Dictionary<int, SKNumberGroupMapper> NumberSetMappers = new Dictionary<int, SKNumberGroupMapper>();

    public SKNumberMapper AddNumberMapper(SKNumberMapper numberMapper)
    {
        _numberMappers[numberMapper.Number.Id] = numberMapper;
        numberMapper.Number.Domain = Domain;
        if (numberMapper.OrderIndex <= 0)
        {
            numberMapper.OrderIndex = _numberOrderCounter++;
        }
        return numberMapper;
    }
    public bool RemoveNumberMapper(SKNumberMapper numberMapper) => _numberMappers.Remove(numberMapper.Number.Id);
    public bool RemoveNumberMapperByIndex(int index)
    {
        var result = false;
        if (index >= 0 && index < _numberMappers.Count)
        {
            var nmId = _numberMappers.Keys.ElementAt(index);
            var nm = _numberMappers[nmId];
            Domain.RemoveNumber(nm.Number);
            result = _numberMappers.Remove(nmId);
        }
        return result;
    }
    public IEnumerable<SKNumberMapper> GetNumberMappers(bool reverse = false)
    {
        var mappers = reverse ? _numberMappers.Values.Reverse() : _numberMappers.Values;
        foreach (var dm in mappers)
        {
            yield return dm;
        }
    }
    public SKNumberMapper GetNumberMapper(int id)
    {
        _numberMappers.TryGetValue(id, out var result);
        return result;
    }
    public SKNumberMapper GetOrCreateNumberMapper(int id)
    {
        return GetOrCreateNumberMapper(Domain.NumberStore[id]);
    }
    public SKNumberMapper GetOrCreateNumberMapper(Number number)
    {
        if(number is NumberGroup ng){
            return GetOrCreateNumberGroupMapper(ng);
        }

        if (!_numberMappers.TryGetValue(number.Id, out var result))
        {
            result = new SKNumberMapper(Agent, number);
            AddNumberMapper(result);
        }
        return result;
    }
    public SKNumberMapper GetOrCreateNumberGroupMapper(NumberGroup numberGroup)
    {
        SKNumberGroupMapper result;
        if (_numberMappers.TryGetValue(numberGroup.Id, out SKNumberMapper? nm) && nm is SKNumberGroupMapper ngm)
        {
             result = ngm;
        }
        else
        { 
            result = new SKNumberGroupMapper(Agent, numberGroup);
            AddNumberMapper(result);
        }
        return result;
    }
    public SKNumberMapper LinkNumber(Number sourceNumber, bool addToStore = true)
    {
        var num = Domain.CreateNumber(sourceNumber.Focal, addToStore);
        return GetOrCreateNumberMapper(num);
    }
    public SKNumberMapper CreateNumber(Focal focal, bool addToStore = true)
    {
        var num = Domain.CreateNumber(focal, addToStore);
        return GetOrCreateNumberMapper(num);
    }
    public SKNumberMapper CreateNumber(PRange value, bool addToStore = true)
    {
        var num = Domain.CreateNumber(value, addToStore);
        return GetOrCreateNumberMapper(num);
    }
    public SKNumberMapper CreateNumber(long start, long end, bool addToStore = true)
    {
        var num = Domain.CreateNumber(start, end, addToStore);
        return GetOrCreateNumberMapper(num);
    }
    public SKNumberMapper CreateNumberFromFloats(float startF, float endF, bool addToStore = true)
    {
        var num = Domain.CreateNumberFromFloats(startF, endF, addToStore);
        return GetOrCreateNumberMapper(num);
    }
    public SKNumberMapper CreateInvertedNumberFromFloats(float startF, float endF, bool addToStore = true)
    {
        var num = Domain.CreateInvertedNumberFromFloats(startF, endF, addToStore);
        return GetOrCreateNumberMapper(num);
    }
    #endregion

    public override void Reset(SKPoint startPoint, SKPoint endPoint)
    {
        base.Reset(startPoint, endPoint);
        AddValidNumbers();
    }
    public void AlignDomainTo(SKDomainMapper other)
    {
        Domain.RemoveNumber(Domain.MinMaxNumber);
        Domain.BasisNumber.Focal = other.Domain.BasisNumber.Focal;
        Domain.MinMaxFocal = other.Domain.MinMaxFocal;

        var unit = other.Domain.BasisNumber;
        var unitMapper = NumberMapperFor(BasisNumber);
        var unitSegment = other.BasisSegment;
        unitMapper.Guideline.Reset(Guideline.ProjectPointOnto(unitSegment.StartPoint), Guideline.ProjectPointOnto(unitSegment.EndPoint));
    }
    public void Recalibrate()
    {
        var mmr = Domain.MinMaxRange;
        var basisGuide = BasisNumberMapper.Guideline;
        Guideline.Reset(basisGuide.PointAlongLine(-mmr.Start), basisGuide.PointAlongLine(mmr.End));
    }
    public SKSegment SegmentAlongGuideline(PRange ratio) => Guideline.SegmentAlongLine(ratio);
    public PRange RangeFromSegment(SKSegment segment)
    {
        var range = BasisSegment.RatiosAsBasis(segment);

        return range;
    }
    public SKPoint PointAt(double value)
    {
        double t = Domain.MinMaxRange.TAtValue(value);
        var pt = Guideline.PointAlongLine(t);
        return pt;
    }

    protected void AddValidNumbers()
    {
        ValidNumberIds.Clear();
        foreach (var num in Domain.Numbers())
        {
            if (num.Id != Domain.BasisNumber.Id && num.Id != Domain.MinMaxNumber.Id)
            {
                ValidNumberIds.Add(num.Id);
            }
        }
    }
    public void SetValueByKind(SKPoint newPoint, UIKind kind)
    {
        var unitRatio = UnitRangeOnDomainLine;
        if (kind.IsMajor())
        {
            EndPoint = newPoint;
        }
        else
        {
            StartPoint = newPoint;
        }
        BasisNumberMapper.Reset(SegmentAlongGuideline(unitRatio));
    }
    public void RotateGuidelineByPoint(SKPoint newPoint, UIKind kind)
    {
        var unitRatio = UnitRangeOnDomainLine;
        var center = Guideline.Midpoint;
        var dif = newPoint - center;
        var angle = (float)Math.Atan2(dif.Y, dif.X);
        Guideline.SetAngleAroundMidpoint(angle, 5);
        BasisNumberMapper.Reset(SegmentAlongGuideline(unitRatio));
    }

    public SKDomainMapper Duplicate(int offset = -1)
    {
        var domain = Domain.Duplicate();
        var result = WorkspaceMapper.GetOrCreateDomainMapper(domain);
        return result;
    }



    public override void Draw()
    {
        if (Domain != null && Domain.IsVisible)
        {
            AddValidNumbers();

            DrawNumberLine();
            DrawLabel();
            DrawUnit();
            DrawMarkers();
            DrawTicks();
            DrawNumbers();
        }
    }
    protected virtual void DrawNumberLine()
    {
        var renderDir = UnitDirectionOnDomainLine;
        var basisDir = BasisNumber.BasisFocal.Direction;
        var startToEnd = renderDir * basisDir == 1;
        var isSelected = Agent.ActiveDomainMapper?.Domain == Domain;
        if (ShowGradientNumberLine)
        {
            Renderer.DrawGradientNumberLine(Guideline, startToEnd, 10, isSelected);
        }
        else if (!ShowBasis) // if not showing units at least color the line
        {
            Renderer.DrawGradientNumberLine(Guideline, startToEnd, 3, isSelected);
        }
        Renderer.DrawSegment(Guideline, Renderer.Pens.NumberLinePen);
    }
    protected virtual void DrawLabel()
    {
        if (Label != null && Label != "")
        {
            Renderer.DrawText(Guideline.EndPoint + new SKPoint(10, 3), Label, Renderer.Pens.LabelBrush);
        }
    }
    protected virtual void DrawUnit()
    {
        if (ShowBasis)
        {
            NumberMapperFor(Domain.BasisNumber).DrawUnit(!ShowInfoOnTop, ShowPolarity);
        }
    }
    protected virtual void DrawMarkers()
    {
        if (ShowBasisMarkers)
        {
            var num = Domain.BasisNumber;
            DrawMarker(num, true);
            DrawMarker(num, false);
        }

        if (ShowValueMarkers)
        {
            var selId = -1;
            if (Agent.SelSelection.ActiveHighlight?.Mapper is SKNumberMapper snm)
            {
                selId = snm.Number.Id;
            }
            foreach (var id in ValidNumberIds)
            {
                var nm = Domain.GetNumber(id);
                if (ValidNumberIds.Count < ShowNumberValuesMax || selId == nm.Id)
                {
                    DrawMarker(nm, true);
                    DrawMarker(nm, false);
                }
            }
        }
    }
    public float startOffset = 6;
    protected virtual void DrawNumbers()
    {
        var showOffset = ShowNumbersOffset || ShowSeparatedSegment;

        var topDir = ShowInfoOnTop ? 1f : -1f;
        var offset = showOffset ? startOffset : 0f;
        offset *= topDir;
        var step = showOffset ? 12f : 0f;
        step *= topDir;
        foreach (var nm in OrderedValidNumbers())
        {
            nm.DrawNumber(offset + topDir * 2);
            //if (nm.Number is NumberGroup numberSet)
            //{
            //    var pts = new SKPoint[2];
            //    int index = 0;
            //    foreach (var num in numberSet.InternalNumbers())
            //    {
            //        //if (num.IsAligned)// || num.IsPositivePointing) // don't draw default false numbers (they point unit right, so negative).
            //        //{
            //        var numMap = new SKNumberMapper(Agent, num);
            //        nm.DrawNumber(offset + topDir * 2);
            //        if (index == 0)
            //        {
            //            //pts[0] = numMap.RenderSegment.StartPoint;
            //            index++;
            //        }
            //        //pts[1] = numMap.RenderSegment.EndPoint;
            //        //}
            //    }
            //    if (index > 0 && pts.Length > 1)
            //    {
            //       // nm.RenderSegment = new SKSegment(pts[0], pts[1]);
            //    }
            //}
            //else
            //{
            //    nm.DrawNumber(offset + topDir * 2);
            //}
            offset += step;
        }
    }

    protected virtual void DrawMarker(Number num, bool isStart)
    {
        var useStart = isStart == num.IsAligned;
        var t = useStart ? num.ValueInRenderPerspective.StartF : num.ValueInRenderPerspective.EndF;

        var txBaseline = DrawMarkerAndGetTextBaseline(num, t);

        var numPaint = isStart ? Pens.UnotMarkerText : Pens.UnitMarkerText; // i value should always be unot color, so use isStart vs useStart
        if (num.IsBasis)
        {
            if (!ShowTickMarkerValues)
            {
                var txt = useStart ? "0" : num.Domain.BasisIsReciprocal ? "" : "1"; // don't show 1 when ticks are larger than unit
                Renderer.DrawTextOnPath(txBaseline, txt, Pens.UnitMarkerText, Pens.TextBackgroundPen);
                if (!useStart && ShowPolarity)
                {
                    txBaseline = DrawMarkerAndGetTextBaseline(num, -1);
                    Renderer.DrawTextOnPath(txBaseline, "i", Pens.UnotMarkerText, Pens.TextBackgroundPen);
                }
            }
        }
        else
        {
            if (!(num.Focal.LengthInTicks == 0 && isStart) && !ShowTickMarkerValues) // don't draw twice for points
            {
                var (whole, frac) = GetFractionText(num, useStart);
                frac = ShowFractions ? frac : "";
                Renderer.DrawFraction((whole, frac), txBaseline, numPaint, Pens.TextBackgroundPen);
            }
        }
    }
    public void DrawEndFraction(Number num, bool aligned, SKPoint point)
    {
        var numPaint = aligned ? Pens.UnitMarkerText : Pens.UnotMarkerText;
        var (whole, frac) = GetFractionText(num, false);
        frac = ShowFractions ? frac : "";
        var txBaseline = new SKSegment(point, point + new SKPoint(10, 0));
        Renderer.DrawFraction((whole, frac), txBaseline, numPaint, Pens.TextBackgroundPen);
    }
    private SKSegment DrawMarkerAndGetTextBaseline(Number num, float t)
    {
        var domainIsUnitPersp = num.Domain.BasisFocal.IsPositiveDirection;
        var txBaseline = DrawMarkerPointer(t, num.IsBasis);
        var segAlignment = txBaseline.DirectionOnLine(Guideline);
        if (!domainIsUnitPersp || segAlignment == -1)
        {
            txBaseline.Reverse();
            txBaseline = txBaseline.ShiftOffLine(-6);
        }
        return txBaseline;
    }
    protected virtual SKSegment DrawMarkerPointer(float t, bool isBasis)
    {
        var wPos = 5f;
        var sign = UnitDirectionOnDomainLine;
        var wDir = wPos * sign;
        var isTop = ShowInfoOnTop ? -1f : 1f;
        var w = wDir * isTop;
        var unitSeg = BasisSegment;
        var markerHW = (float)(1.0 / BasisSegment.Length) * wPos;
        var pt = unitSeg.PointAlongLine(t);
        var ptMinus = unitSeg.PointAlongLine(t - markerHW);
        var ptPlus = unitSeg.PointAlongLine(t + markerHW);
        var p0 = unitSeg.OrthogonalPoint(pt, w * .8f);
        SKPoint textPoint0;
        SKPoint textPoint1;
        if (!isBasis)
        {
            var p1 = unitSeg.OrthogonalPoint(ptMinus, w * 3);
            var p2 = unitSeg.OrthogonalPoint(ptPlus, w * 3);
            Renderer.FillPolyline(Pens.MarkerBrush, p0, p1, p2, p0);
            //Renderer.DrawLine(p1, p2, Pens.Seg1TextBrush);
            textPoint0 = unitSeg.OrthogonalPoint(p1, isTop * 3f); // large pixel offset for text above marker
            textPoint1 = unitSeg.OrthogonalPoint(p2, isTop * 3f);
        }
        else
        {
            var offset = sign > 0 ? -10f : 4f;
            textPoint0 = unitSeg.OrthogonalPoint(ptMinus, offset);
            textPoint1 = unitSeg.OrthogonalPoint(ptPlus, offset);

        }

        return ShowInfoOnTop ? new SKSegment(textPoint0, textPoint1) : new SKSegment(textPoint1, textPoint0);
    }
    protected virtual void DrawTicks()
    {
        if (!ShowTicks || BasisSegment.AbsLength < 4)
        {
            return;
        }
        var topDir = ShowInfoOnTop ? 1f : -1f;
        var offsetRange = -8f;
        var tickCount = Domain.BasisNumber.AbsBasisTicks;
        var tickToBasisRatio = Domain.TickToBasisRatio;
        var upDir = UnitDirectionOnDomainLine;
        // basis ticks
        var rangeInBasis = DisplayLineRange.ClampInner();
        TickPoints.Clear();
        for (long i = (long)rangeInBasis.Min; i <= (long)rangeInBasis.Max; i++)
        {
            var isOnTick = Math.Abs(tickToBasisRatio) <= 1.0 ? true : (long)(i / tickToBasisRatio) * (long)tickToBasisRatio == i;
            var offset = isOnTick ? offsetRange : offsetRange / 8f;
            offset *= topDir;
            var tickPen = ShowMinorTicks ? Renderer.Pens.TickBoldPen : Renderer.Pens.TickPen;
            var startPt = DrawTick((float)i, offset * upDir, tickPen);
            TickPoints.Add(startPt);
            if (ShowTickMarkerValues)
            {
                var pt = new SKPoint(startPt.X + 4, startPt.Y - (upDir * 16));
                Renderer.DrawText(pt, i.ToString(), Pens.TextFractionPen);
            }
        }

        // minor ticks
        var totalTicks = Math.Abs(BasisSegment.Length / tickToBasisRatio);
        if (ShowMinorTicks)
        {
            var tickStep = BasisSegment.Length * 20 < totalTicks ? 0.1f : Math.Abs(tickToBasisRatio);
            var rangeInTicks = Domain.ClampToInnerTick(DisplayLineRange);
            var offset = Domain.BasisIsReciprocal ? offsetRange * 1.5f : offsetRange;
            offset *= topDir;
            for (var i = rangeInTicks.Min; i <= rangeInTicks.Max; i += tickStep)
            {
                if (i != 0) // don't draw tick on origin
                {
                    DrawTick((float)i, offset * upDir, Renderer.Pens.TickPen);
                }
            }
        }
    }
    protected virtual SKPoint DrawTick(float t, float offset, SKPaint paint)
    {
        var pts = BasisSegment.PerpendicularLine(t, offset);
        Renderer.DrawLine(pts.Item1, pts.Item2, paint);
        return pts.Item1;
    }

    protected (string, string) GetFractionText(Number num, bool isStart)
    {
        var isIValue = isStart == num.IsAligned;
        var suffix = ShowPolarity ? (isIValue ? "i" : "") : ""; // can use "r" or "n" for normal numbers
        var fraction = "";
        var val = isStart ? num.StartValue : num.EndValue;
        var whole = "0";
        if (val != 0)
        {
            var wholeNum = isStart ? (ShowPolarity ? num.WholeStartValue : -num.WholeStartValue) : num.WholeEndValue;
            //wholeNum = domainIsUnitPersp ? wholeNum : -wholeNum;
            var sign = wholeNum >= 0 ? "" : "-";// isStart ? (num.StartValue >= 0 ? "" : "-") : (num.EndValue >= 0 ? "" : "-");
            whole = wholeNum == 0 ? "" : wholeNum.ToString();
            // todo:!! need to rewrite unot perspective code. hack for now.
            var numerator = isStart ? num.RemainderStartValue : num.RemainderEndValue;
            if (num.AbsBasisTicks != 0 && numerator != 0)
            {
                fraction = " " + numerator.ToString() + "/" + num.AbsBasisTicks.ToString() + suffix;
            }
            else
            {
                whole += suffix;
            }

            if (whole == "")
            {
                fraction = sign + fraction;
            }
        }

        return (whole, fraction);
    }

    public override SKPath GetHighlightAt(Highlight highlight)
    {
        return Renderer.GetCirclePath(highlight.SnapPoint);
    }

    public override string ToString()
    {
        return "dm: Count " + _numberMappers.Count;
    }
}
