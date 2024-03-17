using Numbers.Drawing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numbers.Utils
{
    public static class SkRectExtension
	{
		public static SKRect SKRect(this RectangleF rectF) =>
			new SKRect(rectF.Left, rectF.Top, rectF.Right, rectF.Bottom);

		public static SKRect SKRect(this Rectangle rect) =>
			new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

		public static SKSegment TopLine(this SKRect self) => new SKSegment(self.Left, self.Top, self.Right, self.Top);
		public static SKSegment LeftLine(this SKRect self) => new SKSegment(self.Left, self.Bottom, self.Left, self.Top);
		public static SKSegment BottomLine(this SKRect self) => new SKSegment(self.Left, self.Bottom, self.Right, self.Bottom);
		public static SKSegment RightLine(this SKRect self) => new SKSegment(self.Right, self.Bottom, self.Right, self.Top);
	}
}
