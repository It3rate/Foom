using NumbersCore.Utils;
using System.Diagnostics;

namespace NumbersCore.Primitives;
public class MaskedNumber : Number
{
    public MaskedFocal MaskedFocal => (MaskedFocal)Focal;

    public BoolState StartState => MaskedFocal.StartState;
    public bool IsEmpty => MaskedFocal.IsEmpty;

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

    private static MaskedFocal ValidatePositions(bool firstMaskIsTrue, long[] maskPositions)
    {
        return new MaskedFocal(firstMaskIsTrue, maskPositions);
    }
}
