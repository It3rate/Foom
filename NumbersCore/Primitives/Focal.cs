
using NumbersCore.Utils;

namespace NumbersCore.Primitives;

// todo: Create a value only (nonRef) focal
//public interface Focal : IMathElement
//{
//    long StartTickPosition { get; set; }
//    long EndTickPosition { get; set; }
//    long LengthInTicks { get; }
//    long AbsLengthInTicks { get; }
//    long NonZeroLength { get; }
//    int Direction { get; }
//    bool IsUnitPerspective { get; }
//    bool IsUnotPerspective { get; }
//    long Min { get; }
//    long Max { get; }

//    FocalPositions FocalPositions { get; set; }
//    void Reset(long start, long end);
//    void Reset(Focal focal);

//    void FlipAroundStartPoint();

//    PRange RangeAsBasis(Focal nonBasis);
//    PRange GetRangeWithBasis(Focal basis, bool isReciprocal);
//    void SetWithRangeAndBasis(PRange range, Focal basis, bool isReciprocal);

//    PRange UnitTRangeIn(Focal basis);
//    Focal Clone();
//}

public class Focal : IMathElement, IEquatable<Focal>
{
    public MathElementKind Kind => MathElementKind.Focal;
    public int Id { get; }
    private static int _focalCounter = 1 + (int)MathElementKind.Focal;
    public int CreationIndex => Id - (int)Kind - 1;

	protected long[] _positions;
	protected double[] _tPositions;
	public virtual bool IsEmpty => false;
	public virtual int Count => 1;

	public virtual bool IsDirty { get; set; } = true; // remove IsDirty once transforms propogate correctly


	public virtual long StartPosition
	{
		get => _positions[0];
		set
		{
			if (_positions[0] != value)
			{
				_positions[0] = value;
				SetTPositions(_tPositions);
				IsDirty = true;
			}
		}
	}
	public virtual long EndPosition
	{
		get => _positions[_positions.Length - 1];
		set
		{
			if (_positions[_positions.Length - 1] != value)
			{
				_positions[_positions.Length - 1] = value;
				SetTPositions(_tPositions);
				IsDirty = true;
			}
		}
	}

	public int Direction => StartPosition < EndPosition ? 1 : StartPosition > EndPosition ? -1 : 0;

    public bool IsPositiveDirection => Direction > 0;
    public virtual long InvertedEndPosition => StartPosition - LengthInTicks;

    /// <summary>
    /// Will be true other than MaskedFocals, where a bool operation can start with false (like with an empty result).
    /// </summary>
    public BoolState StartState { get; protected set; } = BoolState.True;

    /// <summary>
    /// Focals are pre-number segments, not value interpretations.
    /// The basis focal decides the aligned direction in the number, whatever direction that is will be the aligned unit direction.
    /// </summary>
    protected Focal()
    {
        Id = _focalCounter++;
    }
    public Focal(long startTickPosition, long endTickPosition) : this()
	{
		Id = _focalCounter++;
		_positions = new long[2];
		_tPositions = [];
		StartPosition = startTickPosition;
        EndPosition = endTickPosition;
	}
	public virtual IEnumerable<long> Positions()
	{
		for (int i = 0; i < _positions.Length; i++)
		{
			yield return _positions[i];
		}
	}
	public virtual long[] GetPositions() => (long[])_positions.Clone();
	public virtual void SetPosition(int index, long value)
	{
		if (index >= 0 && index < _positions.Length)
		{
			_positions[index] = value;
		}
	}
    public virtual void ClearInternalPositions() { }

	protected virtual double[] GetTPositions() => []; // no internal mask positions in Focal
    protected virtual void SetTPositions(double[] tPositions) { }
    public virtual BoolState GetMaskAtPosition(long position) => BoolState.True;


    public long LengthInTicks => EndPosition - StartPosition;
    public long AbsLengthInTicks => Math.Abs(LengthInTicks);
    public long NonZeroLength => LengthInTicks == 0 ? 1 : LengthInTicks;
    public virtual long AlignedEndPosition(bool isAligned) => isAligned ? EndPosition : InvertedEndPosition;
    public virtual long AlignedLengthInTicks(bool isAligned) => AlignedEndPosition(isAligned) - StartPosition;
    public long AlignedNonZeroLength(bool isAligned)
    {
        var result = AlignedLengthInTicks(isAligned);
        return result == 0 ? (isAligned ? 1 : -1) : result;
    }

    public virtual void Reset(long start, long end)
    {
        StartPosition = start;
        EndPosition = end;
    }
    public virtual void Reset(Focal focal)
    {
        Reset(focal.StartPosition, focal.EndPosition);
    }
    public void InvertBasis()
    {
        EndPosition = StartPosition - LengthInTicks;
    }

    public PRange GetRangeWithBasis(Focal basis, bool isReciprocal, bool isAligned)
    {
        var len = (double)basis.NonZeroLength * (isAligned ? 1 : -1); //AlignedNonZeroLength(isAligned);// 
        var start = (StartPosition - basis.StartPosition) / len;
        var end = (EndPosition - basis.StartPosition) / len;
        if (isReciprocal)
        {
            start = Math.Round(start) * Math.Abs(len);
            end = Math.Round(end) * Math.Abs(len);
        }
        var result = new PRange(-start, end, isAligned);
        return result;
    }
    public void SetWithRangeAndBasis(PRange range, Focal basis, bool isReciprocal)
    {
        if (basis.Id != Id) // basis knows its own focal positions by definition
        {
            var len = (double)basis.NonZeroLength * (range.IsAligned ? 1 : -1);
            var z = basis.StartPosition;
            var start = z - range.Start * len;
            var end = z + range.End * len;
            if (isReciprocal)
            {
                start = Math.Round(start) / Math.Abs(len);
                end = Math.Round(end) / Math.Abs(len);
            }
            StartPosition = (long)Math.Round(start);
            EndPosition = (long)Math.Round(end);
        }
    }
    public PRange RangeAsBasis(Focal nonBasis) => nonBasis.GetRangeWithBasis(this, false, true);
    public PRange UnitTRangeIn(Focal basis)
    {
        var len = (double)Math.Abs(basis.NonZeroLength);
        var start = (StartPosition - basis.StartPosition) / len;
        var end = (EndPosition - basis.StartPosition) / len;
        return new PRange(start, end);
    }
    public long Min => StartPosition <= EndPosition ? StartPosition : EndPosition; // always the end points
    public long Max => StartPosition >= EndPosition ? StartPosition : EndPosition;
    public virtual long MaxExtent => Max; // can be a midpoint in case of a focal group (max extent)
    public virtual long MinExtent => Min;
    public Focal Negated => new Focal(-StartPosition, -EndPosition);
    public void Reverse()
    {
        var temp = StartPosition;
        StartPosition = EndPosition;
        EndPosition = temp;
    }
    public void MakeForward()
    {
        if (EndPosition < StartPosition)
        {
            Reverse();
        }
    }
    public bool Touches(Focal q) => Touches(this, q);


    public static long MinPosition(Focal p, Focal q) => Math.Min(p.Min, q.Min);
    public static long MaxPosition(Focal p, Focal q) => Math.Max(p.Max, q.Max);
    public static long MinStart(Focal p, Focal q) => Math.Min(p.StartPosition, q.StartPosition);
    public static long MaxStart(Focal p, Focal q) => Math.Max(p.StartPosition, q.StartPosition);
    public static long MinEnd(Focal p, Focal q) => Math.Min(p.EndPosition, q.EndPosition);
    public static long MaxEnd(Focal p, Focal q) => Math.Max(p.EndPosition, q.EndPosition);

    /// <summary>
    /// Shares an endpoint, but no overlap (meaning can only share one endpoint).
    /// </summary>
    public static bool Touches(Focal p, Focal q) => p.EndPosition == q.StartPosition || p.StartPosition == q.EndPosition;
    public static Focal? Intersection(Focal p, Focal q)
    {
        Focal result = null;
        var ov = Overlap(p, q);
        if (ov.LengthInTicks != 0)
        {
            result = ov;
            if (!p.IsPositiveDirection) { ov.Reverse(); }
        }
        return result;
    }
    public static Focal Overlap(Focal p, Focal q)
    {
        var start = Math.Max(p.Min, q.Min);
        var end = Math.Min(p.Max, q.Max);
        return (start >= end) ? new Focal(0, 0) : new Focal(start, end);
    }
    public static Focal Extent(Focal p, Focal q)
    {
        var start = Math.Min(p.Min, q.Min);
        var end = Math.Max(p.Max, q.Max);
        return new Focal(start, end);
    }

    // Q. Should direction be preserved in a bool operation?
    public static Focal[] Never(Focal p)
    {
        return new Focal[0];
    }
    public static Focal[] UnaryNot(Focal p)
    {
        // If p starts at the beginning of the time frame and ends at the end, A is always true and the "not A" relationship is empty
        if (p.StartPosition == 0 && p.EndPosition == long.MaxValue)
        {
            return new Focal[] { };
        }
        // If p starts at the beginning of the time frame and ends before the end, the "not A" relationship consists of a single interval from p.EndTickPosition + 1 to the end of the time frame
        else if (p.StartPosition == 0)
        {
            return new Focal[] { new Focal(p.EndPosition + 1, long.MaxValue) };
        }
        // If p starts after the beginning of the time frame and ends at the end, the "not A" relationship consists of a single interval from the beginning of the time frame to p.StartTickPosition - 1
        else if (p.EndPosition == long.MaxValue)
        {
            return new Focal[] { new Focal(0, p.StartPosition - 1) };
        }
        // If p starts and ends within the time frame, the "not A" relationship consists of two intervals: from the beginning of the time frame to p.StartTickPosition - 1, and from p.EndTickPosition + 1 to the end of the time frame
        else
        {
            return new Focal[] { new Focal(0, p.StartPosition - 1), new Focal(p.EndPosition + 1, long.MaxValue) };
        }
    }
    public static Focal[] Transfer(Focal p)
    {
        return new Focal[] { p.Clone() };
    }
    public static Focal[] Always(Focal p)
    {
        return new Focal[] { Focal.MinMaxFocal.Clone() };
    }

    public static Focal[] Never(Focal p, Focal q)
    {
        return new Focal[0];
    }
    public static Focal[] And(Focal p, Focal q)
    {
        var overlap = Overlap(p, q);
        return (overlap.LengthInTicks == 0) ? new Focal[0] : new Focal[] { overlap };
    }
    public static Focal[] B_Inhibits_A(Focal p, Focal q)
    {
        if (p.EndPosition < q.StartPosition - 1 || q.EndPosition < p.StartPosition - 1)
        {
            return new Focal[] { p };
        }
        else
        {
            return new Focal[] { new Focal(p.StartPosition, q.StartPosition - 1) };
        }
    }
    public static Focal[] Transfer_A(Focal p, Focal q)
    {
        return new Focal[] { p };
    }
    public static Focal[] A_Inhibits_B(Focal p, Focal q)
    {
        if (p.EndPosition < q.StartPosition - 1 || q.EndPosition < p.StartPosition - 1)
        {
            return new Focal[] { q };
        }
        else
        {
            return new Focal[] { new Focal(q.StartPosition, p.StartPosition - 1) };
        }
    }
    public static Focal[] Transfer_B(Focal p, Focal q)
    {
        return new Focal[] { q };
    }
    public static Focal[] Xor(Focal p, Focal q)
    {
        // Return the symmetric difference of the two input segments as a new array of segments
        List<Focal> result = new List<Focal>();
        Focal[] andResult = And(p, q);
        if (andResult.Length == 0)
        {
            // If the segments do not intersect, return the segments as separate non-overlapping segments
            result.Add(p);
            result.Add(q);
        }
        else
        {
            // If the segments intersect, return the complement of the intersection in each segment
            Focal[] complement1 = Nor(p, andResult[0]);
            Focal[] complement2 = Nor(q, andResult[0]);
            result.AddRange(complement1);
            result.AddRange(complement2);
        }
        return result.ToArray();
    }
    public static Focal[] Or(Focal p, Focal q)
    {
        var overlap = Overlap(p, q);
        return (overlap.LengthInTicks == 0) ? new Focal[] { p, q } : new Focal[] { Extent(p, q) };
    }
    public static Focal[] Nor(Focal p, Focal q)
    {
        // Return the complement of the union of the two input Focals as a new array of Focals
        List<Focal> result = new List<Focal>();
        Focal[] orResult = Or(p, q);
        if (orResult.Length == 0)
        {
            // If the Focals do not overlap, return both Focals as separate non-overlapping Focals
            result.Add(p);
            result.Add(q);
        }
        else
        {
            // If the Focals overlap, return the complement of the union in each Focal
            Focal[] complement1 = Nand(p, orResult[0]);
            Focal[] complement2 = Nand(q, orResult[0]);
            result.AddRange(complement1);
            result.AddRange(complement2);
        }
        return result.ToArray();
    }
    public static Focal[] Xnor(Focal p, Focal q)
    {
        // Return the complement of the symmetric difference of the two input Focals as a new array of Focals
        List<Focal> result = new List<Focal>();
        Focal[] xorResult = Xor(p, q);
        if (xorResult.Length == 0)
        {
            // If the Focals are equal, return p as a single Focal
            result.Add(p);
        }
        else
        {
            // If the Focals are not equal, return the complement of the symmetric difference in each Focal
            Focal[] complement1 = Nor(p, xorResult[0]);
            Focal[] complement2 = Nor(q, xorResult[0]);
            result.AddRange(complement1);
            result.AddRange(complement2);
        }
        return result.ToArray();
    }
    public static Focal[] Not_B(Focal p, Focal q)
    {
        return UnaryNot(q);
    }
    public static Focal[] B_Implies_A(Focal p, Focal q)
    {
        if (p.EndPosition < q.StartPosition - 1 || q.EndPosition < p.StartPosition - 1)
        {
            return new Focal[] { };
        }
        else
        {
            return new Focal[] { new Focal(MaxStart(p, q), MinEnd(p, q)) };
        }
    }
    public static Focal[] Not_A(Focal p, Focal q)
    {
        return UnaryNot(p);
    }
    public static Focal[] A_Implies_B(Focal p, Focal q)
    {
        if (p.EndPosition < q.StartPosition - 1 || q.EndPosition < p.StartPosition - 1)
        {
            return new Focal[] { };
        }
        else
        {
            return new Focal[] { new Focal(MaxStart(p, q), MinEnd(p, q)) };
        }
    }
    public static Focal[] Nandx(Focal p, Focal q)
    {
        // Return the complement of the intersection of the two input Focals as a new array of Focals
        List<Focal> result = new List<Focal>();
        Focal[] andResult = And(p, q);
        if (andResult.Length == 0)
        {
            // If the Focals do not intersect, return the union of the Focals as a single Focal
            result.Add(new Focal(MinStart(p, q), MaxEnd(p, q)));
        }
        else
        {
            // If the Focals intersect, return the complement of the intersection in each Focal
            Focal[] complement1 = Nor(p, andResult[0]);
            Focal[] complement2 = Nor(q, andResult[0]);
            result.AddRange(complement1);
            result.AddRange(complement2);
        }
        return result.ToArray();
    }
    public static Focal[] Nand(Focal p, Focal q)
    {
        Focal[] result;
        var overlap = Overlap(p, q);
        if (overlap.LengthInTicks == 0)
        {
            result = new Focal[] { Focal.MinMaxFocal.Clone() };
        }
        else
        {
            result = new Focal[]
            {
                new Focal(long.MinValue, overlap.StartPosition),
                new Focal(overlap.EndPosition, long.MaxValue)
            };
        }
        return result;
    }
    public static Focal[] Always(Focal p, Focal q)
    {
        return new Focal[] { Focal.MinMaxFocal.Clone() };
    }

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

    public long Never(long a) { return 0; } // Null
    public long Transfer(long a) { return a; }
    public long UnaryNot(long a) { return ~a; }
    public long Identity(long a) { return -1; }

    public long Never(long a, long b) { return 0; } // Null
    public long And(long a, long b) { return a & b; }
    public long B_Inhibits_A(long a, long b) { return a & ~b; } // inhibition, div a/b. ab`
    public long Transfer_A(long a, long b) { return a; }                   // transfer
    public long A_Inhibits_B(long a, long b) { return ~a & b; } // inhibition, div b/a. a`b
    public long Transfer_B(long a, long b) { return b; }                   // transfer
    public long Xor(long a, long b) { return a ^ b; }
    public long Or(long a, long b) { return a | b; }
    public long Nor(long a, long b) { return ~(a | b); }
    public long Xnor(long a, long b) { return ~(a ^ b); } // equivalence, ==. (xy)`
    public long Not_B(long a, long b) { return ~b; } // complement
    public long B_Implies_A(long a, long b) { return a | ~b; } // implication (Not B alone)
    public long Not_A(long a, long b) { return ~a; } // complement
    public long A_Implies_B(long a, long b) { return b | ~a; } // implication (Not A alone)
    public long Nand(long a, long b) { return ~(a & b); }
    public long Always(long a, long b) { return -1; }

    public long Equals(long a, long b) { return ~(a ^ b); } // xnor
    public long NotEquals(long a, long b) { return a ^ b; } // xor
    public long GreaterThan(long a, long b) { return ~a & b; } // b > a
    public long LessThan(long a, long b) { return a & ~b; } // a > b
    public long GreaterThanOrEqual(long a, long b) { return ~a | b; } // a implies b
    public long LessThanOrEqual(long a, long b) { return a | ~b; } // b implies a



    public static Focal CreateZeroFocal(long ticks) { return new Focal(0, ticks); }
    public static Focal CreateBalancedFocal(long halfTicks) { return new Focal(-halfTicks, halfTicks); }
    public static Focal MinMaxFocal => new Focal(long.MinValue, long.MaxValue);
    public static Focal OneFocal => new Focal(0, 1);
    public static Focal UpMaxFocal => new Focal(0, long.MaxValue);

    /// <summary>
    /// Creates table with all stops, giving left and right position states for each stop.
    /// A truth table only acts on valid parts of segments. -10i+5 has two parts, 0 to -10i and 0 to 5. This is the area bools apply to.
    /// </summary>
    public static List<(long, BoolState, BoolState)> BuildTruthTable(Focal left, Focal right)
    {
        var result = new List<(long, BoolState, BoolState)>();
        var leftPositions = left.GetPositions();
        var rightPositions = right.GetPositions();
        if (leftPositions.Length > 0)
        {
            var sortedAll = new SortedSet<long>(leftPositions);
            sortedAll.UnionWith(rightPositions);
            var leftSideState = left.StartState.Invert(); // start in the pre segment area, so opposite state
            var rightSideState = right.StartState.Invert();
            int index = 0;
            foreach (var pos in sortedAll)
            {
                if (leftPositions.Contains(pos)) { leftSideState = leftSideState.Invert(); }
                if (rightPositions.Contains(pos)) { rightSideState = rightSideState.Invert(); }
                result.Add((pos, leftSideState, rightSideState));
                index++;
            }
        }
        return result;
    }
    /// <summary>
    /// Runs bool op over truth table (all stop positions of two sets, with the boolState for each side).
    /// </summary>
    public static long[] ApplyOpToTruthTable(List<(long, BoolState, BoolState)> data, Func<bool, bool, bool> operation)
    {
        var result = new List<long>();
        var lastResult = false;
        var hadFirstTrue = false;
        for (int i = 0; i < data.Count - 1; i++)
        {
            var item = data[i];
            var valid = BoolStateExtension.AreBool(item.Item2, item.Item3);
            var opResult = operation(item.Item2.BoolValue(), item.Item3.BoolValue());
            if (!hadFirstTrue && opResult == true)
            {
                result.Add(item.Item1);
                hadFirstTrue = true;
                lastResult = opResult;

            }
            else if (lastResult != opResult)
            {
                result.Add(item.Item1);
                lastResult = opResult;
            }
        }

        if (lastResult == true && result.Count > 0) // always close
        {
            result.Add(data.Last().Item1);
        }
        return result.ToArray();
    }

    #region Equality
    public virtual Focal Clone()
    {
        return new Focal(StartPosition, EndPosition);
    }
    public override bool Equals(object? obj)
    {
        return obj is Focal other && Equals(other);
    }
    public bool Equals(Focal? value)
    {
        return ReferenceEquals(this, value) || StartPosition.Equals(value.StartPosition) && EndPosition.Equals(value.EndPosition);
    }

    public static bool operator ==(Focal? a, Focal? b)
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

    public static bool operator !=(Focal? a, Focal? b)
    {
        return !(a == b);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = StartPosition.GetHashCode();
            hashCode = (hashCode * 397) ^ EndPosition.GetHashCode();
            return hashCode;
        }
    }
    #endregion
    public override string ToString() => $"[{StartPosition} : {EndPosition}]";
}
