﻿namespace NumbersCore.Primitives;

using NumbersCore.Operations;
using NumbersCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A group of indexed Focals. Can be empty.
/// </summary>
public class FocalGroup : Focal
{
    // todo: account for direction of focal line?
    // Perhaps the number store and bool segment capabilities should be separated?
    // The first is like a bidirectional relative accumulator, the second a non overlapping ordered group of segments.
    // - Q.Are non overlapping ordered elements a core type of value, or just a special bool case?
    //     the main difference is the bool case only allows forward direction for segments, while a path can meander. Both must be tip to tail.
    //     relative encoding may make more sense as this enforces tip to tail (2,4,3,1 vs 2,6,9,10)
    // todo: Maybe bool results need to be segments of alternating polarity? Currently there is a confusion between 'not considered' and 'false'.
    public virtual MathElementKind Kind => MathElementKind.FocalGroup;


    protected List<Focal> _focals = new List<Focal>();
    public int Count { get; private set; }
    public override bool IsDirty
    {
        get => _focals.Any(focal => focal.IsDirty);
        set
        {
            base.IsDirty = value;
            foreach (var focal in _focals) { focal.IsDirty = false; }
        }

    }
    public override long StartPosition
    {
        get => HasValue ? _focals[0].StartPosition : 0;
        set { if (HasValue) { _focals[0].StartPosition = value; } }
    }
    public override long EndPosition
    {
        get => Count > 0 ? _focals[Count - 1].EndPosition : 0;
        set { if (Count > 0) { _focals[Count - 1].EndPosition = value; } }
    }
    public bool HasValue => Count > 0;

    public FocalGroup(long startTickPosition, long endTickPosition) : base(startTickPosition, endTickPosition)
    {
    }

    public FocalGroup(IEnumerable<Focal> focals = null)
    {
        if (focals != null)
        {
            var positions = GetPositions(focals);
            RegenerateFocals(positions);
        }
    }

    public Focal this[int index]
    {
        get
        {
            Focal result = null;
            if (index >= 0 && index < Count)
            {
                result = _focals[index];
            }
            return result;
        }
    }
    public IEnumerable<Focal> Focals()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _focals[i];
        }
    }

    public override IEnumerable<long> Positions()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _focals[i].StartPosition;
            yield return _focals[i].EndPosition;
        }
    }
    public override long[] GetPositions() => Positions().ToArray();

	public override void SetPosition(int index, long value)
	{
        var focalIndex = index / 2;
        if (index >= 0 && focalIndex < _focals.Count)
        {
            if (index % 2 == 0)
            {
                _focals[focalIndex].StartPosition = value;
            }
            else
            {
                _focals[focalIndex].EndPosition = value;
            }
        }
	}
	public override long MaxExtent => Positions().Max();
    public override long MinExtent => Positions().Min();

    public override void Reset(long start, long end)
    {
        Clear();
        AddPosition(start, end);
    }
    public override void Reset(Focal left)
    {
        Clear();
        AddPosition(left);
    }
    public void Reset(Focal left, Focal right, OperationKind operationKind)
    {
        Clear();
        AddPosition(left);
        ComputeWith(right, operationKind);
    }
    /// <summary>
    /// Merge existing focals to each other by iterating over each one using the public operation.
    /// </summary>
    public void MergeRange(IEnumerable<Focal> focals)
    {
        var orgPositions = GetPositions(focals);
        for (int i = 0; i < orgPositions.Length; i += 2)
        {
            AddPosition(orgPositions[i + 0], orgPositions[i + 1]);
        }
    }
    public void ComputeWith(Focal focal, OperationKind operationKind)
    {
        // bool ops are just comparing state, so they don't care about direction or polarity
        // add, multiply etc, require polarity, so must happen on a higher level.
        // this assumes the two focals have the same resolutions
        if (operationKind.IsBoolOp())
        {
            var tt = BuildTruthTable(this, focal);
            var positions = ApplyOpToTruthTable(tt, BoolOperation.GetFunc(operationKind));
            RegenerateFocals(positions);
        }
        else if (operationKind.IsBoolCompare())
        {
            var maxA = MaxExtent;
            var minA = MinExtent;
            var maxB = focal.Max;// startB > endB ? startB : endB;
            var minB = focal.Min;// startB < endB ? startB : endB;
            long resultStart = 0;
            long resultEnd = 0;
            Clear();
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
                    // todo: truth of bool comparison compares with B here! Equations need causal direction.
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
                AddPosition(resultStart, resultEnd);
            }
            //var tt = BuildTruthTable(_positions.ToArray(), new long[] { start, end });
            //var positions = ApplyOpToTruthTable(tt, operationKind.GetFunc());
            //RegenerateFocals(positions);
        }
    }
    public Focal First() => Count > 0 ? _focals[0] : null;
    public Focal Last() => Count > 0 ? _focals[Count - 1] : null;

    public void AddPosition(long start, long end)
    {
        FillNextPosition(start, end);
    }
    public void AddPosition(Focal position) => FillNextPosition(position.StartPosition, position.EndPosition);
    public void AddPositions(Focal[] focals)
    {
        foreach (var focal in focals)
        {
            FillNextPosition(focal.StartPosition, focal.EndPosition);
        }
    }

    public void RemoveLastPosition()
    {
        if (Count > 0)
        {
            Count--;
        }
    }
    private static long[] GetPositions(IEnumerable<Focal> focals)
    {
        var result = new long[focals.Count() * 2];
        int i = 0;
        foreach (var focal in focals)
        {
            result[i++] = focal.StartPosition;
            result[i++] = focal.EndPosition;
        }
        return result;
    }
    private void RegenerateFocals(long[] positions)
    {
        Clear();
        for (int i = 0; i < positions.Length; i += 2)
        {
            var p0 = positions[i];
            // odd number of positions creates a point at end. Anything depending odd stores on this should use positions directly.
            var p1 = i + 1 < positions.Length ? positions[i + 1] : p0;
            var f = FillNextPosition(p0, p1);
        }
    }
    private void SelfGenerate()
    {
        var focals = new List<Focal>(Focals()); // these are overwritten in process, but internally works on positions, not focals.
        Clear();
        MergeRange(focals);
    }

    public long[] ApplyOpToTruthTable(List<(long, BoolState, BoolState)> data, Func<bool, bool, bool> operation)
    {
        var result = new List<long>();
        var lastResult = false;
        var hadFirstTrue = false;
        //foreach (var item in data)
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

    private Focal FillNextPosition(long startPosition, long endPosition)
    {
        Focal result;
        if (Count < _focals.Count)
        {
            result = _focals[Count];
            result.Reset(startPosition, endPosition);
        }
        else
        {
            result = new Focal(startPosition, endPosition);
            _focals.Add(result);
        }
        Count++;
        return result;
    }
    public void Clear()
    {
        Count = 0;
    }

    #region Equality
    public override bool Equals(object? obj)
    {
        return obj is FocalGroup other && Equals(other);
    }
    public bool Equals(FocalGroup? value)
    {
        var result = false;
        if (ReferenceEquals(this, value))
        {
            result = true;
        }
        else if (Count != value.Count)
        {
            result = false;
        }
        else
        {
            for (int i = 0; i < _focals.Count; i++)
            {
                if (_focals[i] != value._focals[i])
                {
                    result = false;
                    break;
                }
            }
        }
        return result;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _focals.GetHashCode();
            return hashCode;
        }
    }
    #endregion

    public override string ToString()
    {
        var result = $"fc:(";
        for (int i = 0; i < Count; i++)
        {
            var f = _focals[i];
            result += $"[{f.StartPosition}:{f.EndPosition}] ";
        }
        result += $")";
        return result;
    }

}