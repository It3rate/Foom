
//using SkiaSharp;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace InkPad.Adapters
//{
//    public abstract class SkiaShapeAdapter
//    {
//        protected PolyNumberSet FocalSet { get; }

//        protected bool _pathDirty = true;

//        public SKPoint TransformAnchor { get; set; }

//        private SKMatrix _matrix = SKMatrix.Identity;
//        public SKMatrix Matrix
//        {
//            get => _matrix;
//            set
//            {
//                _matrix = value;
//                _pathDirty = true;
//            }
//        }
//        public bool HasContent => FocalSet.Count > 2 || FocalSet.HasNonZeroContent();
//        protected SkiaShapeAdapter(int width, int height)
//        {
//            FocalSet = CreateFocalSet(width, height);
//        }
//        protected abstract PolyNumberSet CreateFocalSet(int width, int height);

//        public int PointCount => FocalSet.Count;
//        public SKPoint MakePoint(double[] values) => new SKPoint((float)values[0], (float)values[1]);
//        public SKPoint[] ToPoints()
//        {
//            var pts = FocalSet.GetIndexedValues();
//            var result = new SKPoint[pts.Length];

//            for (int i = 0; i < pts.Length; i++)
//            {
//                result[i] = MakePoint(pts[i]);
//            }
//            result = Matrix.MapPoints(result);
//            TransformAnchor = result.Center();
//            return result;
//        }

//        public SKPath ToQuadCurves(bool closePath = true)
//        {
//            var result = new SKPath();
//            var pts = FocalSet.GetIndexedValues();

//            result.MoveTo(MakePoint(pts[0]));
//            for (int i = 1; i < pts.Length; i++)
//            {
//                result.QuadTo(MakePoint(pts[i]), MakePoint(pts[i + 1]));
//            }

//            if (closePath == true)
//            {
//                result.Close();
//            }
//            return result;
//        }

//        public SKPath ToPolyline(bool closePath = true)
//        {
//            var result = new SKPath();
//            result.AddPoly(ToPoints(), closePath);
//            return result;
//        }


//        public SKPath ToOptimizedCurve(bool closePath = true, double curveThreshold = 1e-5)
//        {
//            SKPath path = new SKPath();
//            var orgPoints = ToPoints();
//            if (orgPoints != null && orgPoints.Length > 1)
//            {
//                var points = DouglasPeuckerReduction(orgPoints);
//                //Trace.WriteLine("reduced: " + (orgPoints.Length - points.Length) + " / " + orgPoints.Length);
//                var first = points[0];
//                path.MoveTo(first);

//                for (int i = 1; i < points.Length - 1; i++)
//                {
//                    var cur = points[i];
//                    var next = points[i + 1];
//                    if (i < points.Length - 2)
//                    {
//                        var afterNext = points[i + 2];
//                        var midPoint1 = new SKPoint((cur.X + next.X) / 2, (cur.Y + next.Y) / 2);
//                        var midPoint2 = new SKPoint((next.X + afterNext.X) / 2, (next.Y + afterNext.Y) / 2);

//                        float curvature = (afterNext.X - 2 * next.X + cur.X) * (afterNext.Y - cur.Y) -
//                                          (afterNext.Y - 2 * next.Y + cur.Y) * (afterNext.X - cur.X);

//                        if (Math.Abs(curvature) < curveThreshold)
//                        {
//                            path.LineTo(next);
//                        }
//                        else
//                        {
//                            path.QuadTo(next, midPoint2);
//                            i++;
//                        }
//                    }
//                    else
//                    {
//                        path.LineTo(next);
//                    }

//                    if (closePath && next != first)
//                    {
//                        path.LineTo(first);
//                    }
//                }
//            }
//            return path;
//        }

//        public SKPoint[] DouglasPeuckerReduction(SKPoint[] points, double tolerance = 0.5)
//        {
//            var result = points;
//            if (points.Length > 3)
//            {
//                int firstPoint = 0;
//                int lastPoint = points.Length - 1;
//                var pointIndexsToKeep = new List<int>() { firstPoint, lastPoint };

//                while (points[firstPoint].Equals(points[lastPoint]) && lastPoint > firstPoint)
//                {
//                    lastPoint--;
//                }

//                DPReduction(points, firstPoint, lastPoint, tolerance, ref pointIndexsToKeep);

//                pointIndexsToKeep.Sort();
//                points = pointIndexsToKeep.Select(index => points[index]).ToArray();
//            }
//            return points;
//        }
//        private void DPReduction(SKPoint[] points,
//            Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep)
//        {
//            double maxDistance = 0;
//            int indexFarthest = 0;

//            for (int index = firstPoint; index < lastPoint; index++)
//            {
//                var distance = points[index].DistanceToLine(points[firstPoint], points[lastPoint]);
//                if (distance > maxDistance)
//                {
//                    maxDistance = distance;
//                    indexFarthest = index;
//                }
//            }

//            if (maxDistance > tolerance && indexFarthest != 0)
//            {
//                pointIndexsToKeep.Add(indexFarthest);
//                DPReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep);
//                DPReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep);
//            }
//        }

//        public virtual void Reset()
//        {
//            FocalSet.Reset();
//        }
//    }
//}
