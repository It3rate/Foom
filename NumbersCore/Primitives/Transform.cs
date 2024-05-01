using NumbersCore.CoreConcepts.Counter;
using NumbersCore.Operations;
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
	public Number InputLeft { get; set; }
	public Number InputRight { get; set; }
	public Number Left { get; set; } // the object being transformed
	public Number Right { get; set; } // the amount to transform (can change per repeat)
    public MaskedNumber Result { get; set; }  // current result of transform - this acts as a halt condition when it is empty (false)
	// result goes away. Result is Left and Right at the power ratio endpoints.										  
	public Number LeftUnitTransform { get; set; }
	public Number RightUnitTransform { get; set; }
    public Number BasisSource { get; set; } // choose Basis, 0 is self (add) 1 is domain unit (multiply). Bools are probably 0?
	// Q. allowing both directions (a*b, b*a) means there can be two results?
	// Or there is no result, the transform is applied to the input. You can save a copy of the original input if you like.
	public Number PowerRatio { get; set; }
    // decides if the calculation integrates partial results or is calculated as a step
    public bool IsCompounding { get; set;  } = false;
    // sample with a T. This is for interpolation interest only, not part of the calculation.
    public Number ValueAtT(float t){ throw new NotImplementedException(); }

    // the portion and direction of the transform. 0:1 is A*B (B used up), 1:0 is B*A (A used up).
    // 1:1 is proportional with values, or 0.5:0.5 is equal area 50%? Maybe coorlation is an op for domains, transforms always a force?
    // ratio is actually the same as repeat, just used for the two numbers.

    // *Counter: When a big rock hits a small rock, each transfers a different amount of force. Ratio would be the total force partioned to
    // ratios, but that is just totally and portioning for nothing? They are separate forces and should have separate equations.
    // When multiple forces act on a body, they are separate forces, and the portion of each is only interesting statistically, sometimes.
    // This would mean an equation is always a cause, and if it isn't (coorelation), there needs to be a parent equation causing both.

    // todo: it is a number, the unot portion is the left force, the unit the right force. It also represents the power of each side if not 1
    // the compunding flag has elements recombined together each step using running totals, like compound interest, e, etc
    // Compounding has implications for powers, as it isn't a trend extrapolated, it is recalculated per resolution tick.
    //public Ratio Ratio { get; set; }
    /// <summary>
    /// Repeats are powers, but can extend to any operation. Repeated ADD is like multiply, repeated multiply is pow.
    /// Other ops, like GT, LT can also have repeats. Repeate of zero is just a comparison, with the result being the true part.
    /// Repeat of one forces the result to be fully valid with a transform.
    /// More than one is applying that xform n times (and negative is 1/n).
    /// A complex number of repeats is the start to end result segment.
    /// GT, LT, GTE will preserve length, EQ, CONTAINS will not.
    /// </summary>
    //public Number Repeats { get; set; } // power - start with whole numbers, but will eventually allow any number (pow of complex number)

    public bool IsActive { get; private set; }

    public bool IsSingle => Result.Count == 1;
    public bool IsTrue => !IsFalse;
    public bool IsFalse => Result.Count == 0;

    public bool IsEqual => IsSizeEqual && IsPolarityEqual && IsDirectionEqual;
    public bool IsSizeEqual => Left.IsSizeEqual(Result);// IsSingle && (Left.Focal.Min == Result.First().Min && Left.Focal.Max == Result.First().Max);
    public bool IsPolarityEqual => Left.IsPolarityEqual(Result);// IsSingle && (Left.Polarity == Result.FirstPolarity());
    public bool IsDirectionEqual => Left.IsDirectionEqual(Result);// IsSingle && (Left.Direction == Result.FirstDirection());

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
		InputLeft = left.Clone(false);
        Right = right ?? left.Domain.One(false);
		InputRight = right.Clone(false);
		Left = left;
        Right = right;
        PowerRatio = new Whole(1);

        Result = new MaskedNumber(Left);// left.Clone(false);
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
        Result.SetWith(Left);
        if (Result.Domain == null)
        {
            Left.Domain.AddNumber(Result);
        }
        //Result.SetWith(Left);
        OnStartTransformEvent(this);
        IsActive = true;
        //Repeats?.Increment();
    }
    public void ApplyPartial(long tickOffset) { OnTickTransformEvent(this); }
    public void ApplyEnd()
    {
        if (PowerRatio.EndValue == 1)
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
                    val.Multiply(PowerRatio);
                    Result.ComputeWith(val, OperationKind);
                    break;
                case OperationKind.Multiply:
                    val = Number.Pow(Right, PowerRatio.EndNumber); // todo: do both left and right
                    Result.ComputeWith(val, OperationKind);
                    break;
                case OperationKind.Divide:
                    var one = Right.Domain.CreateNumberFromFloats(0, 1);
                    var recip = PowerRatio.EndNumber;
					recip.Divide(one);
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
                    Result.ComputeWith(PowerRatio.EndNumber, OperationKind.Multiply);
                    break;
                case OperationKind.Multiply:
                    Result.Pow(PowerRatio.EndNumber);
                    break;
                case OperationKind.Divide:
                    var one = Left.Domain.CreateNumberFromFloats(0, 1);
                    var recip = PowerRatio.EndNumber.Clone();
                    recip.Divide(one);
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
    Number? Right { get; set; } // the amount to transform (can change per repeat)
    //MaskedNumber Result { get; set; } // current result of transform - this acts as a halt condition when it is empty (false)
    //Number Repeats { get; set; } 

    event TransformEventHandler StartTransformEvent;
    event TransformEventHandler TickTransformEvent;
    event TransformEventHandler EndTransformEvent;
    void Apply();
    void ApplyStart();
    void ApplyEnd();
    void ApplyPartial(long tickOffset);
    Number ValueAtT(float t);
}
