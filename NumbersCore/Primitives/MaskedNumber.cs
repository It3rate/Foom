using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersCore.Primitives;
public class MaskedNumber : Number
{
    private long[] _maskPositions;
    private BoolState StartState { get; }

    public bool IsEmpty => StartState == BoolState.False && _maskPositions.Length == 0;
    public long First => _maskPositions[0];
    public long Last => _maskPositions[_maskPositions.Length - 1];

    /// <summary>
    /// A MaskedNumber is a single number with multiple masks, can be used for the result of bool operations.
    /// The whole compared result is returned, and it can be 'empty' if it has two positions and starts 'false'.
    /// Positions always alternate between true and false, and must be in consecutive increasing order.
    /// An on/off/on sequence in the same position would be reduced to 'on'.
    /// </summary>
    public MaskedNumber(Polarity polarity, bool firstMaskIsTrue, params long[] maskPositions) : base(ValidatePositions(maskPositions), polarity)
    {
        StartState = firstMaskIsTrue ? BoolState.True : BoolState.False;
        _maskPositions = maskPositions;
    }

    public BoolState GetMaskAtPosition(long position)
    {
        var result = BoolState.Unknown;
        if (Focal.Direction == 1 && (position < First || position > Last))
        {
            result = StartState;
            for (int i = 1; i < _maskPositions.Length; i++)
            {
                if (position < _maskPositions[i])
                {
                    break;
                }
                result.Invert();
            }
        }
        else if(Focal.Direction == -1 && (position > First || position < Last))
        {
            result = StartState;
            for (int i = 1; i < _maskPositions.Length; i++)
            {
                if (position > _maskPositions[i])
                {
                    break;
                }
                result.Invert();
            }
        }
        return result;
    }



    private static Focal ValidatePositions(long[] maskPositions)
    {
        if (maskPositions.Length < 2)
        {
            throw new ArgumentException("MaskedNumber needs at least two positons.");
        }

        if (maskPositions[1] == maskPositions[0])
        {
            throw new ArgumentException("MaskedNumbers need a direction (no equal values)");
        }
        else if (maskPositions[1] > maskPositions[0])
        {
            for (int i = 2; i < maskPositions.Length; i++)
            {
                if (maskPositions[i] <= maskPositions[i - 1])
                {
                    throw new ArgumentException("MaskedNumbers need to be ordered with no overlaps.");
                }
            }
        }
        else
        {
            for (int i = 2; i < maskPositions.Length; i++)
            {
                if (maskPositions[i] >= maskPositions[i - 1])
                {
                    throw new ArgumentException("MaskedNumbers need to be ordered with no overlaps.");
                }
            }
        }
        return new Focal(maskPositions[0], maskPositions[maskPositions.Length - 1]);
    }
}
