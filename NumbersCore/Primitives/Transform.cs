using System;
using System.Collections.Generic;
using NumbersCore.CoreConcepts.Counter;
using NumbersCore.Utils;

namespace NumbersCore.Primitives;

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
    public bool IsUnary => Right == null;// OperationKind.IsUnary();
    public Number Left { get; set; } // the object being transformed
    public Number? Right { get; set; } // the amount to transform (can change per repeat)
    public NumberGroup Result { get; set; }  // current result of transform - this acts as a halt condition when it is empty (false)
    
    /// <summary>
    /// Repeats are powers, but can extend to any operation. Repeated ADD is like multiply, repeated multiply is pow.
    /// Other ops, like GT, LT can also have repeats. Repeate of zero is just a comparison, with the result being the true part.
    /// Repeat of one forces the result to be fully valid with a transform.
    /// More than one is applying that xform n times (and negative is 1/n).
    /// A complex number of repeats is the start to end result segment.
    /// GT, LT, GTE will preserve length, EQ, CONTAINS will not.
    /// </summary>
    public Number Repeats { get; set; } // power - start with whole numbers, but will eventually allow any number (pow of complex number)

    public bool IsActive { get; private set; }

    public bool IsSingle => Result.Count == 1;
    public bool IsTrue => !IsFalse;
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

    public Transform(Number left, Number? right, OperationKind kind) // todo: add default numbers (0, 1, unot, -1 etc) in global domain.
    {
        Left = left;
        Right = right;

        Result = new NumberGroup(Left.Domain.MinMaxNumber);// left.Clone(false);
        OperationKind = kind;
        Brain = Left.Brain;
        Id = Brain.NextTransformId();
    }

    public event TransformEventHandler StartTransformEvent;
    public event TransformEventHandler TickTransformEvent;
    public event TransformEventHandler EndTransformEvent;

    public bool Involves(Number num) => (Left.Id == num.Id || Right?.Id == num.Id || Result.Id == num.Id);
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
        //Repeats?.Increment();
    }
    public void ApplyPartial(long tickOffset) { OnTickTransformEvent(this); }
    public void ApplyEnd()
    {
        if (Repeats.EndValue == 1)
        {
            Result.ComputeWith(Right, OperationKind);
        }
        else if (Right != null) 
        {
            Number val;
            switch (OperationKind)
            {
                case OperationKind.Add:
                case OperationKind.Subtract:
                    val = Right.Clone();
                    val.MultiplyValue(Repeats);
                    Result.ComputeWith(val, OperationKind);
                    break;
                case OperationKind.Multiply:
                    val = Number.Pow(Right, Repeats);
                    Result.ComputeWith(val, OperationKind);
                    break;
                case OperationKind.Divide:
                    var one = Right.Domain.CreateNumberFromFloats(0, 1);
                    var recip = Repeats.Clone();
                    recip.DivideValue(one);
                    val = Number.Pow(Right, recip);
                    Result.ComputeWith(val, OperationKind);
                    break;
                default:
                    Result.ComputeWith(Right, OperationKind);
                    break;
            }
        }
        else
        {
            Number val;
            switch (OperationKind)
            {
                case OperationKind.Add:
                case OperationKind.Subtract:
                    Result.ComputeWith(Repeats, OperationKind.Multiply);
                    break;
                case OperationKind.Multiply:
                    Result.Pow(Repeats);
                    break;
                case OperationKind.Divide:
                    var one = Left.Domain.CreateNumberFromFloats(0, 1);
                    var recip = Repeats.Clone();
                    recip.DivideValue(one);
                    Result.Pow(recip);
                    break;
                default:
                    Result.ComputeWith(Left, OperationKind);
                    break;
            }
        }
        OnEndTransformEvent(this);
        IsActive = false;
    }

    public bool Evaluate() => true;

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
    Number Left { get; set; } // the object being transformed
    Number Right { get; set; } // the amount to transform (can change per repeat)
    NumberGroup Result { get; set; } // current result of transform - this acts as a halt condition when it is empty (false)
    //Number Repeats { get; set; } 

    event TransformEventHandler StartTransformEvent;
    event TransformEventHandler TickTransformEvent;
    event TransformEventHandler EndTransformEvent;
    void Apply();
    void ApplyStart();
    void ApplyEnd();
    void ApplyPartial(long tickOffset);
}
