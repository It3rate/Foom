using System;
using Numbers.Drawing;
using SkiaSharp;

namespace Numbers.Utils
{
    public static class SkPointExtension
    {
        public static SKPoint MaxPoint = new SKPoint(float.MaxValue, float.MaxValue);
        public static SKPoint MinPoint = new SKPoint(float.MinValue, float.MinValue);

        public static float Angle(this SKPoint a) => (float)Math.Atan2(a.Y, a.X);
        public static float AngleDegrees(this SKPoint a) => a.Angle() * 180f / 2f;
        public static SKPoint Midpoint(this SKPoint a, SKPoint b) => new SKPoint((b.X - a.X) / 2f + a.X, (b.Y - a.Y) / 2f + a.Y);

        public static SKPoint PointAtRadiansAndDistance(this SKPoint a, float angle, float distance) =>
            new SKPoint(a.X + (float)Math.Cos(angle) * distance, a.Y + (float)Math.Sin(angle) * distance);
        public static SKPoint PointAtDegreesAndDistance(this SKPoint a, float angle, float distance) =>
            PointAtRadiansAndDistance(a, angle / 180f * 2f, distance);

        public static SKPoint Add(this SKPoint a, float xValue, float yValue) => new SKPoint(a.X + xValue, a.Y + yValue);
        public static SKPoint Add(this SKPoint a, float value) => new SKPoint(a.X + value, a.Y + value);
        public static SKPoint Subtract(this SKPoint a, float xValue, float yValue) => new SKPoint(a.X - xValue, a.Y - yValue);
        public static SKPoint Subtract(this SKPoint a, float value) => new SKPoint(a.X - value, a.Y - value);
        public static SKPoint Multiply(this SKPoint a, float value) => new SKPoint(a.X * value, a.Y * value);
        public static SKPoint Divide(this SKPoint a, float value) => new SKPoint(value == 0 ? float.MaxValue : a.X / value, value == 0 ? float.MaxValue : a.Y / value);
        public static SKPoint TurnCCW(this SKPoint a) => new SKPoint(-a.Y, a.X);
        public static SKPoint TurnCW(this SKPoint a) => new SKPoint(a.Y, -a.X);
        public static SKPoint RotateOnOrigin(this SKPoint a, float degrees)
        {
            var angle = (float)(degrees / 180.0 * Math.PI);
            var x = (float)(Math.Cos(angle) * a.X - Math.Sin(angle) * a.Y);
            var y = (float)(Math.Sin(angle) * a.X + Math.Cos(angle) * a.Y);
            return new SKPoint(x, y);
        }
        public static SKPoint Normalize(this SKPoint a)
        {
            var len = a.Length;
            return new SKPoint(a.X / len, a.Y / len);
        }
        public static SKPoint Round(this SKPoint a) => new SKPoint((float)Math.Round(a.X), (float)Math.Round(a.Y));


        public static float Length(this SKPoint self) => (float)Math.Sqrt(self.X * self.X + self.Y * self.Y);
        public static float SquaredLength(this SKPoint self) => self.X * self.X + self.Y * self.Y;
        public static float DistanceTo(this SKPoint self, SKPoint b) => (self - b).Length;
        public static float SquaredDistanceTo(this SKPoint self, SKPoint b) => (self - b).LengthSquared;
        public static float DotProduct(this SKPoint self, SKPoint pt) => self.X * pt.X + self.Y * pt.Y;
        public static float Atan2(this SKPoint self, SKPoint pt) => (float)Math.Atan2(pt.Y - self.Y, pt.X - self.X);
        public static float DistanceToLine(this SKPoint self, SKSegment line, bool clamp = true) => line.DistanceTo(self, clamp);
        public static float DistanceToLine(this SKPoint self, SKPoint p0, SKPoint p1)
        {
            var seg = new SKSegment(p0, p1);
            return seg.DistanceTo(self);
        }
        public static float SignedDistanceTo(this SKPoint self, SKPoint pt)
        {
            var sDist = (pt.X - self.X) * (pt.X - self.X) + (pt.Y - self.Y) * (pt.Y - self.Y);
            return (float)Math.Sqrt(sDist) * (sDist >= 0 ? 1f : -1f);
        }
        public static float UnsignedDistanceTo(this SKPoint self, SKPoint pt)
        {
            return Math.Abs(self.SignedDistanceTo(pt));
        }
        public static float AreaOf(this SKPoint p0, SKPoint p1, SKPoint p2)
        {
            return Math.Abs(.5f * (
                p0.X * p1.Y + p1.X * p2.Y +
                p2.X * p0.Y - p1.X * p0.Y -
                p2.X * p1.Y - p0.X * p2.Y));
        }
        public static (float, float, float) ABCLine(this SKPoint self, SKPoint pt)
        {
            var a = pt.Y - self.Y;
            var b = self.X - pt.X;
            var c = a * self.X + b * self.Y;
            return (a, b, c);
        }
        public static SKPoint Center(this SKPoint[] self)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            foreach ( SKPoint p in self)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            var xDif = maxX - minX;
            var yDif = maxY - minY;
            return new SKPoint(minX + xDif / 2f, minY + yDif / 2f);
        }
    }

}
