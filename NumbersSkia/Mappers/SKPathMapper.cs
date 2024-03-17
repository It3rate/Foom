namespace Numbers.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Agent;
    using Numbers.Drawing;
    using Numbers.Renderer;
    using Numbers.Utils;
    using NumbersCore.CoreConcepts.Spatial;
    using NumbersCore.Primitives;
    using NumbersCore.Utils;
    using SkiaSharp;

    public class SKPathMapper : SKMapper
    {
        public ExtentDomain PolylineDomain => (ExtentDomain)MathElement;

        private List<SKPoint> _points = new List<SKPoint>();
        private SKPoint _lastPoint;
        private SKPoint[] _smoothPoints;
        private SKPoint[] _storedPoints;
        private bool _isShape = false;
        private bool _pathDirty = false;
        public int Count => PolylineDomain.Count;
        public bool IsPermanent { get; set; } = false;
        public SKPaint Pen { get; set; }

        public SKPathMapper(MouseAgent agent, SKSegment guideline = null) : base(agent, new ExtentDomain(500, 500), guideline)
        {
            Pen = CorePens.GetPen(SKColor.FromHsl(220, 60, 60), 7);
        }
        //public SKPathMapper(MouseAgent agent, PolyNumberChain numberSet, SKSegment xBasis) : base(agent, numberSet, xBasis)
        //{
        //}

        public void SetStoredPoints(string points)
        {
            _storedPoints = GetValuesFromString(points);
        }
        public override SKPath GetHighlightAt(Highlight highlight)
        {
            throw new NotImplementedException();
        }

        public void BeginRecord()
        {
            Reset();
        }
        public void EndRecord()
        {
            FinalizePath();
        }
        public void AddPosition(float x, float y)
        {
            AddPosition(new SKPoint(x, y));
        }
        public void AddPosition(double x, double y)
        {
            AddPosition(new SKPoint((float)x, (float)y));
        }
        public void AddPosition(SKPoint point)
            {
                if (_points.Count == 0)
            {
                _lastPoint = point;
            }
            else
            {
                PolylineDomain.AddPosition((long)_lastPoint.X, (long)_lastPoint.Y, (long)point.X, (long)point.Y);
                _lastPoint = point;
            }
            _points.Add(point);

            if ((_points.Count > 0) && (_points.Count % 32 == 0))
            {
                //SmoothPositions();
            }
        }

        public void SetRect(SKPoint p0, SKPoint p1)
        {
            Reset();
            _isShape = true;
            AddPosition(p0.X, p0.Y);
            AddPosition(p1.X, p0.Y);
            AddPosition(p1.X, p1.Y);
            AddPosition(p0.X, p1.Y);
            AddPosition(p0.X, p0.Y);
            _pathDirty = true;
        }
        public void SetOval(SKPoint p0, SKPoint p1)
        {
            Reset();
            _isShape = true;
            var center = p0.Midpoint(p1);
            var xr = center.X - p0.X;
            var yr = center.Y - p0.Y;
            var num = (int)(p0.UnsignedDistanceTo(p1) * .8); // steps in polyline
            var step = MathF.PI * 2 / num;
            for (int i = 0; i < num + 1; i++)
            {
                AddPosition(center.X + Math.Sin(i * step) * xr, center.Y + Math.Cos(i * step) * yr);
            }
            _pathDirty = true;
        }
        public void SetStar(SKPoint p0, SKPoint p1, int points = -1)
        {
            Reset();
            _isShape = true;
            var center = p0.Midpoint(p1);
            var starPoints = GenerateStar(center.X, center.Y, center.X - p0.X, center.Y - p0.Y, points, 0.5f);
            foreach (var pt in starPoints)
            {
                AddPosition(pt.X, pt.Y);
            }
            _pathDirty = true;
        }
        public static SKPoint[] GenerateStar(float xCenter, float yCenter, float xRadius, float yRadius, int points, float innerRatio = 0.5f)
        {
            var num = points == -1 ? 5 * 2 : points * 2; // star points
            num = num < 0 ? 0 : num;
            var step = MathF.PI * 2 / num;
            var result = new SKPoint[num + 1]; // + 1 is link back to start point
            for (int i = 0; i < num + 1; i++)
            {
                var curXr = i % 2 == 1 ? xRadius : xRadius * innerRatio;
                var curYr = i % 2 == 1 ? yRadius : yRadius * innerRatio;
                var x = (float)(xCenter + Math.Sin(i * step) * curXr);
                var y = (float)(yCenter + Math.Cos(i * step) * curYr);
                result[i] = new SKPoint(x, y);
            }
            return result;
        }
        public static SKPath GenerateArcOval(SKRect bounds, float convexity)
        {
            SKPath path = new SKPath();
            float width = bounds.Width;

            float startAngle = 90;
            float sweepAngle = 180;
            var outerRect = bounds;
            var skew = Math.Abs(convexity) * (bounds.Width / 2f);
            outerRect.Left += skew < 0 ? (float)Math.Sin(convexity) * 22f : 0;
            //outerRect.Right += skew > 0 ? skew / 2f : 0;
            SKPoint center = new SKPoint(outerRect.MidX, outerRect.MidY);
            path.AddArc(outerRect, startAngle, sweepAngle);

            var innerRect = outerRect;

            innerRect.Left = center.X - skew;
            innerRect.Right = center.X + skew;
            path.AddArc(innerRect, startAngle + sweepAngle, sweepAngle * Math.Sign(convexity));
            return path;
        }

        public void SmoothPositions()
        {
            if (!_isShape)
            {
                var positions = PolylineDomain.GetContiguousPositions();
                var pts = PositionsToPoints(positions);
                _smoothPoints = DouglasPeuckerReduction(pts);
            }
        }
        public void FinalizePath()
        {
            SmoothPositions();
            var positions = PointsToPositions(_smoothPoints);
            PolylineDomain.ResetWithContiguousPositions(positions);
        }
        private SKPoint[] PositionsToPoints(long[] positions)
        {
            var result = new SKPoint[positions.Length / 2];
            var j = 0;
            for (int i = 0; i < positions.Length; i += 2)
            {
                result[j++] = new SKPoint(positions[i], positions[i + 1]);
            }
            return result;
        }
        private long[] PointsToPositions(SKPoint[] points)
        {
            var result = new long[points.Length * 2];
            var j = 0;
            for (int i = 0; i < points.Length; i++)
            {
                result[j++] = (long)points[i].X;
                result[j++] = (long)points[i].Y;
            }
            return result;
        }
        public SKPoint[] DouglasPeuckerReduction(SKPoint[] points, double tolerance = 1.2)
        {
            var result = points;
            if (points.Length > 3)
            {
                int firstPoint = 0;
                int lastPoint = points.Length - 1;
                var pointIndexesToKeep = new List<int>() { firstPoint, lastPoint };

                while (points[firstPoint].Equals(points[lastPoint]) && lastPoint > firstPoint)
                {
                    lastPoint--;
                }

                DPReduction(points, firstPoint, lastPoint, tolerance, ref pointIndexesToKeep);

                pointIndexesToKeep.Sort();
                points = pointIndexesToKeep.Select(index => points[index]).ToArray();
            }
            return points;
        }
        private void DPReduction(SKPoint[] points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexesToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index += 2)
            {
                var distance = points[index].DistanceToLine(points[firstPoint], points[lastPoint]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                pointIndexesToKeep.Add(indexFarthest);
                DPReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexesToKeep);
                DPReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexesToKeep);
            }
        }

        public SKPaint DefaultBrush { get; set; } = null;
        public override void Draw()
        {
            SKPoint[] pts = _smoothPoints != null ? _smoothPoints : _points.ToArray();
            if(DefaultBrush != null)
            {
                Renderer.FillPolyline(DefaultBrush, pts);
            }
            Renderer.DrawPolyline(Pen, pts);
        }

        public void Reset()
        {
            _points.Clear();
            _smoothPoints = null;
            _isShape = false;
        }
        public void SetPartialPath(double startT, double endT, bool wrap = false)
        {
            if(_storedPoints != null)
            {
                Reset();
                bool isWrapped = false;
                if(startT > endT)
                {
                    var temp = endT;
                    endT = startT;
                    startT = temp;
                }

                if (wrap)
                {
                    isWrapped = (startT < 0 && endT >= 0) || (endT > 1 && startT < 1); // overlapping ends
                    startT = startT % 1f;
                    endT = endT != 1 ? endT % 1f : 1;
                    startT = (startT < 0) ? 1.0 + startT : startT;
                    endT = (endT < 0) ? 1.0 + endT : endT;
                }

                var startIndex = (int)(_storedPoints.Length * startT);  
                startIndex = startIndex < 0 ? 0 : startIndex >= _storedPoints.Length ? _storedPoints.Length : startIndex;
                var endIndex = (int)(_storedPoints.Length * endT);
                endIndex = endIndex < 0 ? 0 : endIndex >= _storedPoints.Length ? _storedPoints.Length : endIndex;

                int length = endIndex - startIndex;
                if(isWrapped)
                {
                    var startlen = _storedPoints.Length - startIndex;
                    _smoothPoints = new SKPoint[Math.Abs(startlen) + endIndex];
                    Array.Copy(_storedPoints, startIndex, _smoothPoints, 0, startlen);
                    Array.Copy(_storedPoints, 0, _smoothPoints, startlen, endIndex);
                }
                else // has wrap
                {
                    length = Math.Abs(startIndex + length >= _storedPoints.Length ? (_storedPoints.Length - 1) - startIndex : length);
                    var start = Math.Min(startIndex, endIndex);
                    _smoothPoints = new SKPoint[length];
                    Array.Copy(_storedPoints, start, _smoothPoints, 0, length);
                }
            }
        }
        public SKPoint[] GetValuesFromString(string values)
        {
            var svals = values.Split(',');
            Reset();
            var result = new SKPoint[svals.Length / 2];
            for (int i = 0; i < result.Length; i ++)
            {
                result[i] = new SKPoint(float.Parse(svals[i * 2]), float.Parse(svals[i * 2 + 1]));
            }
            return result;
        }
        public void SetValuesFromString(string values)
        {
            _smoothPoints = GetValuesFromString(values);
        }
        public string GetValuesAsString()
        {
            var sb = new StringBuilder();
            var comma = "";
            for (int i = 0; i < _smoothPoints.Length; i++)
            {
                sb.Append(comma).AppendFormat(" {0:F2},{1:F2}", _smoothPoints[i].X, _smoothPoints[i].Y);
                comma = ",";
            }
            //SetValuesFromString(" 956.00,313.00, 921.00,332.00, 900.00,349.00, 888.00,373.00, 885.00,389.00, 885.00,404.00, 892.00,418.00, 904.00,435.00, 923.00,451.00, 950.00,470.00, 959.00,481.00, 967.00,500.00, 973.00,523.00, 974.00,534.00, 968.00,548.00, 943.00,573.00, 937.00,582.00, 936.00,595.00, 938.00,601.00, 958.00,612.00, 967.00,615.00, 1000.00,620.00, 1008.00,620.00, 1017.00,601.00\r\n");
            return sb.ToString();
        }
    }
}
