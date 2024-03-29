using Numbers.Agent;
using NumbersCore.Primitives;
using SkiaSharp;

namespace Numbers.Mappers;

using Numbers.Drawing;
using System.Collections.Generic;

public class SKNumberGroupMapper : SKNumberMapper
{
    public NumberGroup NumberGroup => (NumberGroup)MathElement;
    public SKDomainMapper DomainMapper => WorkspaceMapper.GetDomainMapper(NumberGroup.Domain);
    public List<SKNumberMapper> NumberMappers { get; } = new List<SKNumberMapper>();

    public SKNumberGroupMapper(MouseAgent agent, NumberGroup numberSet) : base(agent, numberSet)
    {
    }

    public void EnsureNumberMappers()
    {
        if (NumberGroup.Count > NumberMappers.Count)
        {
            for (int i = NumberMappers.Count; i < NumberGroup.Count; i++)
            {
                NumberMappers.Add(new SKNumberMapper(Agent, NumberGroup[i]));
            }
        }
        else if ((NumberGroup.Count < NumberMappers.Count))
        {
            NumberMappers.RemoveRange(NumberGroup.Count, NumberMappers.Count - NumberGroup.Count);
        }

        for (int i = 0; i < NumberMappers.Count; i++)
        {
            NumberMappers[i].ResetNumber(NumberGroup[i]);
        }
    }

    public void DrawNumberSet()
    {
        EnsureNumberMappers();
        foreach (var skNumberMapper in NumberMappers)
        {
            DomainMapper.DrawNumber(skNumberMapper, 0f, false);
        }
    }
    public override SKPath GetHighlightAt(Highlight highlight)
    {
        var result = new SKPath();
        foreach (var skNumberMapper in NumberMappers)
        {
            var path = skNumberMapper.GetHighlightAt(highlight);
            result.AddPath(path);
        }
        return result;
    }

    public override void Draw()
    {
    }
}
