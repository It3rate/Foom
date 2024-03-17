using Numbers.Agent;
using NumbersCore.Primitives;
using NumbersCore.Utils;
using SkiaSharp;

namespace Numbers.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Drawing;

    public class SKNumberSetMapper : SKMapper
    {
	    public NumberChain NumberSet => (NumberChain)MathElement;
	    public SKDomainMapper DomainMapper => WorkspaceMapper.GetDomainMapper(NumberSet.Domain);
        public List<SKNumberMapper> NumberMappers { get; } = new List<SKNumberMapper>();

        public SKNumberSetMapper(MouseAgent agent, NumberChain numberSet, SKSegment guideline = default) : base(agent, numberSet, guideline)
	    {
	    }

	    public void EnsureNumberMappers()
	    {
		    if (NumberSet.Count > NumberMappers.Count)
		    {
			    for (int i = NumberMappers.Count; i < NumberSet.Count; i++)
			    {
				    NumberMappers.Add(new SKNumberMapper(Agent, NumberSet[i]));
			    }
		    }
            else if ((NumberSet.Count < NumberMappers.Count))
		    {
                NumberMappers.RemoveRange(NumberSet.Count, NumberMappers.Count - NumberSet.Count);
		    }

		    for (int i = 0; i < NumberMappers.Count; i++)
		    {
			    NumberMappers[i].ResetNumber(NumberSet[i]);
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
}
