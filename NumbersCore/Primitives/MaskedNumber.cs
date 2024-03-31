using NumbersCore.Utils;
using System;
using System.Diagnostics;

namespace NumbersCore.Primitives;
public class MaskedNumber : Number
{
    public MaskedFocal MaskedFocal => (MaskedFocal)Focal;

    public BoolState StartState => MaskedFocal.StartState;
    public bool IsEmpty => MaskedFocal.IsEmpty;
    public override int Count => StartState.IsTrue() ? MaskedFocal.Count / 2 : (MaskedFocal.Count - 1) / 2;

    /// <summary>
    /// A MaskedNumber is a single number with multiple masks, can be used for the result of bool operations.
    /// It can be empty, and always has a valid result for the full comparison (empty is a false segment).
    /// It is backed by a MaskedFocal, which holds the masks.
    /// </summary>
    public MaskedNumber(Polarity polarity, bool firstMaskIsTrue, params long[] maskPositions) :
        base(ValidatePositions(firstMaskIsTrue, maskPositions), polarity)
    {
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
    public void Clear()
    {
        MaskedFocal.Clear();
    }

    public BoolState GetMaskAt(float value)
    {
        var num = Domain.CreateNumberFromFloats(value, value, false);
        num.Polarity = Polarity;
        return MaskedFocal.GetMaskAtPosition(num.Focal.StartPosition);
    }

    public void ComputeWith(Number? num, OperationKind operationKind)
    {
    }

    /// <summary>
    /// Compares the positions of each number, regardless of direction.
    /// </summary>
    public bool IsSizeEqual(Number num) 
    {
        var result = false;
        if(Count == num.Count)
        {
            var p0 = Direction == 1 ? GetPositions() : GetPositions().Reverse();
            var p1 = num.Direction == 1 ? num.GetPositions() : num.GetPositions().Reverse();
            result = p0.SequenceEqual(num.GetPositions());
        }
        return result;
    }
    public bool IsPolarityEqual(Number num) => Polarity == num.Polarity;
    public bool IsDirectionEqual(Number num) => Direction == num.Direction;

    private static MaskedFocal ValidatePositions(bool firstMaskIsTrue, long[] maskPositions)
    {
        return new MaskedFocal(firstMaskIsTrue, maskPositions);
    }
}