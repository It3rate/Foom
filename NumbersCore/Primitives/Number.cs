﻿
using NumbersCore.Utils;

namespace NumbersCore.Primitives;

public enum Polarity { None, Unknown, Aligned, Inverted };//, Zero, Max }

/// <summary>
/// Numbers are intervals, calculated by uniting a Focal with a Domain. They supplement a Focal with Polarity.
/// If Focals had Polarity, Numbers wouldn't technically be needed, but Focals are simple start/end points.
/// Numbers are an assembled instance with all information needed to perform operations.
/// </summary>
public class Number : IMathElement
{
    // todo: all numbers are numberChains, this is a special case with one element.
    public virtual MathElementKind Kind => MathElementKind.Number;

    public int Id { get; internal set; }
    private static int _numberCounter = 1 + (int)MathElementKind.Number;
    public static int NextNumberId() => _numberCounter++;
    public int CreationIndex => Id - (int)Kind - 1;

    public Brain Brain => Trait?.MyBrain;
    public Trait Trait => Domain.Trait;
    public virtual Domain Domain { get; set; }
    public bool IsValid => Domain != null;
    public int DomainId
    {
        get => Domain.Id;
        set => Domain = Domain.Trait.DomainStore[value];
    }
    public bool IsDirty { get => Focal.IsDirty; set => Focal.IsDirty = value; }

    // number to the power of x, where x is also a focal. Eventually this is equations, lazy solve them.
    public Focal BasisFocal => Domain.BasisFocal;

    public Focal Focal { get; set; }

    public int FocalId => Focal.Id;

    protected Polarity _polarity = Polarity.Aligned;
    /// <summary>
    /// Determines if this number has an aligned or inverted perspective relative to the domain basis. 
    /// </summary>
    public Polarity Polarity
    {
        get => IsBasis ? Polarity.Aligned : _polarity;
        set => _polarity = value;
    }
    public int PolarityDirection => IsAligned ? 1 : IsInverted ? -1 : 0;
    public bool IsAligned => Polarity == Polarity.Aligned;
    public bool HasPolairty => Polarity == Polarity.Aligned || Polarity == Polarity.Inverted;
    public bool IsInverted => !IsAligned;
    public int Direction => BasisFocal.Direction * PolarityDirection;
    protected long StartTickPosition
    {
        get => Focal.StartPosition;
        set => Focal.StartPosition = value;
    }
    protected long EndTickPosition
    {
        get => Focal.EndPosition;
        set => Focal.EndPosition = value;
    }
    public long StartTicks
    {
        get => -StartTickPosition + ZeroTick;
        set => StartTickPosition = ZeroTick - value;
    }
    public long EndTicks
    {
        get => EndTickPosition - ZeroTick;
        set => EndTickPosition = value + ZeroTick;
    }
    public long MinTickPosition => Math.Min(StartTickPosition, EndTickPosition);
    public long MaxTickPosition => Math.Max(StartTickPosition, EndTickPosition);
    public long TickCount => EndTickPosition - StartTickPosition;

    public double StartValue
    {
        get => Value.Start;
        set => Value = new PRange(value, Value.End, IsAligned);

    }
    public double EndValue
    {
        get => Value.End;
        set => Value = new PRange(Value.Start, value, IsAligned);
    }
    public PRange Value
    {
        get => Domain.GetValueOf(this);
        set => Domain.SetValueOf(this, value);
    }
    public long ZeroTick => BasisFocal.StartPosition;
    public long BasisTicks => BasisFocal.LengthInTicks;
    public long AbsBasisTicks => BasisFocal.AbsLengthInTicks;

    public Number PolarityBasis(bool isAligned)
    {
        var result = Domain.BasisNumber.Clone(false);
        if (!isAligned)
        {
            result.Polarity = Polarity.Inverted;
            result.Focal.InvertBasis();
        }
        return result;
    }
    public bool IsBasis => IsValid && Domain.BasisFocal.Id == Focal.Id;
    public bool IsMinMax => Domain.MinMaxNumber.Id == Id;
    public bool IsDomainNumber => IsBasis || IsMinMax;
    public bool IsPositivePointing => HasPolairty && (IsAligned && EndTickPosition > StartTickPosition) || (!IsAligned && EndTickPosition < StartTickPosition);
    public bool IsUnitPositivePointing => HasPolairty && (EndTickPosition > StartTickPosition);

    public int StoreIndex { get; set; } // order added to domain

    public Number(Focal focal, Polarity polarity = Polarity.Aligned)
    {
        Focal = focal;
        Polarity = polarity;
    }

    public virtual long[] GetPositions()
    {
        return new long[] { StartTickPosition, EndTickPosition };
    }
    public virtual IEnumerable<PRange> InternalRanges()
    {
        if (IsValid)
        {
            yield return Value;
        }
    }
    public virtual IEnumerable<Number> InternalNumbers()
    {
        if (IsValid)
        {
            yield return this;
        }
    }
    public double StartTValue()
    {
        var val = Domain.MinMaxRange;
        var len = val.Length;
        return (StartValue - val.Start) / len;
    }
    public double EndTValue()
    {
        var val = Domain.MinMaxRange;
        var len = val.Length;
        return (EndValue - val.Start) / len;
    }
    public double TValueOf(double position)
    {
        var val = Value;
        var len = val.Length;
        return (position - val.Start) / len;
    }
    public PRange ExpansiveForce
    {
        get
        {
            var v = Value;
            var len = (float)Math.Sqrt(v.Start * v.Start + v.End * v.End);
            var ratio = (v.EndF) / v.AbsLength();
            var lr = ratio <= 0 ? Math.Abs(ratio) : (1f - ratio);
            var rr = ratio <= 0 ? Math.Abs(ratio + 1f) : (ratio);
            return new PRange(lr * len, rr * len);
        }
    }

    public PRange ValueInRenderPerspective => IsAligned ? new PRange(-StartValue, EndValue) : new PRange(StartValue, -EndValue); //: new PRange(-EndValue, StartValue);

    public Number SetWith(Number other)
    {
        if (Domain.Id == other.Domain.Id)
        {
            StartTickPosition = other.StartTickPosition;
            EndTickPosition = other.EndTickPosition;
        }
        else
        {
            Value = other.Value;
        }
        Polarity = other.Polarity;
        return other;
    }

    public Polarity InvertPolarity()
    {
        Polarity = (Polarity == Polarity.Aligned) ? Polarity.Inverted : Polarity.Aligned;
        return Polarity;
    }
    public Polarity FlipPolarityAndValue()
    {
        Focal.InvertBasis();
        return InvertPolarity();
    }
    public void Negate()
    {
        Value *= -1;
    }
    public void Reverse()
    {
        Focal.Reverse();
    }
    public void InvertAndReverse() // change direction and polarity
    {
        InvertPolarity();
        Reverse();
    }

    public long WholeStartValue => (long)StartValue;
    public long WholeEndValue => (long)EndValue;
    public long RemainderStartValue => Domain.BasisIsReciprocal ? 0 : (long)(Math.Abs(StartValue % 1) * AbsBasisTicks);
    public long RemainderEndValue => Domain.BasisIsReciprocal ? 0 : (long)(Math.Abs(EndValue % 1) * AbsBasisTicks);
    public PRange RangeInMinMax => Focal.UnitTRangeIn(Domain.MinMaxFocal);

    public PRange FloorRange => new PRange(Math.Ceiling(StartValue), Math.Floor(EndValue));
    public PRange CeilingRange => new PRange(Math.Floor(StartValue), Math.Ceiling(EndValue));
    public PRange RoundedRange => new PRange(Math.Round(StartValue), Math.Round(EndValue));
    public PRange RemainderRange => Value - FloorRange;

    public void PlusTick() => EndTickPosition += 1;
    public void MinusTick() => EndTickPosition -= 1;
    public void AddStartTicks(long ticks) => StartTickPosition += ticks;
    public void AddEndTicks(long ticks) => EndTickPosition += ticks;
    public void AddTicks(long startTicks, long endTicks) { StartTickPosition += startTicks; EndTickPosition += endTicks; }
    public void ShiftTicks(long ticks) { StartTickPosition = StartTickPosition + ticks; EndTickPosition = EndTickPosition + ticks; }

    // Operations with segments and units allow moving the unit around freely, so for example,
    // you can shift a segment by aligning the unit with start or end,
    // and scale in place by moving the unit to left, right or center (equivalent to affine scale, where you move to zero, scale, then move back)
    // need to have overloads that allow shifting the unit temporarily
    public virtual void AddValue(Number other)
    {
        // todo: eventually all math on Numbers will be in ticks, allowing preservation of precision etc. Requires syncing of basis, domains.
        Value += other.Value;
    }
    public virtual void SubtractValue(Number other)
    {
        Value -= other.Value;
    }
    public virtual void MultiplyValue(Number other)
    {
        Value *= other.Value;
    }
    public virtual void DivideValue(Number other)
    {
        Value /= other.Value;
    }
    public virtual void Pow(Number other)
    {
        Value = PRange.Pow(Value, other.Value);
    }
    public static Number Pow(Number left,Number right)
    {
        var result = left.Clone(false);
        result.Pow(right);
        return result;
    }

    public static Number GetMaxRange(Number a, Number b)
    {
        var min = Math.Min(a.MinTickPosition, b.MinTickPosition);
        var max = Math.Min(a.MaxTickPosition, b.MaxTickPosition);
        var polarity = SolvePolarity(a.Polarity, b.Polarity);
        var result = new Number(new Focal(min, max), polarity);
        result.Domain = b.Domain;
        return result;
    }
    public static Polarity SolvePolarity(Polarity left, Polarity right) => left == right ? Polarity.Aligned : Polarity.Inverted;


    public void ChangeDomain(Domain newDomain)
    {
        if (newDomain != Domain)
        {
            var value = Value;
            Domain = newDomain;
            Value = value;
        }
    }

    public bool FullyContains(Number toTest, bool includeEndpoints = true) => toTest != null ? Value.FullyContains(toTest.Value, includeEndpoints) : false;
    public Number AlignedDomainCopy(Number toCopy) => AlignToDomain(toCopy, Domain);
    public static Number AlignToDomain(Number target, Domain domain)
    {
        var result = target.Clone();
        result.ChangeDomain(domain);
        return result;
    }

    public void InterpolateFromZero(Number t, Number result) => InterpolateFromZero(this, t, result);
    public void InterpolateFrom(Number source, Number t, Number result) => Interpolate(source, this, t, result);
    public void InterpolateTo(Number target, Number t, Number result) => Interpolate(this, target, t, result);
    public static void InterpolateFromZero(Number target, Number t, Number result)
    {
        var targetValue = target.Value;
        var tValue = t.Value;
        result.StartValue = targetValue.Start * tValue.Start;
        result.EndValue = targetValue.End * tValue.End;
    }
    public static void InterpolateFromOne(Number target, Number t, Number result)
    {
        var targetValue = target.Value;
        var tValue = t.Value;
        result.StartValue = targetValue.Start * tValue.Start;
        result.EndValue = (targetValue.End - 1.0) * tValue.End + 1.0;
    }
    public void InterpolateFromOne(Number target, double t)
    {
        if (target != null)
        {
            var targetValue = target.Value;
            StartValue = targetValue.Start * t;
            EndValue = (targetValue.End - 1.0) * t + 1.0;
        }
    }
    public void InterpolateFromOne(PRange range, double t)
    {
        if (range != null)
        {
            StartValue = range.Start * t;
            EndValue = (range.End - 1.0) * t + 1.0;
        }
    }
    public static void Interpolate(Number source, Number target, Number t, Number result)
    {
        var sourceValue = source.Value;
        var targetValue = target.Value;
        var tValue = t.Value;
        result.StartValue = (targetValue.Start - sourceValue.Start) * tValue.Start + sourceValue.Start;
        result.EndValue = (targetValue.End - sourceValue.End) * tValue.End + sourceValue.End;
    }

    // segment comparison considers unot numbers to also have positive segments in the negative space, and
    // unit numbers to be unot in its negative space. So two unit numbers that have a point on both ends of a range,
    // can be considered an ordinary unot number. eg: [+++>....++>] == [....<---...]
    // This should also handle things like gt lt div0 etc

    //  0000	Never			0			FALSE
    //  0001	Both            A ^ B       AND
    //  0010	Only A          A ^ !B      A AND NOT B
    //  0011	A Maybe B       A           A
    //  0100	Only B			!A ^ B      NOT A AND B
    //  0101	B Maybe A       B           B
    //  0110	One of          A xor B     XOR
    //  0111	At least one    A v B       OR
    //  1000	No one          A nor B     NOR
    //  1001	Both or no one  A XNOR B    XNOR
    //  1010	A or no one		!B          NOT B
    //  1011	Not B alone     A v !B      A OR NOT B
    //  1100	B or no one		!A          NOT A
    //  1101	Not A alone		!A v B      NOT A OR B
    //  1110	Not both        A nand B    NAND
    //  1111	Always			1			TRUE

    /*
		function BooleanOperations()
		{
			this.Never = function(a, b) { return 0; };
			this.And = function(a, b) { return a & b; };
			this.LeftandNotRight = function(a, b) { return a & ~b; };
			this.Left = function(a, b) { return a; };
			this.NotLeftAndRight = function(a, b) { return ~a & b; };
			this.Right = function(a, b) { return b; };
			this.Xor = function(a, b) { return a ^ b; };
			this.Or = function(a, b) { return a | b; };
			this.Nor = function(a, b) { return ~(a | b); };
			this.Xnor = function(a, b) { return ~(a ^ b); };
			this.NotRight = function(a, b) { return ~b; };
			this.LeftOrNotRight = function(a, b) { return a | ~b; };// implies
			this.NotLeft = function(a, b) { return ~a; };
			this.RightOrNotLeft = function(a, b) { return ~a | b; }; // implies
			this.Nand = function(a, b) { return ~(a & b); };
			this.Always = function(a, b) { return 1; };
		}

		*/
    // use segments rather than ints
    // convert values to first param's domain's context
    // result in first params's domain
    public virtual NumberChain Never(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Never(Focal, q.Focal));
    public virtual void Never(Number q, NumberChain result) => result.Reset(Focal.Never(Focal, q.Focal));

    public virtual NumberChain And(Number q) => new NumberChain(GetMaxRange(this, q), Focal.And(Focal, q.Focal));
    public virtual void And(Number q, NumberChain result) => result.Reset(Focal.And(Focal, q.Focal));

    public virtual NumberChain B_Inhibits_A(Number q) => new NumberChain(GetMaxRange(this, q), Focal.B_Inhibits_A(Focal, q.Focal));
    public virtual void B_Inhibits_A(Number q, NumberChain result) => result.Reset(Focal.B_Inhibits_A(Focal, q.Focal));

    public virtual NumberChain Transfer_A(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Transfer_A(Focal, q.Focal));
    public virtual void Transfer_A(Number q, NumberChain result) => result.Reset(Focal.Transfer_A(Focal, q.Focal));

    public virtual NumberChain A_Inhibits_B(Number q) => new NumberChain(GetMaxRange(this, q), Focal.A_Inhibits_B(Focal, q.Focal));
    public virtual void A_Inhibits_B(Number q, NumberChain result) => result.Reset(Focal.A_Inhibits_B(Focal, q.Focal));

    public virtual NumberChain Transfer_B(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Transfer_B(Focal, q.Focal));
    public virtual void Transfer_B(Number q, NumberChain result) => result.Reset(Focal.Transfer_B(Focal, q.Focal));

    public virtual NumberChain Xor(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Xor(Focal, q.Focal));
    public virtual void Xor(Number q, NumberChain result) => result.Reset(Focal.Xor(Focal, q.Focal));

    public virtual NumberChain Or(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Or(Focal, q.Focal));
    public virtual void Or(Number q, NumberChain result) => result.Reset(Focal.Or(Focal, q.Focal));

    public virtual NumberChain Nor(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Nor(Focal, q.Focal));
    public virtual void Nor(Number q, NumberChain result) => result.Reset(Focal.Nor(Focal, q.Focal));

    public virtual NumberChain Xnor(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Xnor(Focal, q.Focal));
    public virtual void Xnor(Number q, NumberChain result) => result.Reset(Focal.Xnor(Focal, q.Focal));

    public virtual NumberChain Not_B(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Not_B(Focal, q.Focal));
    public virtual void Not_B(Number q, NumberChain result) => result.Reset(Focal.Not_B(Focal, q.Focal));

    public virtual NumberChain B_Implies_A(Number q) => new NumberChain(GetMaxRange(this, q), Focal.B_Implies_A(Focal, q.Focal));
    public virtual void B_Implies_A(Number q, NumberChain result) => result.Reset(Focal.B_Implies_A(Focal, q.Focal));

    public virtual NumberChain Not_A(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Not_A(Focal, q.Focal));
    public virtual void Not_A(Number q, NumberChain result) => result.Reset(Focal.Not_A(Focal, q.Focal));

    public virtual NumberChain A_Implies_B(Number q) => new NumberChain(GetMaxRange(this, q), Focal.A_Implies_B(Focal, q.Focal));
    public virtual void A_Implies_B(Number q, NumberChain result) => result.Reset(Focal.A_Implies_B(Focal, q.Focal));

    public virtual NumberChain Nand(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Nand(Focal, q.Focal));
    public virtual void Nand(Number q, NumberChain result) => result.Reset(Focal.Nand(Focal, q.Focal));

    public virtual NumberChain Always(Number q) => new NumberChain(GetMaxRange(this, q), Focal.Always(Focal, q.Focal));
    public virtual void Always(Number q, NumberChain result) => result.Reset(Focal.Always(Focal, q.Focal));


    public Number Clone(bool addToStore = true)
    {
        var result = new Number(Focal.Clone(), Polarity);
        return Domain.AddNumber(result, addToStore);
    }

    public static bool operator ==(Number? a, Number? b)
    {
        if (a is null && b is null)
        {
            return true;
        }
        if (a is null || b is null)
        {
            return false;
        }
        return a.Equals(b);
    }

    public static bool operator !=(Number? a, Number? b)
    {
        return !(a == b);
    }
    public override bool Equals(object? obj)
    {
        return obj is Number other && Equals(other);
    }
    public bool Equals(Number? value)
    {
        if (value is null) { return false; }
        return ReferenceEquals(this, value) ||
                (
                //IsValid && value.IsValid &&
                Polarity == value.Polarity &&
                Focal.Equals(this.Focal, value.Focal)
                );
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Focal.GetHashCode() * 17 ^ ((int)Polarity + 27) * 397;// + (IsValid ? 77 : 33);
            return hashCode;
        }
    }
    public override string ToString()
    {
        var result = "x";
        if (IsValid)
        {
            var v = Value;
            var midSign = v.End > 0 ? " + " : " ";
            result = IsAligned ?
                $"({v.UnotValue:0.##}i{midSign}{v.UnitValue}r)" :
                $"~({v.UnitValue:0.##}r{midSign}{v.UnotValue:0.##}i)";
        }
        return result;
    }
}
