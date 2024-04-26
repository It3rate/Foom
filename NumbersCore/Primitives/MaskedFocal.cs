using Microsoft.VisualBasic;
using System.Diagnostics;

namespace NumbersCore.Primitives;
public class MaskedFocal : Focal // todo: maybe this should just do the internal masks if they are present, as an optional property of focal?
{
    public override int Count => StartState.IsTrue() ? _positions.Length / 2 : (_positions.Length - 1) / 2;


    /// <summary>
    /// A MaskedFocal is a single focal with multiple masks, can be used for the result of bool operations.
    /// The whole compared result is returned, and it can be 'empty' if it has two positions and starts 'false'.
    /// Positions always alternate between true and false, and must be in consecutive increasing order.
    /// An on/off/on sequence in the same position would be reduced to 'on'.
    /// </summary>
    public MaskedFocal(bool firstMaskIsTrue, params long[] maskPositions) : base()
    {
        StartState = firstMaskIsTrue ? BoolState.True : BoolState.False;
        Set(StartState, maskPositions);
    }

    public void Set(BoolState startState, params long[] positions)
    {
        StartState = startState;
        ValidatePositions(positions);
        _positions = (long[])positions.Clone();
        _tPositions = GetTPositions();
    }

    protected override double[] GetTPositions()
    {
        var positions = GetPositions();
        var result = new double[positions.Length - 2];
        var len = (double)LengthInTicks;
        var start = StartPosition;
        for (var i = 1; i < positions.Length - 1; i++)
        {
            result[i - 1] = (positions[i] - start) / len;
        }
        return result;
    }
    protected override void SetTPositions(double[] tPositions)
    {
        var len = (double)LengthInTicks;
        var start = StartPosition;
        for (var i = 0; i < tPositions.Length; i++)
        {
            var value = (long)(tPositions[i] * len) + start;
            SetPosition(i + 1, value);
        }
    }
    public override BoolState GetMaskAtPosition(long position)
    {
        var result = BoolState.Unknown;
        if (Direction == 1 && (position < StartPosition || position > EndPosition))
        {
            result = StartState;
            for (int i = 1; i < _positions.Length; i++)
            {
                if (position < _positions[i])
                {
                    break;
                }
                result.Invert();
            }
        }
        else if (Direction == -1 && (position > StartPosition || position < EndPosition))
        {
            result = StartState;
            for (int i = 1; i < _positions.Length; i++)
            {
                if (position > _positions[i])
                {
                    break;
                }
                result.Invert();
            }
        }
        return result;
    }
    public override void ClearInternalPositions()
    {
        _positions = [ 0, 0 ];
        StartState = BoolState.False;
    }

    private static Focal ValidatePositions(long[] maskPositions)
    {
        if (maskPositions.Length < 2)
        {
            throw new ArgumentException("MaskedNumber needs at least two positons.");
        }

        if (maskPositions[maskPositions.Length-1] == maskPositions[0]) // could happen if under resolution after divide, shouldn't throw away data
        {
            //throw new ArgumentException("MaskedNumbers need a direction (no equal values)");
        }
        else if (maskPositions[maskPositions.Length-1] > maskPositions[0]) // increasing order
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
            for (int i = 2; i < maskPositions.Length; i++) // decreasing order
			{
                if (maskPositions[i] >= maskPositions[i - 1])
                {
                    throw new ArgumentException("MaskedNumbers need to be ordered with no overlaps.");
                }
            }
        }
        return new Focal(maskPositions[0], maskPositions[maskPositions.Length - 1]);
    }

    #region Equality
    public override MaskedFocal Clone()
    {
        return new MaskedFocal(StartState.IsTrue(), (long[])_positions.Clone());
    }
    public override bool Equals(object? obj)
    {
        return obj is MaskedFocal other && Equals(other);
    }
    public bool Equals(MaskedFocal? value)
    {
        return ReferenceEquals(this, value) ||
            (value != null && StartState == value.StartState && _positions.SequenceEqual(value._positions));
    }
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = StartState.GetHashCode();
            hashCode = (hashCode * 397) ^ _positions.GetHashCode();
            return hashCode;
        }
    }
    #endregion
}
