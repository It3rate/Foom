using NumbersSkia.Mappers;
using NumbersCore.Utils;
using SkiaSharp;

namespace NumbersSkia.Agent;

/// <summary>
/// Highlight is a sub-selection for UI purposes, can select parts of an equation to drag, like a orgPoint on a segment, or segment in a transform.
/// Can probably be merged with selection eventually, should be similar mechanisms to select with segments.
/// </summary>
public class Highlight
{
    public SKPoint OriginalPoint { get; set; } = SKPoint.Empty;
    public SKPoint _snapPoint = SKPoint.Empty;
    public PRange OriginalValue { get; set; }
    public SKPoint SnapPoint
    {
        get => _snapPoint.IsEmpty ? OriginalPoint : _snapPoint;
        set => _snapPoint = value;

    }
    public SKMapper Mapper { get; set; }

    public SKNumberMapper GetNumberMapper() => Mapper is SKNumberMapper nm ? nm : null;
    public SKDomainMapper GetRelatedDomainMapper()
    {
        SKDomainMapper result = null;
        if (Mapper is SKNumberMapper nm)
        {
            result = nm.DomainMapper;
        }
        else if (Mapper is SKDomainMapper dm)
        {
            result = dm;
        }
        return result;
    }
    public SKTransformMapper GetRelatedTransformMapper(SKWorkspaceMapper wm)
    {
        SKTransformMapper result = null;
        if (Mapper is SKNumberMapper nm)
        {
            result = wm.TransformMapperInvolving(nm.Number);
        }
        return result;
    }
    public float T { get; set; }
    public UIKind Kind { get; set; }
    //public Number RangeInMinMax { get; set; } // will set selection ranges with a length trait number eventually
    public bool IsSet => Mapper != null;

    public Highlight()
    {
    }
    private Highlight(SKPoint orgPoint, SKMapper mapper, float t, UIKind kind)
    {
        OriginalPoint = orgPoint;
        Mapper = mapper;
        T = t;
        Kind = kind;
    }

    public void Set(SKPoint orgPoint, SKPoint snapPoint, SKMapper mapper, float t, UIKind kind, PRange orgValue = default)
    {
        OriginalPoint = orgPoint;
        SnapPoint = snapPoint;
        Mapper = mapper;
        T = t;
        Kind = kind;
        OriginalValue = orgValue;
    }
    public void Reset()
    {
        OriginalPoint = SKPoint.Empty;
        OriginalValue = PRange.Empty;
        SnapPoint = SKPoint.Empty;
        Mapper = null;
        T = 0;
        Kind = UIKind.None;
    }

    public Highlight Clone()
    {
        var result = new Highlight(new SKPoint(OriginalPoint.X, OriginalPoint.Y), Mapper, T, Kind);
        result.OriginalValue = OriginalValue;
        return result;
    }

    public SKPath HighlightPath() => Mapper.GetHighlightAt(this);

}

[Flags]
public enum UIKind
{
    None = 0x00,
    Major = 0x01,
    Inverted = 0x02,

    Label = 0x10,
    Marker = 0x20,
    Tick = 0x40,
    Basis = 0x80,

    Number = 0x100,
    Domain = 0x200,
    Transform = 0x400,
    Shape = 0x800,

    Point = 0x1000,
    Line = 0x2000,
    Area = 0x4000,
    Volume = 0x8000,
}
public static class UIKindExtensions
{
    public static bool IsMajor(this UIKind kind)
    {
        return (kind & UIKind.Major) != UIKind.None;
    }
    public static bool IsBasis(this UIKind kind)
    {
        return (kind & UIKind.Basis) != UIKind.None;
    }
    public static bool IsAligned(this UIKind kind)
    {
        return (kind & UIKind.Inverted) == UIKind.None;
    }
    public static bool IsNumber(this UIKind kind)
    {
        return (kind & UIKind.Number) != UIKind.None;
    }
    public static bool IsLine(this UIKind kind)
    {
        return (kind & UIKind.Line) != UIKind.None;
    }
    public static bool IsDomain(this UIKind kind)
    {
        return (kind & UIKind.Domain) != UIKind.None;
    }
    public static bool IsDomainPoint(this UIKind kind)
    {
        return kind.IsDomain() && (kind & UIKind.Point) != UIKind.None;
    }
    public static bool IsMinorTick(this UIKind kind)
    {
        return !kind.IsMajor() && (kind & UIKind.Tick) != UIKind.None;
    }
    public static bool IsBoldTick(this UIKind kind)
    {
        return kind.IsMajor() && (kind & UIKind.Tick) != UIKind.None;
    }
}
