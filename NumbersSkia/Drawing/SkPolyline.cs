using Numbers.Utils;
using SkiaSharp;

namespace Numbers.Drawing
{
    public class SkPolyline// : SkiaShapeAdapter, ISelectableElement
    {
        #region private fields
        public int Id { get; }
        private SKPath _path = new SKPath();
        private SKPaint _paint = new SKPaint();

        private float _brushSize = 20f;

        private bool _brushDirty = true;
        private bool _pathDirty = true;
        #endregion

        public float BrushSize
        {
            get => _brushSize;
            set
            {
                if (_brushSize != value)
                {
                    _brushSize = value;
                    _brushDirty = true;
                    _pathDirty = true;
                }
            }
        }
        public byte Transparency
        {
            get => (byte)((_colorValue & 0xFF000000) >> 24);
            set
            {
                _colorValue = (uint)((_colorValue & 0xFFFFFF) + (value << 24));
                _brushDirty = true;
            }
        }
        public float Feather { get; set; } = 0f;

        private uint _colorValue;
        public uint ColorValue
        {
            get => _colorValue;
            set
            {
                if (_colorValue != value)
                {
                    _colorValue = value;
                    _brushDirty = true;
                }
            }
        }


        public SkPolyline(int width, int height, float? brushSize = null, uint? currentColor = null, float? feather = null)// : base(width, height)
        {
            Id = 0;// ISelectableElement.EntityCounter++;

            if (brushSize != null)
            {
                BrushSize = brushSize.Value;
            }
            if (currentColor != null)
            {
                ColorValue = currentColor.Value;
            }
            if (feather != null)
            {
                Feather = feather.Value;
            }
        }
        public void SetWithBrushData(BrushData brushData)
        {
            BrushSize = brushData.BrushSize;
            Feather = brushData.Feather;
            Transparency = brushData.Transparency;
            ColorValue = brushData.ColorValue;
        }
        //protected override PolyNumberSet CreatePolyset(int width, int height)
        //{
        //    var trait = PositionTrait.Instance;
        //    var xDomain = new Domain(trait, "xAxis", Focal.UnitFocal, new Focal(0, width));
        //    var yDomain = new Domain(trait, "yAxis", Focal.UnitFocal, new Focal(0, height));
        //    var polyNumberX = new PolyNumber(xDomain, new PolyFocal());
        //    var polyNumberY = new PolyNumber(yDomain, new PolyFocal());
        //    return new PolyNumberSet(polyNumberX, polyNumberY);
        //}
        //public SKRect Bounds
        //{
        //    get
        //    {
        //        var result = Path.Bounds;
        //        result.Inflate(BrushSize / 2, BrushSize / 2);
        //        return result;
        //    }
        //}

        //public bool IsPointInside(SKPoint point)
        //{
        //    return PathOutline.Contains(point.X, point.Y);
        //}
        //public void AddPoint(double x, double y)
        //{
        //    Polyset.AddValue(x, y);
        //    _isShape = false;
        //    _pathDirty = true;
        //}
        //public void RemovePoint()
        //{
        //    Polyset.RemoveLastValue();
        //}

        //private bool _isShape = false;
        //public void SetRect(SKPoint p0, SKPoint p1)
        //{
        //    Reset();
        //    Polyset.AddValue(p0.X, p0.Y);
        //    Polyset.AddValue(p1.X, p0.Y);
        //    Polyset.AddValue(p1.X, p1.Y);
        //    Polyset.AddValue(p0.X, p1.Y);
        //    Polyset.AddValue(p0.X, p0.Y);
        //    _isShape = true;
        //    _pathDirty = true;
        //}

        //public void SetOval(SKPoint p0, SKPoint p1)
        //{
        //    Reset();
        //    var center = p0.MidPoint(p1);
        //    var xr = center.X - p0.X;
        //    var yr = center.Y - p0.Y;
        //    var num = (int)(p0.UnsignedDistanceTo(p1) * .3); // steps in polyline
        //    var step = MathF.PI * 2 / num;
        //    for (int i = 0; i < num + 1; i++)
        //    {
        //        Polyset.AddValue(center.X + MathF.Sin(i * step) * xr, center.Y + MathF.Cos(i * step) * yr);
        //    }
        //    _isShape = true;
        //    _pathDirty = true;
        //}
        //public void SetStar(SKPoint p0, SKPoint p1)
        //{
        //    Reset();
        //    var center = p0.MidPoint(p1);
        //    var xr = center.X - p0.X;
        //    var yr = center.Y - p0.Y;
        //    var innerRatio = 0.5f;
        //    var num = 5 * 2; // star points
        //    var step = MathF.PI * 2 / num;
        //    for (int i = 0; i < num + 1; i++)
        //    {
        //        var curXr = i % 2 == 1 ? xr : xr * innerRatio;
        //        var curYr = i % 2 == 1 ? yr : yr * innerRatio;
        //        Polyset.AddValue(center.X + MathF.Sin(i * step) * curXr, center.Y + MathF.Cos(i * step) * curYr);
        //    }
        //    _isShape = true;
        //    _pathDirty = true;
        //}

        //public SKPaint Paint
        //{
        //    get
        //    {
        //        if (_brushDirty)
        //        {
        //            _paint.Dispose();
        //            _paint = SKRenderUtils.CreatePen(ColorValue, BrushSize, Feather);
        //            _brushDirty = false;
        //        }
        //        return _paint;
        //    }
        //}
        //private SKPath _optimizedPath;
        //public SKPath Path
        //{
        //    get
        //    {
        //        _ = PathOutline;
        //        return _optimizedPath;
        //    }
        //}
        //public SKPath PathOutline
        //{
        //    get
        //    {
        //        if (_pathDirty || _path.PointCount == 0)
        //        {
        //            if (_isShape)
        //            {
        //                _optimizedPath = ToPolyline(false);
        //            }
        //            else
        //            {
        //                _optimizedPath = ToOptimizedCurve(false);
        //            }
        //            _path = Paint.GetFillPath(_optimizedPath);
        //            _pathDirty = false;
        //        }
        //        return _path;
        //    }
        //}
        //public override void Reset()
        //{
        //    base.Reset();
        //    _brushDirty = true;
        //    _pathDirty = true;
        //}
    }
}