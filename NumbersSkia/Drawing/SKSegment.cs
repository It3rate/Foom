using NumbersSkia.Utils;
using NumbersCore.Utils;
using SkiaSharp;

namespace NumbersSkia.Drawing;

public class SKSegment
{
    public SKPoint StartPoint { get; set; }
    public SKPoint EndPoint { get; set; }
    public SKPoint Midpoint
    {
        get => Vector.Divide(2f) + StartPoint;
        set
        {
            var dif = Midpoint - StartPoint;
            StartPoint = value - dif;
            EndPoint = value + dif;
        }
    }
    public SKPoint InvertedEndPoint => PointAlongLine(-1);

    public static readonly SKSegment Empty = new SKSegment(SKPoint.Empty, SKPoint.Empty);

    public SKSegment(SKPoint start, SKPoint end)
    {
        StartPoint = start;
        EndPoint = end;
    }

    public void Reset(SKPoint startPoint, SKPoint endPoint) { StartPoint = startPoint; EndPoint = endPoint; }

    public SKSegment(float x0, float y0, float x1, float y1) : this(new SKPoint(x0, y0), new SKPoint(x1, y1)) { }

    public SKPoint[] Points => new[] { StartPoint, EndPoint };

    public SKSegment Clone() => new SKSegment(StartPoint, EndPoint);

    public SKSegment InsetSegment(float pixels) => new SKSegment(PointAlongLine(pixels / Length), PointAlongLine(1f - pixels / Length));

    public static SKSegment operator +(SKSegment a, SKPoint value)
    {
        var result = a.Clone();
        result.StartPoint += value;
        result.EndPoint += value;
        return result;
    }
    public static SKSegment operator -(SKSegment a, SKPoint value)
    {
        var result = a.Clone();
        result.StartPoint -= value;
        result.EndPoint -= value;
        return result;
    }

    public static SKSegment operator +(SKSegment a, float value)
    {
        a.StartPoint = a.StartPoint.Add(value);
        a.EndPoint = a.EndPoint.Add(value);
        return a.Clone();
    }
    public static SKSegment operator -(SKSegment a, float value)
    {
        a.StartPoint = a.StartPoint.Subtract(value);
        a.EndPoint = a.EndPoint.Subtract(value);
        return a.Clone();
    }
    public static SKSegment operator *(SKSegment a, float value)
    {
        a.StartPoint = a.StartPoint.Multiply(value);
        a.EndPoint = a.EndPoint.Multiply(value);
        return a.Clone();
    }
    public static SKSegment operator /(SKSegment a, float value)
    {
        a.StartPoint = a.StartPoint.Divide(value);
        a.EndPoint = a.EndPoint.Divide(value);
        return a.Clone();
    }

    public float Length => (float)Math.Sqrt((EndPoint.X - StartPoint.X) * (EndPoint.X - StartPoint.X) + (EndPoint.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y));
    public float AbsLength => (float)Math.Abs(Length);
    public float LengthSquared => (EndPoint.X - StartPoint.X) * (EndPoint.X - StartPoint.X) + (EndPoint.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y);
    public float NonZeroLength => Length == 0 ? 0.001f : Length;
    public SKPoint PointAlongLine(double t, float offsetT = 0)
    {
        return new SKPoint(
            (EndPoint.X - StartPoint.X) * ((float)t + offsetT) + StartPoint.X,
            (EndPoint.Y - StartPoint.Y) * ((float)t + offsetT) + StartPoint.Y);
    }
    public (SKPoint, SKPoint) PerpendicularLine(float t, float offset) => (PointAlongLine(t), OrthogonalPoint(PointAlongLine(t), offset));
    public SKPoint RelativeOffset(float offset) => OrthogonalPoint(SKPoint.Empty, offset);
    public SKPoint OffsetAlongLine(float t, float offset) => OrthogonalPoint(PointAlongLine(t), offset);
    public SKPoint DistanceOffsetAlongLine(float offsetAlong, float offsetOrthogonal)
    {
        var shiftSeg = ShiftOffLine(offsetOrthogonal);
        return shiftSeg.SKPointFromStart(offsetAlong);
    }
    public SKPoint SKPointFromStart(float dist) => PointAlongLine(dist / Math.Max(MathFUtils.tolerance, Length));
    public SKPoint SKPointFromEnd(float dist) => PointAlongLine(1 - dist / Math.Max(MathFUtils.tolerance, Length));
    public SKPoint Vector => EndPoint - StartPoint;
    public SKSegment ShiftOffLine(float shift)
    {
        var p0 = OrthogonalPoint(StartPoint, shift);
        var p1 = OrthogonalPoint(EndPoint, shift);
        return new SKSegment(p0, p1);
    }
    public SKSegment ShiftOffLine(SKSegment landmark, float shift)
    {
        var p0 = landmark.OrthogonalPoint(StartPoint, shift);
        var p1 = landmark.OrthogonalPoint(EndPoint, shift);
        return new SKSegment(p0, p1);
	}
	public SKSegment LeftToRight() => StartPoint.X < EndPoint.X ? this : Inverted();
	public SKSegment RightToLeft() => StartPoint.X < EndPoint.X ? Inverted() : this;
	public SKSegment Inverted() => new SKSegment(EndPoint, StartPoint);
    public SKSegment SegmentAlongLine(PRange ratios, float offsetT = 0) => SegmentAlongLine(ratios.StartF, ratios.EndF, offsetT);
    public SKSegment SegmentAlongLine(float startT, float endT, float offsetT = 0)
    {
        var startPoint = PointAlongLine(startT, offsetT);
        var endPoint = PointAlongLine(endT, offsetT);
        return new SKSegment(startPoint, endPoint);
    }
    public void FlipAroundStartPoint()
    {
        EndPoint = StartPoint + (StartPoint - EndPoint);
    }
    public void Reverse()
    {
        var temp = EndPoint;
        EndPoint = StartPoint;
        StartPoint = temp;
    }

    public SKSegment GetMeasuredSegmentByMidpoint(float length)
    {
        var ratio = length / Length / 2f;
        var p0 = PointAlongLine(-ratio, 0.5f);
        var p1 = PointAlongLine(ratio, 0.5f);
        return new SKSegment(p0, p1);
    }

    public bool IsHorizontal() => Math.Abs(StartPoint.Y - EndPoint.Y) < MathFUtils.tolerance;
    public bool IsVertical() => Math.Abs(StartPoint.X - EndPoint.X) < MathFUtils.tolerance;
    public bool IsCollinearTo(SKSegment segment)
    {
        var sDist = SquaredDistanceTo(segment.StartPoint, false);
        var eDist = SquaredDistanceTo(segment.EndPoint, false);
        return Math.Abs(sDist) < MathFUtils.tolerance && Math.Abs(eDist) < MathFUtils.tolerance;
    }
    public bool IsParallelTo(SKSegment segment)
    {
        var angleA = AngleRadians;
        var angleB = segment.AngleRadians;
        var rem = Math.IEEERemainder(angleA - angleB, MathFUtils.PI);
        return Math.Abs(rem) < MathFUtils.tolerance;
    }
    public bool IsPerpendicularTo(SKSegment segment)
    {
        var angleA = AngleRadians;
        var angleB = segment.AngleRadians;
        var rem = Math.IEEERemainder(angleA - angleB, MathF.PI);
        return Math.Abs(Math.Abs(rem) - MathFUtils.HalfPI) < MathFUtils.tolerance;
    }
    public float AngleRadians
    {
        get
        {
            var dif = Vector;
            return (float)Math.Atan2(dif.Y, dif.X);
        }
        set
        {
            var radius = Length;
            var sp = StartPoint;
            var ep = new SKPoint((float)Math.Cos(value) * radius + sp.X, (float)Math.Sin(value) * radius + sp.Y);
            EndPoint = ep;
        }
    }
    public float AngleDegrees
    {
        get => MathFUtils.ToDegrees(AngleRadians);
        set => AngleRadians = MathFUtils.ToRadians(value);
    }
    private static float SnapAngle(float radians, int stepDegrees = 15)
    {
        var degrees = MathFUtils.ToDegrees(radians) + 360;
        var rem = degrees % stepDegrees;
        var centerOffset = rem > stepDegrees / 2f ? stepDegrees : 0f;
        var clamp = (int)(degrees / stepDegrees) * stepDegrees + centerOffset;
        return MathFUtils.ToRadians(clamp); ;
    }

    public void SetAngleAroundMidpoint(float radians, int stepDegrees = 1)
    {
        var clampedRadians = SnapAngle(radians, stepDegrees);
        var radius = Length / 2f;
        var mp = Midpoint;
        var ep = new SKPoint((float)Math.Cos(clampedRadians) * radius + mp.X, (float)Math.Sin(clampedRadians) * radius + mp.Y);
        StartPoint = mp - (ep - mp);
        EndPoint = ep;

    }

    public SKPoint SnapAngleToStep(int stepDegrees = 15)
    {
        AngleRadians = SnapAngle(AngleRadians, stepDegrees);
        return EndPoint;
    }

    public SKPoint OrthogonalPoint(SKPoint pt, float offset)
    {
        var angle = Vector.Angle();
        return pt.PointAtRadiansAndDistance(angle + (float)Math.PI / 2f, offset);
    }

    public float DistanceTo(SKPoint p, bool clamp = true)
    {
        var pp = ProjectPointOnto(p, clamp);
        return p.DistanceTo(pp);
    }
    public float SquaredDistanceTo(SKPoint p, bool clamp = true)
    {
        var pp = ProjectPointOnto(p, clamp);
        return p.SquaredDistanceTo(pp);
    }

    public SKPoint ProjectPointOnto(SKPoint p, bool clamp = true)
    {
        SKPoint result = StartPoint; // if seg is point, return point
        if (Length != 0)
        {
            var e1 = Vector;
            var e2 = p - StartPoint;
            var dp = e1.DotProduct(e2);
            var len2 = e1.SquaredLength();
            if (len2 < 0.001f)
            {
                result = p;
            }
            else
            {
                var x = StartPoint.X + dp * e1.X / len2;
                var y = StartPoint.Y + dp * e1.Y / len2;
                if (clamp)
                {
                    x = x < StartPoint.X && x < EndPoint.X ? (float)Math.Min(StartPoint.X, EndPoint.X) : x > StartPoint.X && x > EndPoint.X ? (float)Math.Max(StartPoint.X, EndPoint.X) : x;
                    y = y < StartPoint.Y && y < EndPoint.Y ? (float)Math.Min(StartPoint.Y, EndPoint.Y) : y > StartPoint.Y && y > EndPoint.Y ? (float)Math.Max(StartPoint.Y, EndPoint.Y) : y;
                }

                result = new SKPoint(x, y);
            }
        }

        return result;
    }

    // positive if same direction (1 is collinear), negative is opposite direction (-1 is collinear), 0 if orthogonal.
    public double CosineSimilarity(SKSegment seg)
    {
        var e1 = Vector;
        var e2 = seg.Vector;
        var dp = e1.DotProduct(e2);
        var norm = Math.Sqrt((e1.X * e1.X + e1.Y * e1.Y) * (e2.X * e2.X + e2.Y * e2.Y));
        return dp / norm;
    }

    public int DirectionOnLine(SKSegment seg)
    {
        return LengthSquared == 0 || CosineSimilarity(seg) >= 0 ? 1 : -1;
    }
    public (float, SKPoint) TFromPoint(SKPoint point, bool clamp)
    {
        var pp = ProjectPointOnto(point, clamp);
        var segLen = Vector;
        var ptOffset = pp - StartPoint;
        var sign = segLen.X * ptOffset.X >= 0 && segLen.Y * ptOffset.Y >= 0 ? 1f : -1f;
        var totalLen = segLen.LengthSquared;
        var ptLen = ptOffset.LengthSquared;
        var t = ptLen / totalLen;
        t = (float)(Math.Sqrt(t) * sign);
        return (t, pp);
    }

    // Note: Skia segments work in skia space, so they are not complex number segments (thus start is not negated).
    public PRange RatiosWithBasis(SKSegment basis)
    {
        var sp = basis.TFromPoint(StartPoint, false).Item1;
        var ep = basis.TFromPoint(EndPoint, false).Item1;
        return new PRange(sp, ep);
    }
    public PRange RatiosAsBasis(SKSegment nonBasis) => RatiosAsBasis(nonBasis.StartPoint, nonBasis.EndPoint);
    public PRange RatiosAsBasis(SKPoint startPoint, SKPoint endPoint)
    {
        var sp = TFromPoint(startPoint, false).Item1;
        var ep = TFromPoint(endPoint, false).Item1;
        return new PRange(sp, ep);
    }

    public SKPoint[] EndArrow(float dist = 8f)
    {
        var result = new SKPoint[3];
        var p0 = SKPointFromEnd(dist);
        result[0] = OrthogonalPoint(p0, -dist / 2f);
        result[1] = EndPoint;
        result[2] = OrthogonalPoint(p0, dist / 2f);

        return result;
    }
    //public SKPoint[] EndArrow(bool unitPerspective, float dist = 8f)
    //{
    // var result = new SKPoint[4];
    // var p0 = unitPerspective ? SKPointFromEnd(dist) : EndPoint;
    // result[0] = OrthogonalPoint(p0, -dist / 2f);
    // result[1] = unitPerspective ? EndPoint : SKPointFromEnd(dist);
    // result[2] = OrthogonalPoint(p0, dist / 2f);
    // result[3] = result[0];

    // return result;
    //}

    public override string ToString()
    {
        return $"[{StartPoint},{EndPoint}]";
    }
}
