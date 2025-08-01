﻿namespace NumbersCore.Primitives;

using NumbersCore.Operations;
using NumbersCore.Utils;
using System.Collections.Generic;
using System.Linq;

// Q. Why is this not just a domain?
// A. A domain defines a class of a certain situation, and there can be many instances.
// e.g. two domains can define an environment for 2D shapes.
// A group can define a shape, but the domain can have many shapes. It is important to keep these groups (instances) seperable.

/// <summary>
/// These are an ordered Group of numbers. They share a domain, but can have any polarity and direction. Can be empty.
/// </summary>
public class NumberGroup : Number, IMathElement
{
    public override MathElementKind Kind => MathElementKind.NumberGroup;

    public new bool IsDirty { get => _focalGroup.IsDirty; set => _focalGroup.IsDirty = value; } // base just calls this

    private FocalGroup _focalGroup => (FocalGroup)Focal;
    private List<Polarity> _polarityGroup { get; } = new List<Polarity>();
    public override int Count => _focalGroup.Count;

    public override Domain Domain // todo: lookup domain on PolyDomain
    {
        get => base.Domain;
        set => base.Domain = value;
    }

    public NumberGroup(Number targetNumber, params Focal[] focals) : base(new FocalGroup(), targetNumber.Polarity)
    {
        Domain = targetNumber.Domain;
        _focalGroup.MergeRange(focals);
    }
    public NumberGroup(Domain domain, Focal[] focals = null, Polarity[] polarities = null) : base(new FocalGroup(), Polarity.Aligned)
    {
        Domain = domain;
        if (focals != null)
        {
            int i = 0;
            foreach (Focal focal in focals)
            {
                var polarity = polarities != null && polarities.Length > i ? polarities[i] : Polarity.Aligned;
                _focalGroup.AddPosition(focal);
                _polarityGroup.Add(polarity);
            }
        }
    }
    public Polarity PolarityAt(int index) => _polarityGroup.Count > index ? _polarityGroup[index] : Polarity.Aligned;

    public override long[] GetPositions()
    {
        return _focalGroup.GetPositions();
    }

    public override Polarity[] Polarities => _polarityGroup.ToArray();
    public override int[] Directions => InternalNumbers().Select(obj => obj.Direction).ToArray();
    public override bool IsPolarityEqual(Number num)
    {
        var result = false;
        if (Count == num.Count)
        {
            result = _polarityGroup.SequenceEqual(num.Polarities);
        }
        return result;
    }
    public override bool IsDirectionEqual(Number num)
    {
        var result = false;
        if (Count == num.Count)
        {
            result = Directions.SequenceEqual(num.Directions);
        }
        return result;
    }


    public override IEnumerable<PRange> InternalRanges()
    {
        var i = 0;
        foreach (var focal in _focalGroup.Focals())
        {
            var val = Domain.GetValueOf(_focalGroup, PolarityAt(i));
            yield return val;

        }
    }
    public override IEnumerable<Number> InternalNumbers()
    {
        var i = 0;
        foreach (var focal in _focalGroup.Focals())
        {
            var nm = new Number(focal, PolarityAt(i));
            nm.Domain = Domain;
            yield return nm;
            i++;
        }
    }

    public void Reset(Number value, OperationKind operationKind)
    {
        ClearInternalPositions();
        if (value.IsValid)
        {
            _focalGroup.Reset(value.Focal);
            _polarityGroup.Add(value.Polarity);
        }
    }
    public override void ClearInternalPositions()
    {
        _focalGroup.Clear();
        _polarityGroup.Clear();
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
    public Focal First() => _focalGroup.First();
    public Polarity FirstPolarity() => _polarityGroup.Count > 0 ? _polarityGroup[0] : Polarity.None;
    public int FirstDirection() => FirstNumber()?.PolarityDirection ?? 0;
    public Focal Last() => _focalGroup.Last();
    // todo: account for polarity
    public Focal CreateFocalFromRange(PRange value) => Domain.CreateFocalFromRange(value);

    //public override void ComputeBoolOp(Number other, OperationKind operationKind)
    //{
    //    var (_, table) = SegmentedTable(Domain, true, this, other);
    //    var (focals, polarities, dir) = ApplyOpToSegmentedTable(table, operationKind);
    //    ClearInternalPositions();
    //    _focalGroup.AddPositions(focals);
    //    _polarityGroup.AddRange(polarities);
    //}
    //public override void ComputeBoolCompare(Number num, OperationKind operationKind)
    //{
    //    _focalGroup.ComputeWith(num.Focal, operationKind);
    //}
    public override void AddPosition(long start, long end)
    {
        _focalGroup.AddPosition(start, end);
    }

    public void RemoveLastPosition() => _focalGroup.RemoveLastPosition();
    public override void SetWith(Number other)
    {
        ClearInternalPositions();
        Polarity = other.Polarity;
        AddPosition(other.Value);
    }
    public void Reset(params Focal[] focals)
    {
        ClearInternalPositions();
        _focalGroup.MergeRange(focals);
        foreach (var focal in focals)
        {
            _polarityGroup.Add(Polarity.Aligned);
        }
    }
    public Number this[int index] => index < Count ? Domain.CreateNumber(_focalGroup[index], false) : null;
    public Focal FocalAt(int index) => index < Count ? _focalGroup[index] : null;

    public Number SumAll()
    {
        var result = Domain.CreateNumber(new Focal(0, 0));
        foreach (var number in InternalNumbers())
        {
            result.Add(number);
        }
        return result;
    }

    // todo: Add/Multiply all the public segments as well. Adding may be ok as is, multiply needs to interpolate stretches
    public override void Add(Number q) { base.Add(q); }
    public override void Subtract(Number q) { base.Subtract(q); }
    public override void Multiply(Number q) { base.Multiply(q); }
    public override void Divide(Number q) { base.Divide(q); }

    public void Not(Number q) { Reset(Focal.UnaryNot(q.Focal)); }

    #region Equality
    public override bool Equals(object? obj)
    {
        return obj is NumberGroup other && Equals(other);
    }
    public bool Equals(NumberGroup? value)
    {
        return ReferenceEquals(this, value) ||
            (
                Polarity == value.Polarity &&
                FocalGroup.Equals(this._focalGroup, value._focalGroup)
            );
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _focalGroup.GetHashCode() * 17 ^ ((int)Polarity + 27) * 397;
            return hashCode;
        }
    }
    #endregion


    public override string ToString()
    {
        string result;
        var val = Value;
        if (Polarity == Polarity.None)
        {
            result = $"x({val.Start:0.##}_{val.End:0.##})"; // no polarity, so just list values
        }
        else
        {
            var midSign = val.End > 0 ? " + " : " ";
            var pol = Polarity == Polarity.Inverted ? "~" : "";
            var start = val.Start == 0 ? "0" :
                val.Start == 1 ? "" :
                val.Start == -1 ? "-" :
                $"{val.Start:0.##}";

            var end = val.End == 0 ? "0" : $"{val.End:0.##}";
            result = $"{pol}({start}i{midSign}{end})";
        }
        return result;
    }
    //public override string ToString()
    //{
    //    var result = "ng(0)";
    //    if (Count > 0)
    //    {
    //        var v = Value;
    //        var midSign = v.End > 0 ? " + " : " ";
    //        result = IsAligned ?
    //            $"ng{Count}:({v.UnotValue:0.##}i {midSign}{v.UnitValue}r)" :
    //            $"ng{Count}:~({v.UnitValue:0.##}r {midSign}{v.UnotValue:0.##}i)";
    //    }
    //    return result;
    //}
}
