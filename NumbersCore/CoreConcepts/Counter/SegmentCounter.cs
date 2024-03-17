namespace NumbersCore.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.Primitives;

    public class SegmentCounter : CounterDomain
    {
        public Number Segment { get; }
        public long Value => Segment.Focal.EndPosition;
        public SegmentCounter(long min, long max) : base(Focal.CreateZeroFocal(1), new Focal(min, max), "Segment")
        {
            Segment = new Number(new Focal(min, min));
            AddNumber(Segment, true);
            Reset();
        }
        public void SetValue(long value)
        {
            SetAndClamp(Segment, Segment.Focal.StartPosition, value);
        }
    }
}
