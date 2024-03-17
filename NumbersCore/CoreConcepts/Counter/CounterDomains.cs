using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Spatial;
    using NumbersCore.Utils;

    /// <summary>
    /// This domain can characterize all counters (start, end?, step, events, increment/decrement methods, completion Tests).
    /// The domain can apply Increment() to all numbers it contains in one step.
    /// It can enforce the types of numbers that it can contain (maybe require them to have an Increment method?)
    /// </summary>
    public class CounterDomain : Domain
    {
        public static CounterDomain UpCounterDomain { get; } = new CounterDomain(Focal.CreateZeroFocal(1), new Focal(0, long.MaxValue), "CounterUp");
        public static CounterDomain UpDownDomain { get; } = new CounterDomain(Focal.CreateZeroFocal(1), Focal.MinMaxFocal, "CounterUpDown");

        public long Start => MinMaxFocal.StartPosition;
        public long End => MinMaxFocal.EndPosition;
        public long Step { get; set; } = 1; // step could be ticks of basis focal? Or initial number value, which is held separately internally. Should be focal at least.
        public CounterDomain(Focal basisFocal, Focal maxFocal, string name) : base(CounterTrait.Instance, basisFocal, maxFocal, name)
        {
            IsVisible = false;
            Reset();
        }

        public static CounterDomain CreateDomain(DomainScope df, string name, bool isVisible = true)
        {
            var domain = new CounterDomain(df.Basis, df.MinMax, name);
            return domain;
        }

        public long[] IncrementAll() => Increment(NumberStore.Values.ToArray());
        public long[] Increment(params Number[] numbers)
        {
            var result = new List<long>();
            foreach (var num in numbers)
            {
                SetAndClamp(num, num.Focal.StartPosition, num.Focal.EndPosition + Step);
                result.Add(num.Focal.EndPosition);
            }
            return result.ToArray();
        }
        public long[] DecrementAll() => Increment(NumberStore.Values.ToArray());
        public long[] Decrement(params Number[] numbers)
        {
            var result = new List<long>();
            foreach (var num in numbers)
            {
                SetAndClamp(num, num.Focal.StartPosition, num.Focal.EndPosition - Step);
                result.Add(num.Focal.EndPosition);
            }
            return result.ToArray();
        }
        public void SetAndClampAll(long start, long end)
        {
            var nums = NumberStore.Values.ToArray();
            foreach (var num in nums)
            {
                SetAndClamp(num, start, end);
            }
        }
         public void SetAndClamp(Number num, long start, long end)
        {
            start = (start > End) ? End : (start < Start) ? Start : start;
            end = (end > End) ? End : (end < Start) ? Start : end;
            num.Focal.StartPosition = start;
            num.Focal.EndPosition = end;
        }
        public override Number CreateDefaultNumber(bool addToStore = true)
        {
            var num = new Number(new Focal(0, 1));
            return AddNumber(num, addToStore);
        }

        public void Reset()
        {
            foreach (var num in NumberStore.Values)
            {
                if (!num.IsBasis && !num.IsMinMax)
                {
                    num.Focal.StartPosition = Start;
                    num.Focal.EndPosition = Start;
                }
            }
        }
    }
}
