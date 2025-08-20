using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public static RectOrientation FromNearestPoint(SKRect rect, SKPoint point, int xRightDirection = 1, int yDownDirection = 1)
    {
        var horz = RectDirections.Left;
        var vert = RectDirections.Top;
        if (((xRightDirection > 0) && (point.X > rect.MidX)) || ((xRightDirection < 0) && (point.X < rect.MidX)))
        {
            horz = RectDirections.Right;
        }

        if (((yDownDirection < 0) && (point.Y < rect.MidY)) || ((yDownDirection > 0) && (point.Y > rect.MidY)))
        {
            vert = RectDirections.Bottom;
        }
        return new RectOrientation( vert | horz, xRightDirection, yDownDirection);
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
	public static bool HasLeft(this RectDirections state) => (state & RectDirections.Left) != 0;
	public static bool HasRight(this RectDirections state) => (state & RectDirections.Right) != 0;
	public static bool HasTop(this RectDirections state) => (state & RectDirections.Top) != 0;
    public static bool HasBottom(this RectDirections state) => (state & RectDirections.Bottom) != 0;
    public static bool IsCorner(this RectDirections state) => 
        state == RectDirections.BottomLeft || state == RectDirections.BottomRight || 
        state == RectDirections.TopLeft || state == RectDirections.TopRight;

    public static bool CornersFormEdge(this RectDirections state, RectDirections otherCorner)
    {
        var result = false;
        if (state.IsCorner() && otherCorner.IsCorner())
        {
            result = (state & otherCorner) > 0;
        }
        return result;
    }
    public static bool CornersFormDiagonal(this RectDirections state, RectDirections otherCorner)
    {
        var result = false;
        if (state.IsCorner() && otherCorner.IsCorner())
        {
            result = (state & otherCorner) == 0;
        }
        return result;
    }
    public static int Clockwise45DegreeStepsTo(this RectDirections start, RectDirections end)
    {
        int startIndex = GetDirectionIndex(start);
        int endIndex = GetDirectionIndex(end);

        return (endIndex - startIndex + 8) % 8;
    }
    public static int CounterClockwise45DegreeStepsTo(this RectDirections start, RectDirections end)
    {
        int startIndex = GetDirectionIndex(start);
        int endIndex = GetDirectionIndex(end);

        return (startIndex - endIndex + 8) % 8;
    }

    public static int CWDegreesTo(this RectDirections start, RectDirections end)
    {
        return start.Clockwise45DegreeStepsTo(end) * 45;
    }
    public static float CWRadiansTo(this RectDirections start, RectDirections end)
    {
        return start.Clockwise45DegreeStepsTo(end) * 45 * MathF.PI / 180f;
    }
    public static int CCWDegreesTo(this RectDirections start, RectDirections end)
    {
        return start.CounterClockwise45DegreeStepsTo(end) * 45;
    }
    public static float CCWRadiansTo(this RectDirections start, RectDirections end)
    {
        return start.CounterClockwise45DegreeStepsTo(end) * 45 * MathF.PI / 180f;
    }

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
    public static RectDirections TRBLFromIndex(int index)
    {
        var result = RectDirections.None;
        switch (index)
        {
            case 0:
                result = RectDirections.Top;
                break;
            case 1:
                result = RectDirections.Right;
                break;
            case 2:
                result = RectDirections.Bottom;
                break;
            case 3:
                result = RectDirections.Left;
                break;
        }
        return result;
    }
    public  static int GetDirectionIndex(this RectDirections state)
    {
        return state switch
        {
            RectDirections.Top => 0,
            RectDirections.TopRight => 1,
            RectDirections.Right => 2,
            RectDirections.BottomRight => 3,
            RectDirections.Bottom => 4,
            RectDirections.BottomLeft => 5,
            RectDirections.Left => 6,
            RectDirections.TopLeft => 7,
            _ => 0
        };
    }
}
