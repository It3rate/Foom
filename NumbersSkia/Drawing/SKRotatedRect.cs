using NumbersSkia.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersSkia.Drawing;
public class SKRotatedRect
{
	public SKPoint BottomLeft { get; set; }
	public SKPoint TopLeft { get; set; }
	public SKPoint TopRight { get; set; }
	public SKPoint BottomRight { get; set; }

	public static SKRotatedRect Empty => new SKRotatedRect(SKPoint.Empty, SKPoint.Empty, SKPoint.Empty, SKPoint.Empty);

	public SKRotatedRect(SKPoint bl, SKPoint tl, SKPoint tr, SKPoint br)
	{
		BottomLeft = bl;
		TopLeft = tl;
		TopRight = tr;
		BottomRight = br;
	}
	public SKRotatedRect(SKRect r) : this(r.ClockWisePointsFromBL()){}
	public SKRotatedRect(params SKPoint[] pts)
	{
		BottomLeft = pts.Length > 0 ? pts[0] : SKPoint.Empty;
		TopLeft = pts.Length > 1 ? pts[1] : SKPoint.Empty;
		TopRight = pts.Length > 2 ? pts[2] : SKPoint.Empty;
		BottomRight = pts.Length > 3 ? pts[3] : SKPoint.Empty;
    }
    public SKSegment LeftLineUpward => new SKSegment(BottomLeft, TopLeft);
    public SKSegment TopLineRightward => new SKSegment(TopLeft, TopRight);
    public SKSegment RightLineDownward => new SKSegment(TopRight, BottomRight);
    public SKSegment BottomLineLeftward => new SKSegment(BottomRight, BottomLeft);
    public SKSegment LeftLineDownWard => new SKSegment(TopLeft, BottomLeft);
    public SKSegment TopLineLeftward => new SKSegment(TopRight, TopLeft);
    public SKSegment RightLineUpward => new SKSegment(BottomRight, TopRight);
    public SKSegment BottomLineRightward => new SKSegment(BottomLeft, BottomRight);

    public float XMin => Math.Min(BottomLeft.X, Math.Min(TopLeft.X, Math.Min(TopRight.X, BottomRight.X)));
	public float XMax => Math.Max(BottomLeft.X, Math.Max(TopLeft.X, Math.Max(TopRight.X, BottomRight.X)));
	public float YMin => Math.Min(BottomLeft.Y, Math.Min(TopLeft.Y, Math.Min(TopRight.Y, BottomRight.Y)));
	public float YMax => Math.Max(BottomLeft.Y, Math.Max(TopLeft.Y, Math.Max(TopRight.Y, BottomRight.Y)));
	public SKRect Bounds()
	{
		return new SKRect(XMin, YMin, XMax, YMax);
	}
	public SKPoint[] GetPoints() =>  [BottomLeft, TopLeft, TopRight, BottomRight];

    public bool IsEmpty =>  BottomLeft == SKPoint.Empty && TopLeft == SKPoint.Empty &&
			   TopRight == SKPoint.Empty && BottomRight == SKPoint.Empty;
}
