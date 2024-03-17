using System;
using System.Collections.Generic;
using NumbersCore.CoreConcepts.Counter;
using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
    // **** All ops, history, sequences, equations should fit on traits as focals.

    // Operations on source(s)
    // Select (add, can create or add to or shift selection)
    // Unary - Invert (apply only polarity change, no multiply), Negate, Not, flip arrow direction, mirror on start point/endpoint
    // Multiply (stretch)
    // Steps (part of an interpolation op, can break into subsegments)
    // Repeat (can add/duplicate segments if repeated op does)
    // Causal directions (one time, one way, two way, result only, locks)
    // Select partial (either unit, unot)
    // Links (link domains, basis, focals)
    // Bool ops (like contain, repel,interpolate until etc)
    // Branch (bool ops can split segment, resulting in choice?)

    public class Transform : ITransform
    {
	    public MathElementKind Kind => MathElementKind.Transform;

	    public int Id { get; set; }
	    public int CreationIndex => Id - (int)Kind - 1;
        public Brain Brain { get; }

        public bool IsDirty { get; set; } = true;
        public OperationKind OperationKind { get; set; }
        public bool IsUnary => OperationKind.IsUnary();
        public int Repeats { get; set;  } = 1;
        public int Steps { get; set; } = 1;
	    public Number Left { get; set; } // the object being transformed
	    public Number Right { get; set; } // the amount to transform (can change per repeat)
        public NumberChain Result { get; set; } // current result of transform

        public UpCounter RepeatCounter { get; } = new UpCounter();
        public UpCounter StepCounter { get; } = new UpCounter();
        public Evaluation HaltCondition { get; set; } // the evaluation that decides if the transform can continue
        public bool IsActive { get; private set; }

        public bool IsSingle => Result.Count == 1;
        public bool IsTrue => Result.Count > 0;
        public bool IsFalse => Result.Count == 0;
        public bool IsEqual => IsSizeEqual && IsPolarityEqual && IsDirectionEqual;
        public bool IsSizeEqual => IsSingle && (Left.Focal.Min == Result.First().Min && Left.Focal.Max == Result.First().Max);
        public bool IsPolarityEqual => IsSingle && (Left.Polarity == Result.FirstPolarity());
        public bool IsDirectionEqual => IsSingle && (Left.Direction == Result.FirstDirection());

        public IEnumerable<Number> UsedNumbers()
        {
            yield return Left;

            if (!IsUnary)
            {
                yield return Right;
            }

            yield return Result;
        }

        public Transform(Number left, Number right, OperationKind kind) // todo: add default numbers (0, 1, unot, -1 etc) in global domain.
        {
	        Left = left;
	        Right = right;

            Result = new NumberChain(Right.Domain.MinMaxNumber);// left.Clone(false);
	        OperationKind = kind;
	        Brain = right.Brain;
	        Id = Brain.NextTransformId();
        }

        public event TransformEventHandler StartTransformEvent;
	    public event TransformEventHandler TickTransformEvent;
	    public event TransformEventHandler EndTransformEvent;

        public bool Involves(Number num) => (Left.Id == num.Id || Right.Id == num.Id || Result.Id == num.Id);
        public void Apply()
        {
            ApplyStart();
            ApplyEnd();
        }
	    public void ApplyStart()
        {
            Result.Reset(Left, OperationKind.None);
            //Result.SetWith(Left);
            OnStartTransformEvent(this);
		    IsActive = true;
		    RepeatCounter.Increment();
	    }
	    public void ApplyPartial(long tickOffset) { OnTickTransformEvent(this); }
	    public void ApplyEnd()
	    {
            Result.ComputeWith(Right, OperationKind);
		    OnEndTransformEvent(this);
		    IsActive = false;
	    }

	    public bool Evaluate() => true;
	    public bool IsComplete() => HaltCondition?.EvaluateFlags() ?? true;

	    protected virtual void OnStartTransformEvent(ITransform e)
	    {
		    StartTransformEvent?.Invoke(this, e);
	    }

	    protected virtual void OnTickTransformEvent(ITransform e)
	    {
		    TickTransformEvent?.Invoke(this, e);
	    }

	    protected virtual void OnEndTransformEvent(ITransform e)
	    {
		    EndTransformEvent?.Invoke(this, e);
	    }
        public override string ToString()
        {
            var symbol = OperationKind.GetSymbol();
            return $"{Left} {symbol} {Right} = {Result}";
        }
    }

    public delegate void TransformEventHandler(object sender, ITransform e);
    public interface ITransform : IMathElement
    {
	    Number Right { get; set; }
	    event TransformEventHandler StartTransformEvent;
	    event TransformEventHandler TickTransformEvent;
	    event TransformEventHandler EndTransformEvent;
        void Apply();
        void ApplyStart();
	    void ApplyEnd();
	    void ApplyPartial(long tickOffset);
	    bool IsComplete();
    }
}
