using NumbersSkia.Agent;
using NumbersSkia.Drawing;
using NumbersCore.Primitives;
using SkiaSharp;
using System;

namespace NumbersSkia.Mappers;

public class SKNumberMapper : SKMapper
{
    public Number Number => (Number)MathElement;
    public MouseAgent MouseAgent => (MouseAgent)Agent;
    public virtual SKSegment RenderSegment { get; set; }

    public SKDomainMapper DomainMapper => WorkspaceMapper.GetDomainMapper(Number.Domain);
    public SKSegment UnitSegment => DomainMapper.BasisSegment;
    public bool IsBasis => Number.IsBasis;
    public int BasisSign => Number.BasisFocal.Direction;
    public SKSegment GetBasisSegment() => DomainMapper.BasisSegmentForNumber(Number);
    public Polarity Polarity { get => Number.Polarity; set => Number.Polarity = value; }
    public int UnitDirectionOnDomainLine => Guideline.DirectionOnLine(DomainMapper.Guideline);

    public int OrderIndex { get; set; } = -1;

    public SKNumberMapper(MouseAgent agent, Number number) : base(agent, number)
    {
        Id = number.Id;
    }


    public Polarity InvertPolarity()
    {
        return Number.InvertPolarity();
    }
    public void ResetNumber(Number number) => MathElement = number;
    public void EnsureSegment()
    {
        var val = Number.ValueInRenderPerspective;
        Reset(UnitSegment.SegmentAlongLine(val.StartF, val.EndF));
    }
    public event EventHandler OnSelected;
    public void OnSelect()
    {
        OnSelected?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler OnDeselected;
    public void OnDeselect()
    {
        OnDeselected?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler OnChanged;
    public void OnChange()
    {
        OnChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void Draw() { } // drawn by domain with offset

    protected bool IsSelected() => Agent.SelSelection.ActiveHighlight?.Mapper == this;
    public virtual void DrawNumber(float offset)
    {
        EnsureSegment();
        RenderSegment = DrawNumber(Number, offset, IsSelected());
    }

    // passing in number to allow rendering grouped numbers individually
    public SKSegment DrawNumber(Number num, float offset, bool isSelected)
    {
        var pen = isSelected ? Pens.SegPenHighlight : Pens.SegPens[Number.StoreIndex % Pens.SegPens.Count];
        var result = DrawNumberStroke(num, offset, pen); // background

        if (num.IsAligned)
        {
            var invPen = DomainMapper.ShowPolarity ? Pens.UnotInlinePen : Pens.UnitInlinePen;
            DrawNumberStroke(num, offset, Pens.UnitInlinePen, invPen);
        }
        else
        {
            var invPen = DomainMapper.ShowPolarity ? Pens.UnitInlinePen : Pens.UnotInlinePen;
            DrawNumberStroke(num, offset, Pens.UnotInlinePen, invPen);
        }
        return result;
    }
    protected bool _drawNumStartCap = true;
    protected bool _drawNumEndCap = true;
    protected SKSegment DrawNumberStroke(Number num, float offset, SKPaint paint, SKPaint invertPaint = null)
    {
        SKSegment result;
        var pen2 = invertPaint ?? paint;
        if (DomainMapper.ShowSeparatedSegment)
        {
            var val = num.ValueInRenderPerspective;
            var segEndDir = val.EndF >= 0 ? 1 : -1;
            var endSeg = UnitSegment.SegmentAlongLine(0, val.EndF).ShiftOffLine((offset + 10) * segEndDir);
            Renderer.DrawHalfLine(endSeg, paint);

            var segStatDir = val.StartF >= 0 ? 1 : -1;
            var startSeg = UnitSegment.SegmentAlongLine(0, val.StartF).ShiftOffLine((offset + 6) * segStatDir);
            Renderer.DrawHalfLine(startSeg, pen2);

            if(_drawNumEndCap){ Renderer.DrawEndCap(endSeg, paint); }
            if (_drawNumStartCap) { Renderer.DrawStartCap(startSeg, pen2); }

            result = new SKSegment(startSeg.EndPoint, endSeg.EndPoint);
        }
        else
        {
            result = GetFullRenderSegment(num, offset);
            Renderer.DrawDirectedLine(result, paint, pen2, _drawNumStartCap, _drawNumEndCap);
        }
        return result;
    }
    protected SKSegment GetFullRenderSegment(Number num, float offset)
    {
        var val = num.ValueInRenderPerspective;
        var dir = UnitDirectionOnDomainLine;
        return UnitSegment.SegmentAlongLine(val.StartF, val.EndF).ShiftOffLine(Guideline, offset * dir);
    }

    public void DrawUnit(bool aboveLine, bool showPolarity)
    {
        // BasisNumber is a special case where we don't want it's direction set by the unit direction of the line (itself).
        // So don't call EnsureSegment here.
        var dir = Number.Focal.Direction;
        var unitPen = Pens.UnitPenLight; // the basis defines the unit direction, so it is always unit color
        var offset = Guideline.OffsetAlongLine(0, unitPen.StrokeWidth / 2f * dir) - Guideline.StartPoint;
        RenderSegment = aboveLine ? Guideline + offset : Guideline - offset;
        if (Pens.UnitStrokePen != null)
        {
            Renderer.DrawSegment(RenderSegment, Pens.UnitStrokePen);
        }
        Renderer.DrawSegment(RenderSegment, unitPen);
        if (showPolarity)
        {
            var unotSeg = RenderSegment.Clone();
            unotSeg.FlipAroundStartPoint();
            Renderer.DrawSegment(unotSeg, Pens.UnotPenLight);
        }
    }

    public float TFromPoint(SKPoint point)
    {
        var basisSeg = GetBasisSegment();
        var pt = basisSeg.ProjectPointOnto(point, false);
        var (t, _) = basisSeg.TFromPoint(pt, false);
        t = (float)(Math.Round(t * basisSeg.Length) / basisSeg.Length);
        return t;
    }

    public void AdjustBySegmentChange(HighlightSet beginState) => AdjustBySegmentChange(beginState.OriginalSegment, beginState.OriginalFocal);
    public void AdjustBySegmentChange(SKSegment originalSegment, Focal originalFocal)
    {
        var change = originalSegment.RatiosAsBasis(Guideline);
        var ofp = originalFocal;
        Number.Focal.Reset(
            (long)(ofp.StartPosition + change.Start * ofp.LengthInTicks),
            (long)(ofp.EndPosition + (change.End - 1.0) * ofp.LengthInTicks));
    }

    public void SetValueByKind(SKPoint newPoint, UIKind kind)
    {
        if (kind.IsBasis())
        {
            SetValueOfBasis(newPoint, kind);
        }
        else if (kind.IsMajor())
        {
            SetEndValueByPoint(newPoint);
        }
        else
        {
            SetStartValueByPoint(newPoint);
        }
    }

    public void MoveSegmentByT(SKSegment orgSeg, float diffT)
    {
        var basisSeg = GetBasisSegment();
        var orgStartT = -basisSeg.TFromPoint(orgSeg.StartPoint, false).Item1;
        var orgEndT = basisSeg.TFromPoint(orgSeg.EndPoint, false).Item1;
        Number.StartValue = orgStartT - diffT;
        Number.EndValue = orgEndT + diffT;
    }
    public void MoveBasisSegmentByT(SKSegment orgSeg, float diffT)
    {
        var dl = DomainMapper.Guideline;
        var orgStartT = dl.TFromPoint(orgSeg.StartPoint, false).Item1;
        var orgEndT = dl.TFromPoint(orgSeg.EndPoint, false).Item1;
        Guideline.StartPoint = dl.PointAlongLine(orgStartT + diffT);
        Guideline.EndPoint = dl.PointAlongLine(orgEndT + diffT);
    }

    public void SetStartValueByPoint(SKPoint newPoint)
    {
        Number.StartValue = -TFromPoint(newPoint);
    }
    public void SetEndValueByPoint(SKPoint newPoint)
    {
        Number.EndValue = TFromPoint(newPoint);
    }
    public void SetValueOfBasis(SKPoint newPoint, UIKind kind)
    {
        var pt = DomainMapper.Guideline.ProjectPointOnto(newPoint);
        if (kind.IsMajor())
        {
            Guideline.EndPoint = pt;
        }
        else
        {
            Guideline.StartPoint = pt;
        }
    }

    public override SKPath GetHighlightAt(Highlight highlight)
    {
        SKPath result;
        if (highlight.Kind.IsLine())
        {
            result = Renderer.GetSegmentPath(RenderSegment, 0f);
        }
        else
        {
            result = Renderer.GetCirclePath(highlight.SnapPoint);
        }
        return result;
    }
    public override string ToString()
    {
        return "nm:" + Number.ToString();
    }
}
