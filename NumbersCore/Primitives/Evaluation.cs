namespace NumbersCore.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Flags]
    public enum EvalFlag
    {
        None = 0,
        Trait,
        Domain,
        BasisFocal,
        BasisOrigin,
        BasisResolution,
        MinMax,
        Length,
        HasOverlap,
        Equal,
        Contains,
        ContainedBy,
        WhollyGreater,
        WhollyLess,
        PartiallyGreater,
        PartiallyLess,
        Wrapped,
        Bounced,
        Overflowed,
    }
    public class Evaluation
    {
        // Flags:
        // same trait, domain, basis, resolution, minmax
        // direction (same as basis, same abs)
        // length
        // overlap
        // per endpoint - gt, lt, eq, per basis/abs
        // containment
        // same mod, whole
        // has wrapped/bounced/overflowed

        public Number Source { get; }
        public Number Target { get; }
        public NumberChain Result { get; }
        public FilterOperator FilterOperator { get; }
        public EvalFlag TestFlags { get; } = (EvalFlag) 0x7FFFFFFF;
        public EvalFlag ResultFlags { get; private set; }

        public Evaluation(Number source, Number target, FilterOperator filterOperator)
        {
	        Source = source;
	        Target = target;
            FilterOperator = filterOperator;
            Result = new NumberChain(target); // result should be calculated
        }

        /// <summary>
        /// Tests all the flags set in TestFlags, sets the ResultFlags to the set bits.
        /// </summary>
        /// <returns>Returns true if any of the flags are set.</returns>
        public bool EvaluateFlags()
        {
            ApplyFilter();
            // compare Numbers for the testFlag matches. (maybe just test all, then & with TestFlags)
            ResultFlags = EvalFlag.None;

            ResultFlags |= TargetContainsSource() ? EvalFlag.Contains : 0;

            return (int)ResultFlags > 0;
        }
        public void ApplyFilter()
        {
            switch (FilterOperator)
            {
                case FilterOperator.Never:
                    Source.Never(Target, Result);
                    break;
                case FilterOperator.And:
                    Source.And(Target, Result);
                    break;
                case FilterOperator.B_Inhibits_A:
                    Source.B_Inhibits_A(Target, Result);
                    break;
                case FilterOperator.Transfer_A:
                    Source.Transfer_A(Target, Result);
                    break;
                case FilterOperator.A_Inhibits_B:
                    Source.A_Inhibits_B(Target, Result);
                    break;
                case FilterOperator.Transfer_B:
                    Source.Transfer_B(Target, Result);
                    break;
                case FilterOperator.Xor:
                    Source.Xor(Target, Result);
                    break;
                case FilterOperator.Or:
                    Source.Or(Target, Result);
                    break;
                case FilterOperator.Nor:
                    Source.Nor(Target, Result);
                    break;
                case FilterOperator.Xnor:
                    Source.Xnor(Target, Result);
                    break;
                case FilterOperator.Not_B:
                    Source.Not_B(Target, Result);
                    break;
                case FilterOperator.B_Implies_A:
                    Source.B_Implies_A(Target, Result);
                    break;
                case FilterOperator.Not_A:
                    Source.Not_A(Target, Result);
                    break;
                case FilterOperator.A_Implies_B:
                    Source.A_Implies_B(Target, Result);
                    break;
                case FilterOperator.Nand:
                    Source.Nand(Target, Result);
                    break;
                case FilterOperator.Always:
                    Source.Always(Target, Result);
                    break;
            }
        }


        public bool TargetContainsSource() => Target?.FullyContains(Source) ?? false;
    }
}
