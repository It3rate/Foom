namespace Numbers.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Agent;
    using Numbers.Drawing;
    using NumbersCore.Primitives;

    public class SKNumberChainMapper : SKNumberMapper
    {
        public override SKSegment RenderSegment { get; set; }

        private List<SKSegment> _renderSegments = new List<SKSegment>();
        private List<SKNumberMapper> _activeMappers = new List<SKNumberMapper>();
        public SKNumberChainMapper(MouseAgent agent, NumberChain number) : base(agent, number)
        {
        }
    }
}
