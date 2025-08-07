using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace NumbersSkia.Utils;

public class RectOrientation
{
    public RectDirections State { get; }
    private int _xRightDirection;
    private int _yDownDirection;
    public RectOrientation( RectDirections state, int xRightDirection = 1, int yDownDirection = 1)
    {
        State = state;
        _xRightDirection = xRightDirection;
        _yDownDirection = yDownDirection;
    }
    public SKPoint DirectionToCenter => State.DirectionToCenter(_xRightDirection, _yDownDirection);
}

[Flags]
public enum RectDirections
{
    None = 0,
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8,

    BottomLeft = Bottom | Left,
    BottomRight = Bottom | Right,
    TopLeft = Top | Left,
    TopRight = Top | Right,
}
public static class RectDirectionsExtensions
{
    public static SKPoint DirectionToCenter(this RectDirections state, int xRightDirection = 1, int yDownDirection = 1)
    {
        var result = SKPoint.Empty;

        if ((state & RectDirections.Left) != 0)
        {
            result.X = xRightDirection;
        }
        else if ((state & RectDirections.Right) != 0)
        {
            result.X = -xRightDirection;
        }

        if ((state & RectDirections.Top) != 0)
        {
            result.Y = yDownDirection;
        }
        else if ((state & RectDirections.Bottom) != 0)
        {
            result.Y = -yDownDirection;
        }

        return result;
    }
}
