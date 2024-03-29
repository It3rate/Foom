﻿using Microsoft.VisualBasic;

namespace NumbersCore.Primitives;
public class MaskedFocal : Focal
{
    private long[] _positions;
    public bool IsEmpty => StartState == BoolState.False && _positions.Length == 0;

    public virtual long StartPosition
    {
        get => _positions[0];
        set
        {
            if (_positions[0] != value)
            {
                _positions[0] = value;
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
                IsDirty = true;
            }
        }
    }
    /// <summary>
    /// A MaskedFocal is a single focal with multiple masks, can be used for the result of bool operations.
    /// The whole compared result is returned, and it can be 'empty' if it has two positions and starts 'false'.
    /// Positions always alternate between true and false, and must be in consecutive increasing order.
    /// An on/off/on sequence in the same position would be reduced to 'on'.
    /// </summary>
    public MaskedFocal(bool firstMaskIsTrue, params long[] maskPositions) : base()
    {
        ValidatePositions(maskPositions);
        StartState = firstMaskIsTrue ? BoolState.True : BoolState.False;
        _positions = maskPositions;
    }

    public void Set(BoolState startState, params long[] positions)
    {
        StartState = startState;
        _positions = (long[])positions.Clone();
    }

    public override IEnumerable<long> Positions()
    {
        for (int i = 0; i < _positions.Length; i++)
        {
            yield return _positions[i];
        }
    }
    public override long[] GetPositions() => (long[])_positions.Clone();

    public BoolState GetMaskAtPosition(long position)
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
    public void Clear()
    {
        _positions = new long[] { 0, 0 };
        StartState = BoolState.False;
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
