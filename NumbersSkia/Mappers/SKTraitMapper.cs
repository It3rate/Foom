using Numbers.Agent;
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
}
