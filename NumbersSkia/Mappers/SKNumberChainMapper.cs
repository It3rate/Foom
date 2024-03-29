namespace Numbers.Mappers;

using Numbers.Agent;
using Numbers.Drawing;
using NumbersCore.Primitives;
using System.Collections.Generic;

public class SKNumberChainMapper : SKNumberMapper
{
    public override SKSegment RenderSegment { get; set; }

    private List<SKSegment> _renderSegments = new List<SKSegment>();
    private List<SKNumberMapper> _activeMappers = new List<SKNumberMapper>();
    public SKNumberChainMapper(MouseAgent agent, NumberGroup number) : base(agent, number)
    {
    }
}
