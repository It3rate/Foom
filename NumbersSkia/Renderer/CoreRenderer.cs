using Numbers.Agent;
using Numbers.Drawing;
using Numbers.Mappers;
using NumbersCore.Primitives;
using SkiaSharp;

namespace Numbers.Renderer;

public class CoreRenderer
{
    private static CoreRenderer _instance;
    public static CoreRenderer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CoreRenderer();
            }
            return _instance;
        }
    }
    public MouseAgent Agent { get; set; }
    public Brain Brain => Agent.Brain;
    public Workspace Workspace => Agent.Workspace;
    public SKWorkspaceMapper CurrentWorkspaceMapper => Agent.WorkspaceMapper;

    public int Width { get; set; }
    public int Height { get; set; }
    public event EventHandler DrawingComplete;

    public SKCanvas Canvas;
    public CorePens Pens { get; set; }
    public SKBitmap Bitmap { get; set; }
    public bool ShowBitmap { get; set; }

    private CoreRenderer()
    {
        GeneratePens();
    }

    public virtual void BeginDraw()
    {
        Canvas.Save();
        Canvas.SetMatrix(Matrix);
        Canvas.Clear(Pens.BkgColor);
    }
    public virtual void Draw()
    {
        CurrentWorkspaceMapper?.Draw();
        Agent?.Draw();
    }
    public virtual void EndDraw()
    {
        Canvas.Restore();
        if (ShowBitmap && Bitmap != null)
        {
            DrawBitmap(Bitmap);
        }

        Canvas = null;
        OnDrawingComplete();
    }
    protected void OnDrawingComplete()
    {
        DrawingComplete?.Invoke(this, EventArgs.Empty);
    }

    public void DrawSegment(SKSegment seg, SKPaint paint)
    {
        Canvas.DrawLine(seg.StartPoint, seg.EndPoint, paint);
    }
    public void DrawLine(SKPoint p0, SKPoint p1, SKPaint paint)
    {
        Canvas.DrawLine(p0, p1, paint);
    }
    public void DrawGradientNumberLine(SKSegment segment, bool startToEnd, float width, bool isSelected = false)
    {
        if (isSelected)
        {
            Pens.DomainPenHighlight.StrokeWidth = width + 3;
            DrawSegment(segment, Pens.DomainPenHighlight);
        }
        var gsp = startToEnd ? segment.StartPoint : segment.EndPoint;
        var gep = startToEnd ? segment.EndPoint : segment.StartPoint;
        var pnt = CorePens.GetGradientPen(gsp, gep, Pens.UnotLineColor, Pens.UnitLineColor, width);
        DrawSegment(segment, pnt);
    }
    public void DrawRoundBox(SKPoint point, SKPaint paint, float radius = 8f)
    {
        float round = radius / 3f;
        var box = new SKRect(point.X - radius, point.Y - radius, point.X + radius, point.Y + radius);
        Canvas.DrawRoundRect(box, round, round, paint);
    }
    public void DrawPolyline(SKPaint paint, params SKPoint[] polyline)
    {
        Canvas.DrawPoints(SKPointMode.Polygon, polyline, paint);
    }
    public void FillPolyline(SKPaint paint, params SKPoint[] polyline)
    {
        var path = new SKPath
        {
            FillType = SKPathFillType.EvenOdd
        };
        path.MoveTo(polyline[0]);
        path.AddPoly(polyline, true);
        Canvas.DrawPath(path, paint);
    }
    public void DrawLine(SKSegment segIn, SKPaint paint)
    {
        DrawPolyline(paint, segIn.Points);
    }
    public void DrawDirectedLine(SKSegment segIn, SKPaint paint, SKPaint invertPaint = null)
    {
        invertPaint = invertPaint ?? paint;
        var seg = segIn.Clone();
        DrawPolyline(paint, seg.Points);
        Canvas.DrawCircle(seg.StartPoint, 3, paint);
        Canvas.DrawCircle(seg.StartPoint, 1.8f, invertPaint);
        var triPts = seg.EndArrow(8);
        Canvas.DrawPoints(SKPointMode.Polygon, triPts, paint);
    }
    public void DrawHalfLine(SKSegment segIn, SKPaint paint)
    {
        var seg = segIn.Clone();
        DrawPolyline(paint, seg.Points);
        Canvas.DrawCircle(segIn.StartPoint, 1.5f, paint);
    }
    public void DrawStartCap(SKSegment segIn, SKPaint paint)
    {
        Canvas.DrawCircle(segIn.EndPoint, 2f, paint);
        Canvas.DrawCircle(segIn.EndPoint, 4, paint);
    }
    public void DrawEndCap(SKSegment segIn, SKPaint paint)
    {
        var triPts = segIn.EndArrow(8);
        Canvas.DrawPoints(SKPointMode.Polygon, triPts, paint);
    }
    public void DrawTextAt(SKPoint point, string text, SKPaint paint)
    {
        Canvas.DrawText(text, point.X, point.Y, paint);
    }
    public void DrawText(SKPoint center, string text, SKPaint paint, SKPaint background = null)
    {
        if (background != null)
        {
            var rect = GetTextBackgroundSize(center.X, center.Y, text, paint);
            DrawTextBackground(rect, background);
            center = new SKPoint(rect.Left + 4, center.Y);
        }
        Canvas.DrawText(text, center.X, center.Y, paint);
    }
    public void DrawTextOnPath(SKSegment baseline, string text, SKPaint paint, SKPaint background = null)
    {
        var path = new SKPath();
        path.MoveTo(baseline.StartPoint);
        path.LineTo(baseline.EndPoint);
        if (background != null)
        {
            //var rect = GetTextBackgroundSize(center.X, center.Y, text, paint);
            //DrawTextBackground(rect, background);
        }
        Canvas.DrawTextOnPath(text, path, SKPoint.Empty, paint);
    }

    public void DrawTextBackground(SKRect rect, SKPaint background)
    {
        Canvas.DrawRoundRect(rect, 5, 5, background);
    }
    public void DrawBitmap(SKBitmap bitmap)
    {
        Canvas.DrawBitmap(bitmap, new SKRect(0, 0, Width, Height));
    }
    public void DrawBitmap(SKBitmap bitmap, SKRect bounds)
    {
        using (SKPaint paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High })
        {
            Canvas.DrawBitmap(bitmap, bounds, paint);
        }
    }

    public SKPath GetCirclePath(SKPoint center, float radius = 10)
    {
        var path = new SKPath();
        path.AddCircle(center.X, center.Y, radius);
        return path;
    }
    public SKPath GetRectPath(SKPoint topLeft, SKPoint bottomRight)
    {
        var path = new SKPath();
        path.AddRect(new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y));
        return path;
    }
    public SKPath GetSegmentPath(SKSegment segment, float radius = 10)
    {
        var path = new SKPath();
        var (pt0, pt1) = segment.PerpendicularLine(0, radius);
        var ptDiff = pt1 - pt0;
        path.AddPoly(new SKPoint[]
                {segment.StartPoint + ptDiff, segment.EndPoint + ptDiff, segment.EndPoint - ptDiff, segment.StartPoint - ptDiff}, true);
        return path;
    }
    public void GeneratePens(ColorTheme colorTheme = ColorTheme.Normal)
    {
        Pens = new CorePens(1, colorTheme);
    }
    public SKRect GetTextBackgroundSize(float x, float y, String text, SKPaint paint)
    {
        var fm = paint.FontMetrics;
        float halfTextLength = paint.MeasureText(text) / 2 + 4;
        return new SKRect((int)(x - halfTextLength), (int)(y + fm.Top + 3), (int)(x + halfTextLength), (int)(y + fm.Bottom - 1));
    }
    public SKRect GetTextBackgroundSizeExact(float x, float y, String text, SKPaint paint)
    {
        var fm = paint.FontMetrics;
        float halfTextLength = paint.MeasureText(text) / 2f + 0.1f;
        return new SKRect((int)(x - halfTextLength), (int)(y + fm.Top), (int)(x + halfTextLength), (int)(y + fm.Bottom));
    }

    #region RenderSurface

    public void DrawOnBitmapSurface()
    {
        if (Bitmap != null)
        {
            using (SKCanvas canvas = new SKCanvas(Bitmap))
            {
                DrawOnCanvas(canvas);
            }
        }
    }
    public void DrawOnCanvas(SKCanvas canvas)
    {
        Canvas = canvas;
        if (Workspace != null && Workspace.IsActive)
        {
            BeginDraw();
            Draw();
            EndDraw();
        }
    }
    //public void DrawFraction((string, string) parts, SKPoint txtPoint, SKPaint txtPaint, SKPaint txtBkgPen)
    //{
    //}
    public void DrawFraction((string, string) parts, SKSegment txtSeg, SKPaint txtPaint, SKPaint txtBkgPen)
    {
        var whole = parts.Item1;
        var fraction = parts.Item2;
        var fractionPen = Pens.TextFractionPen;
        var wRect = GetTextBackgroundSizeExact(0, 0, whole, txtPaint);
        var fRect = GetTextBackgroundSizeExact(0, 0, fraction, Pens.TextFractionPen);
        var segLen = (float)txtSeg.Length;
        var wWidth = wRect.Width;
        var fWidth = fRect.Width;
        var tWidth = wWidth + fWidth + 2f; //padding to allow space in fraction
        var ratio = (tWidth / segLen) * 0.5f;
        var bothSeg = txtSeg.SegmentAlongLine(0.5f - ratio, 0.5f + ratio);
        var txtAlign = txtPaint.TextAlign;
        var fracAlign = fractionPen.TextAlign;

        if (fraction != "")
        {
            fractionPen.Color = txtPaint.Color;
            if (whole == "")
            {
                DrawTextOnPath(bothSeg, fraction, fractionPen, txtBkgPen);
            }
            else
            {
                txtPaint.TextAlign = SKTextAlign.Left;
                fractionPen.TextAlign = SKTextAlign.Right;
                DrawTextOnPath(bothSeg, whole, txtPaint, null);
                DrawTextOnPath(bothSeg, fraction, fractionPen, null);

                // todo: proper polygon bkg
                var bothRect = SKRect.Create(wRect.Location, wRect.Size);
                bothRect.Union(fRect);
                DrawTextBackground(bothRect, txtBkgPen);
                txtPaint.TextAlign = txtAlign;
            }
            fractionPen.TextAlign = fracAlign;
            txtPaint.TextAlign = txtAlign;
        }
        else
        {
            DrawTextOnPath(bothSeg, whole, txtPaint, txtBkgPen);
        }
    }
    public SKBitmap GenerateBitmap(int width, int height)
    {
        Bitmap = new SKBitmap(width, height);
        return Bitmap;
    }
    #endregion

    #region View Matrix

    private SKMatrix _matrix = SKMatrix.CreateIdentity();
    public SKMatrix Matrix
    {
        get => _matrix;
        set => _matrix = value;
    }
    public float ScreenScale { get; set; } = 1f;

    public void SetPanAndZoom(SKMatrix initalMatrix, SKPoint anchorPt, SKPoint translation, float scale)
    {
        var scaledAnchor = new SKPoint(anchorPt.X * ScreenScale, anchorPt.Y * ScreenScale);
        var scaledTranslation = new SKPoint(translation.X * ScreenScale, translation.Y * ScreenScale);

        var mTranslation = SKMatrix.CreateTranslation(scaledTranslation.X, scaledTranslation.Y);
        var mScale = SKMatrix.CreateScale(scale, scale, scaledAnchor.X, scaledAnchor.Y);
        var mIdent = SKMatrix.CreateIdentity();
        SKMatrix.Concat(ref mIdent, mTranslation, mScale);
        SKMatrix.Concat(ref _matrix, mIdent, initalMatrix);
    }

    public void ResetZoom()
    {
        Matrix = SKMatrix.CreateIdentity();
    }

    #endregion

}
