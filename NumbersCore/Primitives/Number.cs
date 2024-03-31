
using System.Data;
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
    #region MathElement
    public virtual MathElementKind Kind => MathElementKind.Number;

    public int Id { get; internal set; }
    private static int _numberCounter = 1 + (int)MathElementKind.Number;
    public static int NextNumberId() => _numberCounter++;
    public int CreationIndex => Id - (int)Kind - 1;

    public Brain Brain => Trait?.MyBrain;
    public Trait Trait => Domain.Trait;
    #endregion
    public bool IsDirty { get => Focal.IsDirty; set => Focal.IsDirty = value; } // todo: no dirty flags

    public Number(Focal focal, Polarity polarity = Polarity.Aligned)
    {
        Focal = focal;
        Polarity = polarity;
    }
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
    public void SetWith(Focal other, Polarity polarity)
    {
        StartTickPosition = other.StartPosition;
        EndTickPosition = other.EndPosition;
        Polarity = polarity;
    }

    #region Domain
    public virtual Domain Domain { get; set; }
    public bool IsValid => Domain != null;
    public int DomainId
    {
        get => Domain.Id;
        set => Domain = Domain.Trait.DomainStore[value];
    }
    public int StoreIndex { get; set; } // order added to domain
    public Focal BasisFocal => Domain.BasisFocal;

    public bool IsBasis => IsValid && Domain.BasisFocal.Id == Focal.Id;
    public bool IsMinMax => Domain.MinMaxNumber.Id == Id;
    public bool IsDomainNumber => IsBasis || IsMinMax;

    public void ChangeDomain(Domain newDomain)
    {
        if (newDomain != Domain)
        {
            var value = Value;
            Domain = newDomain;
            Value = value;
        }
    }
    public Number AlignedDomainCopy(Number toCopy) => AlignToDomain(toCopy, Domain);
    public static Number AlignToDomain(Number target, Domain domain)
    {
        var result = target.Clone();
        result.ChangeDomain(domain);
        return result;
    }
    #endregion
    #region Focal
    public Focal Focal { get; set; }
    public int FocalId => Focal.Id;

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
    public long ZeroTick => BasisFocal.StartPosition;
    public long BasisTicks => BasisFocal.LengthInTicks;
    public long AbsBasisTicks => BasisFocal.AbsLengthInTicks;

    public void PlusTick() => EndTickPosition += 1;
    public void MinusTick() => EndTickPosition -= 1;
    public void AddStartTicks(long ticks) => StartTickPosition += ticks;
    public void AddEndTicks(long ticks) => EndTickPosition += ticks;
    public void AddTicks(long startTicks, long endTicks) { StartTickPosition += startTicks; EndTickPosition += endTicks; }
    public void ShiftTicks(long ticks) { StartTickPosition = StartTickPosition + ticks; EndTickPosition = EndTickPosition + ticks; }
    #endregion
    #region Direction Polarity
    public int Direction => BasisFocal.Direction * PolarityDirection;
    protected Polarity _polarity = Polarity.Aligned;
    /// <summary>
    /// Determines if this number has an aligned or inverted perspective relative to the domain basis. 
    /// </summary>
    public Polarity Polarity
    {
        get => IsBasis ? Polarity.Aligned : _polarity;
        set => _polarity = value;
    }

    public virtual Polarity[] Polarities => [Polarity];
    public virtual int[] Directions => [Direction];
    /// <summary>
    /// Compares the positions of each number, regardless of direction.
    /// </summary>
    public virtual bool IsSizeEqual(Number num)
    {
        var result = false;
        if (Count == num.Count)
        {
            var p0 = Direction == 1 ? GetPositions() : GetPositions().Reverse();
            var p1 = num.Direction == 1 ? num.GetPositions() : num.GetPositions().Reverse();
            result = p0.SequenceEqual(num.GetPositions());
        }
        return result;
    }
    public virtual bool IsPolarityEqual(Number num) => Polarity == num.Polarity;
    public virtual bool IsDirectionEqual(Number num) => Direction == num.Direction;

    public int PolarityDirection => IsAligned ? 1 : IsInverted ? -1 : 0;
    public bool IsAligned => Polarity == Polarity.Aligned;
    public bool HasPolairty => Polarity == Polarity.Aligned || Polarity == Polarity.Inverted;
    public bool IsInverted => !IsAligned;
    public bool IsPositivePointing => HasPolairty && (IsAligned && EndTickPosition > StartTickPosition) || (!IsAligned && EndTickPosition < StartTickPosition);
    public bool IsUnitPositivePointing => HasPolairty && (EndTickPosition > StartTickPosition);

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
    public static Polarity SolvePolarity(Polarity left, Polarity right) => left == right ? Polarity.Aligned : Polarity.Inverted;
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
    #endregion
    #region PRange
    public virtual double StartValue
    {
        get => Value.Start;
        set => Value = new PRange(value, Value.End, IsAligned);

    }
    public virtual double EndValue
    {
        get => Value.End;
        set => Value = new PRange(Value.Start, value, IsAligned);
    }
    public virtual PRange Value
    {
        get => Domain.GetValueOf(this);
        set => Domain.SetValueOf(this, value);
    }

    public virtual IEnumerable<PRange> InternalRanges()
    {
        if (IsValid)
        {
            yield return Value;
        }
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

    public long WholeStartValue => (long)StartValue;
    public long WholeEndValue => (long)EndValue;
    public long RemainderStartValue => Domain.BasisIsReciprocal ? 0 : (long)(Math.Abs(StartValue % 1) * AbsBasisTicks);
    public long RemainderEndValue => Domain.BasisIsReciprocal ? 0 : (long)(Math.Abs(EndValue % 1) * AbsBasisTicks);
    public PRange RangeInMinMax => Focal.UnitTRangeIn(Domain.MinMaxFocal);

    public PRange FloorRange => new PRange(Math.Ceiling(StartValue), Math.Floor(EndValue));
    public PRange CeilingRange => new PRange(Math.Floor(StartValue), Math.Ceiling(EndValue));
    public PRange RoundedRange => new PRange(Math.Round(StartValue), Math.Round(EndValue));
    public PRange RemainderRange => Value - FloorRange;
    #endregion
    #region T Calculations
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
    #endregion
    #region Operations
    // Operations with segments and units allow moving the unit around freely, so for example,
    // you can shift a segment by aligning the unit with start or end,
    // and scale in place by moving the unit to left, right or center (equivalent to affine scale, where you move to zero, scale, then move back)
    // need to have overloads that allow shifting the unit temporarily
    public virtual void Add(Number right)
    {
        // todo: eventually all math on Numbers will be in ticks, allowing preservation of precision etc. Requires syncing of basis, domains.
        Value += right.Value;
    }
    public virtual void Subtract(Number right)
    {
        Value -= right.Value;
    }
    public virtual void Multiply(Number right)
    {
        Value *= right.Value;
    }
    public virtual void Divide(Number right)
    {
        Value /= right.Value;
    }
    public virtual void Pow(Number right)
    {
        Value = PRange.Pow(Value, right.Value);
    }
    public static Number Pow(Number left, Number right)
    {
        var result = left.Clone(false);
        result.Pow(right);
        return result;
    }

    public void ComputeWith(Number? num, OperationKind operationKind)
    {
        if (operationKind.IsUnary())
        {
            switch (operationKind)
            {
                case OperationKind.None:
                    break;
                case OperationKind.Negate:
                    Negate();
                    break;
                case OperationKind.Reciprocal:
                    break;
                case OperationKind.FlipPolarityInPlace: // switch polarity only, arrow same
                    break;
                case OperationKind.FlipPolarity:
                    break;
                case OperationKind.MirrorOnUnit:
                    break;
                case OperationKind.MirrorOnUnot:
                    break;
                case OperationKind.MirrorOnStart:
                    break;
                case OperationKind.MirrorOnEnd:
                    break;
                case OperationKind.FilterUnit:
                    break;
                case OperationKind.FilterUnot:
                    break;
                case OperationKind.FilterStart:
                    break;
                case OperationKind.FilterEnd:
                    break;
                case OperationKind.NegateInPlace:
                    //_focalGroup.ComputeWith(Focal, operationKind); // change arrow dir
                    break;
            }
        }
        else if (num != null)
        {

            if (operationKind.IsBoolOp())
            {
                ComputeBoolOp(num, operationKind);
            }
            else if (operationKind.IsBoolCompare())
            {
                ComputeBoolCompare(num, operationKind);
            }
            else if (operationKind.IsBinary())
            {
                switch (operationKind)
                {
                    case OperationKind.Add:
                        Add(num);
                        break;
                    case OperationKind.Subtract:
                        Subtract(num);
                        break;
                    case OperationKind.Multiply:
                        Multiply(num);
                        break;
                    case OperationKind.Divide:
                        Divide(num);
                        break;
                    case OperationKind.Root:
                        break;
                    case OperationKind.Wedge:
                        break;
                    case OperationKind.DotProduct:
                        break;
                    case OperationKind.GeometricProduct:
                        break;
                    case OperationKind.Blend:
                        break;
                }
            }
            else if (operationKind.IsTernary())
            {
                switch (operationKind)
                {
                    case OperationKind.PowerAdd:
                        break;
                    case OperationKind.PowerMultiply:
                        break;
                }
            }
            else
            {
                switch (operationKind)
                {
                    case OperationKind.None:
                        break;
                    case OperationKind.AppendAll:
                        break;
                    case OperationKind.MultiplyAll:
                        break;
                    case OperationKind.Average:
                        break;
                }
            }
        }
    }
    public virtual void ComputeBoolOp(Number other, OperationKind operationKind)
    {
        // todo: all bool/compare ops need to use normalized basis', or for now ranges. 
        // really numbers should never be used in bool ops, eventually combine maskedNumber with Number and this goes away
        var (_, table) = SegmentedTable(Domain, true, this, other);
        var (focals, polarities) = ApplyOpToSegmentedTable(table, operationKind);
        Focal resultFocal = new Focal(focals[0].StartPosition, focals[focals.Length - 1].EndPosition);
        var resultPolarity = Polarity.None;
        for (int i = 0; i < polarities.Length; i++)
        {
            if (polarities[i] != Polarity.None){
                resultFocal = focals[i];
                resultPolarity = polarities[i];
                break;
            }
        }
        SetWith(resultFocal, resultPolarity);
    }
    public static Number GetMaxExtent(Number a, Number b)
    {
        var min = Math.Min(a.MinTickPosition, b.MinTickPosition);
        var max = Math.Max(a.MaxTickPosition, b.MaxTickPosition);
        var polarity = SolvePolarity(a.Polarity, b.Polarity);
        var result = new Number(new Focal(min, max), polarity);
        result.Domain = a.Domain;
        return result;
    }
    public virtual void ComputeBoolCompare(Number num, OperationKind operationKind)
    {
        //var positions = new List<long>();
        // todo: calc with ranges or normalize domain as domains may differ
        var maxA = Focal.MaxExtent;
        var minA = Focal.MinExtent;
        var maxB = num.Focal.Max;// startB > endB ? startB : endB;
        var minB = num.Focal.Min;// startB < endB ? startB : endB;
        long resultStart = 0;
        long resultEnd = 0;
        ClearInternalPositions();
        switch (operationKind)
        {
            case OperationKind.GreaterThan: // A all to right of B
                if (minA > maxB) { resultStart = minA; resultEnd = maxA; } else if (maxA > maxB) { resultStart = Math.Max(minA, maxB); resultEnd = maxA; }
                break;
            case OperationKind.GreaterThanOrEqual: // no part of A to left of B
                if (minA >= minB) { resultStart = minA; resultEnd = maxA; } else if (maxA >= minB) { resultStart = Math.Max(minA, minB); resultEnd = maxA; }
                break;
            case OperationKind.GreaterThanAndEqual: // no part of A to left of B, and part to the right of B (A overlap BMax)
                if (minA < maxB && minA > minB && maxA > maxB) { resultStart = minA; resultEnd = maxA; }
                else if (minA <= minB && maxA > maxB) { resultStart = Math.Max(minA, minB); resultEnd = maxA; }
                break;
            case OperationKind.ContainedBy: // A fits inside B
                if (minA >= minB && maxA <= maxB) { resultStart = minA; resultEnd = maxA; }
                else if ((minA < minB && maxA > minB) || (maxA > maxB && minA < maxB)) { resultStart = Math.Max(minA, minB); resultEnd = Math.Min(maxA, maxB); }
                break;
            case OperationKind.Equals: // B matches A
                if (minA == minB && maxA == maxB) { resultStart = minA; resultEnd = maxA; }
                break;
            case OperationKind.Contains: // B fits inside A
                if (minB >= minA && maxB <= maxA) { resultStart = minB; resultEnd = maxB; }
                else if ((minB < minA && maxB > minA) || (maxB > maxA && minB < maxA)) { resultStart = Math.Max(minB, minA); resultEnd = Math.Min(maxB, maxA); }
                break;
            case OperationKind.LessThanAndEqual: // no part of A to right of B, and part to the left of B  (overlap left)
                if (minA < minB && maxA > minB && maxA < maxB) { resultStart = minA; resultEnd = maxA; }
                else if (minA <= minB && maxA > minB) { resultStart = minA; resultEnd = Math.Min(maxA, maxB); }
                break;
            case OperationKind.LessThanOrEqual: // no part of A to right of B
                if (maxA <= maxB) { resultStart = minA; resultEnd = maxA; } else if (minA <= maxB) { resultStart = minA; resultEnd = Math.Min(maxA, maxB); }
                break;
            case OperationKind.LessThan: // A all to left of B
                if (maxA < minB) { resultStart = minA; resultEnd = maxA; } else if (minA < minB) { resultStart = minA; resultEnd = Math.Min(maxA, minB); }
                break;
        }

        if (resultStart - resultEnd != 0) // zero length result not allowed, so this works
        {
            //positions.Add(resultStart);
            //positions.Add(resultStart);
            AddPosition(resultStart, resultEnd);
        }
    }
    #endregion
    #region Bool Ops
    public bool FullyContains(Number toTest, bool includeEndpoints = true) => toTest != null ? Value.FullyContains(toTest.Value, includeEndpoints) : false;

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
    //public virtual NumberGroup Never(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Never(Focal, q.Focal));
    //public virtual void Never(Number q, NumberGroup result) => result.Reset(Focal.Never(Focal, q.Focal));

    //public virtual NumberGroup And(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.And(Focal, q.Focal));
    //public virtual void And(Number q, NumberGroup result) => result.Reset(Focal.And(Focal, q.Focal));

    //public virtual NumberGroup B_Inhibits_A(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.B_Inhibits_A(Focal, q.Focal));
    //public virtual void B_Inhibits_A(Number q, NumberGroup result) => result.Reset(Focal.B_Inhibits_A(Focal, q.Focal));

    //public virtual NumberGroup Transfer_A(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Transfer_A(Focal, q.Focal));
    //public virtual void Transfer_A(Number q, NumberGroup result) => result.Reset(Focal.Transfer_A(Focal, q.Focal));

    //public virtual NumberGroup A_Inhibits_B(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.A_Inhibits_B(Focal, q.Focal));
    //public virtual void A_Inhibits_B(Number q, NumberGroup result) => result.Reset(Focal.A_Inhibits_B(Focal, q.Focal));

    //public virtual NumberGroup Transfer_B(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Transfer_B(Focal, q.Focal));
    //public virtual void Transfer_B(Number q, NumberGroup result) => result.Reset(Focal.Transfer_B(Focal, q.Focal));

    //public virtual NumberGroup Xor(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Xor(Focal, q.Focal));
    //public virtual void Xor(Number q, NumberGroup result) => result.Reset(Focal.Xor(Focal, q.Focal));

    //public virtual NumberGroup Or(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Or(Focal, q.Focal));
    //public virtual void Or(Number q, NumberGroup result) => result.Reset(Focal.Or(Focal, q.Focal));

    //public virtual NumberGroup Nor(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Nor(Focal, q.Focal));
    //public virtual void Nor(Number q, NumberGroup result) => result.Reset(Focal.Nor(Focal, q.Focal));

    //public virtual NumberGroup Xnor(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Xnor(Focal, q.Focal));
    //public virtual void Xnor(Number q, NumberGroup result) => result.Reset(Focal.Xnor(Focal, q.Focal));

    //public virtual NumberGroup Not_B(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Not_B(Focal, q.Focal));
    //public virtual void Not_B(Number q, NumberGroup result) => result.Reset(Focal.Not_B(Focal, q.Focal));

    //public virtual NumberGroup B_Implies_A(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.B_Implies_A(Focal, q.Focal));
    //public virtual void B_Implies_A(Number q, NumberGroup result) => result.Reset(Focal.B_Implies_A(Focal, q.Focal));

    //public virtual NumberGroup Not_A(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Not_A(Focal, q.Focal));
    //public virtual void Not_A(Number q, NumberGroup result) => result.Reset(Focal.Not_A(Focal, q.Focal));

    //public virtual NumberGroup A_Implies_B(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.A_Implies_B(Focal, q.Focal));
    //public virtual void A_Implies_B(Number q, NumberGroup result) => result.Reset(Focal.A_Implies_B(Focal, q.Focal));

    //public virtual NumberGroup Nand(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Nand(Focal, q.Focal));
    //public virtual void Nand(Number q, NumberGroup result) => result.Reset(Focal.Nand(Focal, q.Focal));

    //public virtual NumberGroup Always(Number q) => new NumberGroup(GetMaxRange(this, q), Focal.Always(Focal, q.Focal));
    //public virtual void Always(Number q, NumberGroup result) => result.Reset(Focal.Always(Focal, q.Focal));
    #endregion
    #region Interpolation
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
    #endregion
    #region Internal Segments
    // todo: internal segments should be a class added with composition
    /// <summary>
    /// The number of subnumbers, will always be 1 for normal numbers, can be 0->n for masked or groups numbers.
    /// </summary>
    public virtual int Count => 1;
    public virtual long[] GetPositions()
    {
        return new long[] { StartTickPosition, EndTickPosition };
    }
    public virtual void ClearInternalPositions() { }
    public virtual void AddPosition(long start, long end) 
    {
        StartTickPosition = start;
        EndTickPosition = end;
    }
    public virtual void AddPosition(Focal focal)
    {
        AddPosition(focal.StartPosition, focal.EndPosition);
    }
    public virtual void AddPosition(Number num)
    {
        AddPosition(num.Focal.StartPosition, num.Focal.EndPosition);
    }
    public virtual void AddPosition(PRange range)
    {
        var focal = Domain.CreateFocalFromRange(range);
        AddPosition(focal.StartPosition, focal.EndPosition);
    }
    public virtual IEnumerable<Number> InternalNumbers()
    {
        if (IsValid)
        {
            yield return this;
        }
    }
    /// <summary>
    /// NumberGroups can have overlapping numbers, so this segmented version returns all partial ranges for each possible segment.
    /// Assumes aligned domains.
    /// </summary>
    public static (long[], List<Number[]>) SegmentedTable(Domain domain, bool allowOverlap, params Number[] numbers)
    {
        var result = new List<Number[]>();
        var internalNumberSets = new List<Number[]>();
        var sPositions = new SortedSet<long>();
        foreach (var number in numbers)
        {
            if (number.IsValid)
            {
                internalNumberSets.Add(number.InternalNumbers().ToArray());
                sPositions.UnionWith(number.GetPositions());
            }
            else
            {
                internalNumberSets.Add([]);
            }
        }
        var positions = sPositions.ToArray();

        for (int i = 1; i < positions.Length; i++)
        {
            var focal = new Focal(positions[i - 1], positions[i]);
            var matches = new List<Number>();
            foreach (var numSet in internalNumberSets)
            {
                if (numSet.Length == 0)
                {
                    matches.Add(CreateSubsegment(domain, focal, Polarity.None));
                }
                else
                {
                    foreach (var number in numSet)
                    {
                        var intersection = Focal.Intersection(number.Focal, focal);
                        if (intersection != null)
                        {
                            matches.Add(CreateSubsegment(domain, intersection, number.Polarity));
                        }
                        else if(allowOverlap)
                        {
                            matches.Add(CreateSubsegment(domain, focal, Polarity.None)); // False is polarity none
                        }
                    }
                }
            }
            result.Add(matches.ToArray());
        }
        return (positions, result);
    }
    protected static Number CreateSubsegment(Domain domain, Focal focal, Polarity polairty = Polarity.None)
    {
        var result = new Number(focal.Clone(), polairty); // false
        result.Domain = domain;
        return result;
    }
    protected static (Focal[], Polarity[]) ApplyOpToSegmentedTable(List<Number[]> data, OperationKind operation)
    {
        var focals = new List<Focal>();
        var polarities = new List<Polarity>();
        Focal? lastFocal = null;
        Polarity lastPolarity = Polarity.Unknown;

        foreach (var seg in data)
        {
            if (seg.Length == 0)
            {
            }
            else
            {
                var first = seg[0];
                var op = operation;
                var opResult = false;
                var dirResult = false;
                var polResult = false;
                for (int i = 0; i < seg.Length; i++)
                {
                    var curNum = seg[i];
                    var func = op.GetFunc();
                    if (i == 0)
                    {
                        polResult = curNum.IsAligned;
                        dirResult = curNum.IsUnitPositivePointing;
                        opResult = curNum.HasPolairty;
                    }
                    else
                    {
                        polResult = func(polResult, curNum.IsAligned); ;
                        dirResult = func(dirResult, curNum.IsUnitPositivePointing);
                        opResult = func(opResult, curNum.HasPolairty);
                    }
                }

                if (opResult)
                {
                    var focal = dirResult ? new Focal(first.MinTickPosition, first.MaxTickPosition) : new Focal(first.MaxTickPosition, first.MinTickPosition);
                    var polarity = polResult ? Polarity.Aligned : Polarity.Inverted;
                    if (lastFocal != null && lastPolarity == polarity && lastFocal.IsPositiveDirection == focal.IsPositiveDirection) // merge continuous segments
                    {
                        if (lastFocal.IsPositiveDirection)
                        {
                            lastFocal.EndPosition = focal.EndPosition;
                        }
                        else
                        {
                            lastFocal.StartPosition = focal.StartPosition;
                        }
                    }
                    else
                    {
                        focals.Add(focal);
                        polarities.Add(polarity);
                        lastFocal = focal;
                        lastPolarity = polarity;
                    }
                }
                else
                {
                    lastFocal = null;
                }
            }
        }

        return (focals.ToArray(), polarities.ToArray());

    }
    #endregion

    #region Equality
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
    #endregion

    public override string ToString()
    {
        var result = "x";
        if (IsValid)
        {
            var v = Value;
            if (Polarity == Polarity.None)
            {
                result = $"x({v.Start:0.##}_{-v.End:0.##})"; // no polarity, so just list values
            }
            else
            {
                var midSign = v.End > 0 ? " + " : " ";
                result = IsAligned ?
                    $"({v.UnotValue:0.##}i{midSign}{v.UnitValue}r)" :
                    $"~({v.UnitValue:0.##}r{midSign}{v.UnotValue:0.##}i)";
            }
        }
        return result;
    }
}
