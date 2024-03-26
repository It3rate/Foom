using NumbersCore.Utils;

namespace NumbersCore.Primitives;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ordered numbers, array style, clamped to base Number size. Optionally can be merged and ordered with bool Operations.
/// All Numbers are probably NumberChain, with zero based Unit and Unot values cancelling on overlap.
/// Can represent a boolean evaluation of two numbers that result in non overlapping ordered segments, which share polarity and domain.
/// Can also represent the start/endpoints of mergable repeats, like 3+3+3 or 3*3*3. 
/// </summary>
public class NumberChain : Number, IMathElement
{
    // should be able to access and update proportionally, where 8 even subdivisions will remain so even when changing the base number.
    // they can be recorded as on/off/on/off along the line, allowing construction of 2D continuous paths (though this would just be an optimization).
    // knowing if focals are touching (contiguous) matters.
    // all clamping, overlap removal and repears should be done in the focalSet
    public override MathElementKind Kind => MathElementKind.NumberChain;

    public new bool IsDirty { get => _focalChain.IsDirty; set => _focalChain.IsDirty = value; } // base just calls this

    private FocalChain _focalChain => (FocalChain)Focal;
    private List<Polarity> _polarityChain { get; } = new List<Polarity>();
    public int Count => _focalChain.Count;

    public override Domain Domain // todo: lookup domain on PolyDomain
    {
        get => base.Domain;
        set => base.Domain = value;
    }

    public NumberChain(Number targetNumber, params Focal[] focals) : base(new FocalChain(), targetNumber.Polarity)
    {
        Domain = targetNumber.Domain;
        _focalChain.MergeRange(focals);
    }
    public NumberChain(Domain domain, Focal[] focals = null, Polarity[] polarities = null) : base(new FocalChain(), Polarity.Aligned)
    {
        Domain = domain;
        if (focals != null)
        {
            int i = 0;
            foreach (Focal focal in focals)
            {
                var polarity = polarities != null && polarities.Length > i ? polarities[i] : Polarity.Aligned;
                _focalChain.AddPosition(focal);
                _polarityChain.Add(polarity);
            }
        }
    }
    public Polarity PolarityAt(int index) => _polarityChain.Count > index ? _polarityChain[index] : Polarity.Aligned;

    public override long[] GetPositions()
    {
        return _focalChain.GetPositions();
    }
    public override IEnumerable<PRange> InternalRanges()
    {
        var i = 0;
        foreach (var focal in _focalChain.Focals())
        {
            var val = Domain.GetValueOf(_focalChain, PolarityAt(i));
            yield return val;

        }
    }
    public override IEnumerable<Number> InternalNumbers()
    {
        var i = 0;
        foreach (var focal in _focalChain.Focals())
        {
            var nm = new Number(focal, PolarityAt(i));
            nm.Domain = Domain;
            yield return nm;
            i++;
        }
    }

    public void Reset(Number value, OperationKind operationKind)
    {
        Clear();
        if (value.IsValid)
        {
            _focalChain.Reset(value.Focal);
            _polarityChain.Add(value.Polarity);
        }
    }
    public void Clear()
    {
        _focalChain.Clear();
        _polarityChain.Clear();
    }

    public Number FirstNumber()
    {
        Number result = null;
        if (Count > 0)
        {
            result = new Number(First(), PolarityAt(0));
            result.Domain = Domain;
        }
        return result;
    }
    public Focal First() => _focalChain.First();
    public Polarity FirstPolarity() => _polarityChain.Count > 0 ? _polarityChain[0] : Polarity.None;
    public int FirstDirection() => FirstNumber()?.PolarityDirection ?? 0;
    public Focal Last() => _focalChain.Last();
    // todo: account for polarity
    public Focal CreateFocalFromRange(PRange value) => Domain.CreateFocalFromRange(value);

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
                    _focalChain.ComputeWith(Focal, operationKind); // change arrow dir
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
                ComputeBoolCompare(num.Focal, operationKind);
            }
            else if (operationKind.IsBinary())
            {
                switch (operationKind)
                {
                    case OperationKind.Add:
                        AddValue(num);
                        break;
                    case OperationKind.Subtract:
                        SubtractValue(num);
                        break;
                    case OperationKind.Multiply:
                        MultiplyValue(num);
                        break;
                    case OperationKind.Divide:
                        DivideValue(num);
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
    public void ComputeWith(Focal focal, OperationKind operationKind) => _focalChain.ComputeWith(focal, operationKind);
    public void ComputeWith(long start, long end, OperationKind operationKind) => ComputeWith(new Focal(start, end), operationKind);
    public void ComputeWith(PRange range, OperationKind operationKind)
    {
        var focal = Domain.CreateFocalFromRange(range);
        _focalChain.ComputeWith(focal, operationKind);
    }
    public void ComputeBoolOp(Number other, OperationKind operationKind)
    {
        // bool ops are just comparing state, so they don't care about direction or polarity, thus happen on focals
        // however, this requires they have the same resolutions, so really should be on number chains.
        //_focalChain.ComputeWith(focal, operationKind);
        var (_, table) = SegmentedTable(Domain, this, other);
        var (focals, polarities) = ApplyOpToSegmentedTable(table, operationKind);
        Clear();
        _focalChain.AddPositions(focals);
        _polarityChain.AddRange(polarities);
    }
    public void ComputeBoolCompare(Focal focal, OperationKind operationKind)
    {
        _focalChain.ComputeWith(focal, operationKind);
    }
    public void AddPosition(long start, long end)
    {
        _focalChain.AddPosition(start, end);
    }
    public void AddPosition(Focal focal)
    {
        AddPosition(focal.StartPosition, focal.EndPosition);
    }
    public void AddPosition(Number num)
    {
        AddPosition(num.Focal.StartPosition, num.Focal.EndPosition);
    }
    public void AddPosition(PRange range)
    {
        var focal = Domain.CreateFocalFromRange(range);
        AddPosition(focal.StartPosition, focal.EndPosition);
    }
    public void RemoveLastPosition() => _focalChain.RemoveLastPosition();

    public void Reset(params Focal[] focals)
    {
        Clear();
        _focalChain.MergeRange(focals);
        foreach (var focal in focals)
        {
            _polarityChain.Add(Polarity.Aligned);
        }
    }
    public Number this[int index] => index < Count ? Domain.CreateNumber(_focalChain[index], false) : null;
    public Focal FocalAt(int index) => index < Count ? _focalChain[index] : null;

    public Number SumAll()
    {
        var result = Domain.CreateNumber(new Focal(0, 0));
        foreach (var number in InternalNumbers())
        {
            result.AddValue(number);
        }
        return result;
    }

    // todo: Add/Multiply all the internal segments as well. Adding may be ok as is, multiply needs to interpolate stretches
    public override void AddValue(Number q) { base.AddValue(q); }
    public override void SubtractValue(Number q) { base.SubtractValue(q); }
    public override void MultiplyValue(Number q) { base.MultiplyValue(q); }
    public override void DivideValue(Number q) { base.DivideValue(q); }

    public void Not(Number q) { Reset(Focal.UnaryNot(q.Focal)); }


    /// <summary>
    /// NumberChains can have overlapping numbers, so this segmented version returns all partial ranges for each possible segment.
    /// Assumes aligned domains.
    /// </summary>
    public static (long[], List<Number[]>) SegmentedTable(Domain domain, params Number[] numbers)
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
                internalNumberSets.Add(new Number[] { });
            }
        }
        var positions = sPositions.ToArray();
        Number partial;
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
                        else
                        {
                            matches.Add(CreateSubsegment(domain, focal, Polarity.None));
                        }
                    }
                }
            }
            result.Add(matches.ToArray());
        }

        return (positions, result);
    }
    private static Number CreateSubsegment(Domain domain, Focal focal, Polarity polairty = Polarity.None)
    {
        var result = new Number(focal.Clone(), polairty); // false
        result.Domain = domain;
        return result;
    }
    private (Focal[], Polarity[]) ApplyOpToSegmentedTable(List<Number[]> data, OperationKind operation)
    {
        var focals = new List<Focal>();
        var polarities = new List<Polarity>();
        Focal lastFocal = null;
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

    //// truth table only acts on valid parts of segments. Remember a -10i+5 has two parts, 0 to -10i and 0 to 5. This is the area bools apply to.
    //private List<(long, BoolState, BoolState)> BuildTruthTable(Number right)
    //{
    //    var leftRanges = InternalRanges();
    //    var rightRanges = right.InternalRanges();
    //    //var pos = _focalChain.BuildTruthTable(leftPositions, rightPositions);
    //    var result = new List<(long, BoolState, BoolState)>();
    //    if (left.Length > 0)
    //    {
    //        var sortedAll = new SortedSet<long>(left);
    //        sortedAll.UnionWith(right);
    //        var leftSideState = BoolState.False;
    //        var rightSideState = BoolState.False;
    //        int index = 0;
    //        foreach (var pos in sortedAll)
    //        {
    //            if (left.Contains(pos)) { leftSideState = leftSideState.Invert(); }
    //            if (right.Contains(pos)) { rightSideState = rightSideState.Invert(); }
    //            //var left = index == 0 ? BoolState.Underflow : leftSideState;
    //            //var right = index == sortedAll.Count - 1 ? BoolState.Overflow : rightSideState;
    //            result.Add((pos, leftSideState, rightSideState));
    //            index++;
    //        }
    //    }
    //    return result;
    //}

    public static bool operator ==(NumberChain a, NumberChain b)
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

    public static bool operator !=(NumberChain a, NumberChain b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        return obj is NumberChain other && Equals(other);
    }
    public bool Equals(NumberChain value)
    {
        return ReferenceEquals(this, value) ||
            (
                Polarity == value.Polarity &&
                FocalChain.Equals(this._focalChain, value._focalChain)
            );
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _focalChain.GetHashCode() * 17 ^ ((int)Polarity + 27) * 397;
            return hashCode;
        }
    }

    public override string ToString()
    {
        var v = Value;
        var midSign = v.End > 0 ? " + " : " ";
        var result = IsAligned ?
            $"nc:({v.UnotValue:0.##}i {midSign}{v.UnitValue}r)" :
            $"nc:~({v.UnitValue:0.##}r {midSign}{v.UnotValue:0.##}i)";
        return result;
    }
}
