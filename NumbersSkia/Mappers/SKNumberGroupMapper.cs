using Numbers.Agent;
using NumbersCore.Primitives;
using SkiaSharp;

namespace Numbers.Mappers;

using Numbers.Drawing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SKNumberGroupMapper : SKNumberMapper
{
    public NumberGroup NumberGroup => (NumberGroup)MathElement;
    public List<SKNumberMapper> NumberMappers { get; } = new List<SKNumberMapper>();

    public SKNumberGroupMapper(MouseAgent agent, NumberGroup numberSet) : base(agent, numberSet)
    {
    }

    private List<SKSegment> _segPaths = new List<SKSegment>();
    public override void DrawNumber(float offset)
    {
        EnsureSegment();
        var isSelected = IsSelected();
        _segPaths.Clear();
        foreach (var num in NumberGroup.InternalNumbers())
        {
            _segPaths.Add( DrawNumber(num, offset, isSelected) );
        }
    }

    public override SKPath GetHighlightAt(Highlight highlight)
    {
        var path = new SKPath();
        foreach (var segment in _segPaths) // todo: highlights should involve all segments if one is selected?
        {
            var (pt0, pt1) = segment.PerpendicularLine(0, 0);
            var ptDiff = pt1 - pt0;
            path.AddPoly([segment.StartPoint + ptDiff, segment.EndPoint + ptDiff,
                      segment.EndPoint - ptDiff, segment.StartPoint - ptDiff], true);
        }
        return path;
    }

    public override void Draw() // drawn by domain with offset
    {
    }
}
