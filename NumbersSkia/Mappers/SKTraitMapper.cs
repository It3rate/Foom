using Numbers.Agent;
using NumbersCore.Utils;
using SkiaSharp;

namespace Numbers.Mappers;

using Numbers.Drawing;
using System;

public class SKTraitMapper : SKMapper
{
    public SKTraitMapper(MouseAgent agent, IMathElement element, SKSegment guideline = default) : base(agent, element, guideline)
    {
    }

    public override SKPath GetHighlightAt(Highlight highlight)
    {
        throw new NotImplementedException();
    }
    public override void Draw()
    {
    }
}
