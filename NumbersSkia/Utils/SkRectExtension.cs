using NumbersSkia.Drawing;
using SkiaSharp;
using System.Drawing;

namespace NumbersSkia.Utils;

public static class SkRectExtension
{
    public static SKRect SKRect(this RectangleF rectF) =>
        new SKRect(rectF.Left, rectF.Top, rectF.Right, rectF.Bottom);

    public static SKRect SKRect(this Rectangle rect) =>
        new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

	public static SKPoint BottomLeft(this SKRect self) => new(self.Left, self.Bottom);
	public static SKPoint TopLeft(this SKRect self) => new(self.Left, self.Top);
	public static SKPoint TopRight(this SKRect self) => new(self.Right, self.Top);
	public static SKPoint BottomRight(this SKRect self) => new(self.Right, self.Bottom);
	public static SKSegment TopLine(this SKRect self) => new SKSegment(self.Left, self.Top, self.Right, self.Top);
    public static SKSegment LeftLine(this SKRect self) => new SKSegment(self.Left, self.Bottom, self.Left, self.Top);
    public static SKSegment BottomLine(this SKRect self) => new SKSegment(self.Left, self.Bottom, self.Right, self.Bottom);
    public static SKSegment RightLine(this SKRect self) => new SKSegment(self.Right, self.Bottom, self.Right, self.Top);

	public static SKPoint[] ClockWisePointsFromBL(this SKRect self) =>
        [self.BottomLeft(), self.TopLeft(), self.TopRight(), self.BottomRight()];
}
