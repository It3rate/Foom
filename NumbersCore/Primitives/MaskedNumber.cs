using NumbersCore.Utils;
using System;
using System.Diagnostics;

namespace NumbersCore.Primitives;
public class MaskedNumber : Number
{
    public MaskedFocal MaskedFocal => (MaskedFocal)Focal;

    public override BoolState StartState => MaskedFocal.StartState;
    public bool IsEmpty => MaskedFocal.IsEmpty;
    public override int Count => StartState.IsTrue() ? MaskedFocal.Count / 2 : (MaskedFocal.Count - 1) / 2;

    /// <summary>
    /// A MaskedNumber is a single number with multiple masks, can be used for the result of bool operations.
    /// It can be empty, and always has a valid result for the full comparison (empty is a false segment).
    /// It is backed by a MaskedFocal, which holds the masks.
    /// </summary>
    public MaskedNumber(Polarity polarity, bool firstMaskIsTrue, params long[] maskPositions) :
        base(ValidatePositions(firstMaskIsTrue, maskPositions), polarity)  { }
    public MaskedNumber(Number num) : // todo: guard for unmerged number group
        base(ValidatePositions(num.StartState.IsTrue(), num.GetPositions()), num.Polarity) { }
    private MaskedNumber(Polarity polarity, MaskedFocal maskedFocal) : base(maskedFocal, polarity) { }

    public override void SetWith(Number num)
    {
        //Domain = num.Domain;
        Polarity = num.Polarity;
        Focal = ValidatePositions(num.StartState.IsTrue(), num.GetPositions());
    }
    public override void SetWith(Focal[] focals, Polarity[] polarities)
    {
        ClearInternalPositions();
        Polarity = polarities.FirstOrDefault((value)=>value.HasPolarity(), Polarity.None);
        if (focals.Length > 0 && polarities.Length > 0)
        {
            List<long> positions = new List<long>();
            foreach(var focal in focals)
            {
                positions.Add(focal.StartPosition);
                positions.Add(focal.EndPosition);
            }
            MaskedFocal.Set(BoolState.True, positions.ToArray());
        }
    }

    public IEnumerable<long> Positions()
    {
        foreach (var pos in MaskedFocal.Positions())
        {
            yield return pos;
        }
    }
    public override long[] GetPositions()
    {
        return MaskedFocal.GetPositions();
    }

    public override IEnumerable<Number> InternalNumbers()
    {
        var positions = MaskedFocal.GetPositions();
        var start = StartState.IsTrue() ? 0 : 1;
        for(int i = start; i < positions.Length - 1; i += 2)
        {
            yield return CreateSubsegment(Domain, new Focal(positions[i], positions[i + 1]), Polarity);
        }
    }
    public override void ClearInternalPositions()
    {
        MaskedFocal.Clear();
    }
    public override void AddPosition(long start, long end)
    {
        MaskedFocal.Set(BoolState.True, start, end);
    }

    public BoolState GetMaskAt(float value)
    {
        var num = Domain.CreateNumberFromFloats(value, value, false);
        num.Polarity = Polarity;
        return MaskedFocal.GetMaskAtPosition(num.Focal.StartPosition);
    }

    public void ComputeWith(Number? num, OperationKind operationKind)
    {
        base.ComputeWith(num, operationKind);
    }

    private static MaskedFocal ValidatePositions(bool firstMaskIsTrue, long[] maskPositions)
    {
        return new MaskedFocal(firstMaskIsTrue, maskPositions);
    }

    #region Equality
    public new MaskedNumber Clone(bool addToStore = true)
    {
        MaskedNumber result = new MaskedNumber(Polarity, MaskedFocal.Clone());
        Domain.AddNumber(result, addToStore);
        return result;
    }
    public override bool Equals(object? obj)
    {
        return obj is MaskedNumber other && Equals(other);
    }
    public bool Equals(MaskedNumber? value)
    {
        if (value is null) { return false; }
        return ReferenceEquals(this, value) ||
                (
                Polarity == value.Polarity &&
                Focal.Equals(this.MaskedFocal, value.MaskedFocal)
                );
    }
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = MaskedFocal.GetHashCode() * 17 ^ ((int)Polarity + 27) * 33;
            return hashCode;
        }
    }
    #endregion
}