using System.Collections.Generic;
using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
	public class Formula : IMathElement
    {
        public MathElementKind Kind => MathElementKind.Formula;
        public int Id { get; set; }
        public int CreationIndex => Id - (int)Kind - 1;
        public Brain Brain { get; }
        public bool IsDirty { get; set; } = true;

        // Formula needs a list to select from, and a result stack. Then all sel/change/rep/eval come from these collections?

        //   private Number _repeatIndex; // need a trait in the dictionary that just represents values
        //   public Number Selection { get; } // find multiplicand(s) on stack. -n from end, n from start, 0 is all?
        //   public Number Change { get; } // find multiplier on stack, or use number as multiplier (need flag?)
        //   public Number Repeats { get; } // iterations (also lookup or set)
        //   public Number Evaluator { get; } // evaluation range, needs eval op
        public Stack<Transform> TransformStack { get; } = new Stack<Transform>();

        public bool CanRepeat() { return true;}

        public void ApplyStart() { }
        public void ApplyEnd() { }
        public void ApplyPartial(long tickOffset) { }

        public Formula(Brain brain)
        {
	        Brain = brain;
	        Id = Brain.NextFormulaId();
        }
    }
}
