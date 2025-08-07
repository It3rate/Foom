using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NumbersSkia.Utils;
using SkiaSharp;

namespace NumbersSkia.Drawing;
public class SKRotatedRectGrid
{
    public SKRotatedRect RotatedRect { get; }
    private RectDirections _origin;
    public SKPoint[,] Grid { get; private set; }
    public float[,] HeightMap { get; private set; }
    public bool HasHeightData { get; private set; } = false;

    private float _minStepSize = -1;
    public int ColumnCount { get; set; } // Horizontal points (left to right)
    public int RowCount { get; set; }    // Vertical points (bottom to top)


    private bool _needMinMaxCalc = false;
    private float _zMin = float.MaxValue;
    public float ZMin
    {
        get
        {
            if (_needMinMaxCalc) { CalcMinMax(); }
            return _zMin;
        }
        private set { _zMin = value; }
    }
    private float _zMax = float.MaxValue;
    public float ZMax
    {
        get
        {
            if (_needMinMaxCalc)  {  CalcMinMax();  }
            return _zMax;
        }
        private set { _zMax = value; }
    }
    public SKRotatedRectGrid(SKRotatedRect rect, int rowCount, int columnCount, RectDirections origin)
    {
        RotatedRect = rect;
        ColumnCount = columnCount;
        RowCount = rowCount;
        _origin = origin;
        Grid = GenerateGrid();
        HeightMap = new float[RowCount, ColumnCount];
    }
    public SKRotatedRectGrid(SKRotatedRect rect, float minStepSize, RectDirections origin)
    {
        RotatedRect = rect;
        _minStepSize = minStepSize;
        _origin = origin;
        GenerateCounts();
        Grid = GenerateGrid();
        HeightMap = new float[RowCount, ColumnCount];
    }
    public SKMatrix GetOriginTranslationMatrix(RectDirections jobOrientation)
    {
        return SKMatrix.Identity;
    }
    public IEnumerable<(int, int, SKPoint)> SerpentineIterator()
    {
        int rows = Grid.GetLength(0);
        int cols = Grid.GetLength(1);

        for (int col = 0; col < cols; col++)
        {
            if (col % 2 == 0)
            {
                // Even column: up (from bottom to top)
                for (int row = rows - 1; row >= 0; row--)
                {
                    yield return (row, col, Grid[row, col]);
                }
            }
            else
            {
                // Odd column: down (from top to bottom)
                for (int row = 0; row < rows; row++)
                {
                    yield return (row, col, Grid[row, col]);
                }
            }
        }
        _needMinMaxCalc = true;
    }

    private SKPoint[,] GenerateGrid(int xDir = 1, int yDir = 1)
    {
        var result = new SKPoint[RowCount, ColumnCount];
        var r = RotatedRect;

        var bottomSeg = xDir == 1 ? r.BottomLineRightward : r.BottomLineLeftward;// new SKSegment(r.BottomLeft, r.BottomRight);
        var topSeg = xDir == 1 ? r.TopLineRightward : r.TopLineLeftward; // new SKSegment(r.TopLeft, r.TopRight);

        for (int col = 0; col < ColumnCount; col++)
        {
            float u = (float)col / (ColumnCount - 1);
            var b = bottomSeg.PointAlongLine(u);
            var t = topSeg.PointAlongLine(u);
            var seg = yDir == 1 ? new SKSegment(b, t) : new SKSegment(t, b);
            for (int row = 0; row < RowCount; row++)
            {
                float v = (float)row / (RowCount - 1);

                result[row, col] = seg.PointAlongLine(v);
            }
        }
        _needMinMaxCalc = true;
        return result;
    }
    private void GenerateCounts()
    {
        var leftLine = RotatedRect.LeftLineUpward;
        var bottomLine = RotatedRect.BottomLineLeftward;
        if (leftLine.Length < _minStepSize || bottomLine.Length < _minStepSize)
        {
            ColumnCount = 2;
            RowCount = 2;
        }
        else
        {
            ColumnCount = (int)Math.Ceiling(bottomLine.Length / (double)_minStepSize);
            RowCount = (int)Math.Ceiling(leftLine.Length / (double)_minStepSize);
        }
        _needMinMaxCalc = true;
    }

    public void AddValue(int rowIndex, int columnIndex, float value)
    {
        if (columnIndex >= 0 && columnIndex < ColumnCount && rowIndex >= 0 && rowIndex < RowCount)
        {
            HeightMap[rowIndex, columnIndex] = value;
        }
        HasHeightData = true;
        _needMinMaxCalc = true;
    }
    public void AddValues(float[,] heightMap)
    {
        if (heightMap.GetLength(0) == RowCount && heightMap.GetLength(1) == ColumnCount)
        {
            heightMap.CopyTo(HeightMap, 0);
        }
        HasHeightData = true;
        _needMinMaxCalc = true;
    }

    public float GetInterpolatedHeight(float xPosition, float yPosition)
    {
        SKPoint A = RotatedRect.BottomLeft;
        SKPoint B = RotatedRect.BottomRight;
        SKPoint C = RotatedRect.TopRight;
        SKPoint D = RotatedRect.TopLeft;

        float Ax = A.X, Ay = A.Y;
        float Bx = B.X, By = B.Y;
        float Cx = C.X, Cy = C.Y;
        float Dx = D.X, Dy = D.Y;

        float mux = xPosition - Ax;
        float muy = yPosition - Ay;

        float dx1 = Bx - Ax;
        float dy1 = By - Ay;
        float dx2 = Dx - Ax;
        float dy2 = Dy - Ay;
        float dx3 = Cx - Bx - Dx + Ax;
        float dy3 = Cy - By - Dy + Ay;

        float a = dy2 * dx3 - dx2 * dy3;
        float b = mux * dy3 - muy * dx3 - dy1 * dx2 + dx1 * dy2;
        float c = mux * dy1 - muy * dx1;

        float s, t;
        const float epsilon = 1e-5f;

        if (Math.Abs(a) < epsilon)
        {
            if (Math.Abs(b) < epsilon)
            {
                return 0f;
            }
            t = -c / b;

            float denomx = dx1 + t * dx3;
            float denomy = dy1 + t * dy3;

            if (Math.Abs(denomx) > Math.Abs(denomy))
            {
                s = (mux - t * dx2) / denomx;
            }
            else
            {
                s = (muy - t * dy2) / denomy;
            }
        }
        else
        {
            float disc = b * b - 4 * a * c;
            if (disc < 0) return 0f;

            float sqrt_disc = (float)Math.Sqrt(disc);
            float t1 = (-b + sqrt_disc) / (2 * a);
            float t2 = (-b - sqrt_disc) / (2 * a);

            bool found = false;
            float t_use = 0f, s_use = 0f;

            // Check t1
            if (t1 >= 0 && t1 <= 1)
            {
                float denomx = dx1 + t1 * dx3;
                float denomy = dy1 + t1 * dy3;
                if (Math.Abs(denomx) >= epsilon || Math.Abs(denomy) >= epsilon)
                {
                    float s1;
                    if (Math.Abs(denomx) > Math.Abs(denomy))
                    {
                        s1 = (mux - t1 * dx2) / denomx;
                    }
                    else
                    {
                        s1 = (muy - t1 * dy2) / denomy;
                    }
                    if (s1 >= 0 && s1 <= 1)
                    {
                        t_use = t1;
                        s_use = s1;
                        found = true;
                    }
                }
            }

            // Check t2 if not found
            if (!found && t2 >= 0 && t2 <= 1)
            {
                float denomx = dx1 + t2 * dx3;
                float denomy = dy1 + t2 * dy3;
                if (Math.Abs(denomx) >= epsilon || Math.Abs(denomy) >= epsilon)
                {
                    float s2;
                    if (Math.Abs(denomx) > Math.Abs(denomy))
                    {
                        s2 = (mux - t2 * dx2) / denomx;
                    }
                    else
                    {
                        s2 = (muy - t2 * dy2) / denomy;
                    }
                    if (s2 >= 0 && s2 <= 1)
                    {
                        t_use = t2;
                        s_use = s2;
                        found = true;
                    }
                }
            }

            if (!found) return 0f;

            s = s_use;
            t = t_use;
        }

        // Clamp s and t to [0,1] for safety
        s = Math.Max(0f, Math.Min(1f, s));
        t = Math.Max(0f, Math.Min(1f, t));

        float horizontalFrac = s * (ColumnCount - 1);
        float verticalFrac = t * (RowCount - 1);

        int col = (int)Math.Floor(horizontalFrac);
        int row = (int)Math.Floor(verticalFrac);

        col = Math.Max(0, Math.Min(col, ColumnCount - 2));
        row = Math.Max(0, Math.Min(row, RowCount - 2));

        float tx = horizontalFrac - col;
        float ty = verticalFrac - row;

        float z00 = HeightMap[row, col];
        float z10 = HeightMap[row, col + 1];
        float z01 = HeightMap[row + 1, col];
        float z11 = HeightMap[row + 1, col + 1];

        return (1 - tx) * (1 - ty) * z00 + tx * (1 - ty) * z10 + (1 - tx) * ty * z01 + tx * ty * z11;
    }

    private void CalcMinMax()
    {
        _needMinMaxCalc = false;
        ZMin = float.MaxValue;
        ZMax = float.MinValue;
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                var val = Grid[row, col];
                if (HasHeightData)
                {
                    var zVal = HeightMap[row, col];
                    if (zVal < ZMin)
                    {
                        ZMin = zVal;
                    }
                    else if (zVal > ZMax)
                    {
                        ZMax = zVal;
                    }
                }
            }
        }
    }
    public string SaveData(string targetDir, string filePath)
    {
        var result = "";
        var dict = new Dictionary<string, object>
            {
                { "column_count", ColumnCount },
                { "row_count", RowCount },
                { "origin", (int)_origin },
                { "rotated_rect", Serialize.SKPointsToFloat2D(RotatedRect.GetPoints()) },
                { "grid", Serialize.SKPointsToFloat3D(Grid) },
                { "height_map", Serialize.ToJaggedArray(HeightMap) },
            };

        var options = new JsonSerializerOptions { WriteIndented = true };
        result = JsonSerializer.Serialize(dict, options);
        Serialize.WriteToDisk(result, targetDir, filePath);
        return result;
    }
    public static SKRotatedRectGrid? LoadData(string targetDir, string filePath)
    {
        SKRotatedRectGrid? result = null;
        var json = Serialize.ReadFromDisk(targetDir, filePath);
        if (json != "")
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options)
                    ?? throw new JsonException("Failed to deserialize JSON");

                var columnCount = Serialize.GetInt(dict, "row_count");
                var rowCount = Serialize.GetInt(dict, "column_count");
                var origin = (RectDirections)Serialize.GetInt(dict, "origin");

                if (dict.TryGetValue("rotated_rect", out var rrectToken))
                {
                    var rrect = new SKRotatedRect(Serialize.ParseFloat2DToSKPoints(rrectToken.ToString()));
                    if (rrect != null)
                    {
                        result = new SKRotatedRectGrid(rrect, rowCount, columnCount, origin);
                        if (dict.TryGetValue("grid", out var gridToken))
                        {
                            result.Grid = Serialize.ParseFloat3DToSKPoints(gridToken.ToString());
                        }

                        if (dict.TryGetValue("height_map", out var mapToken))
                        {
                            var jagged = JsonSerializer.Deserialize<float[][]>(mapToken.ToString(), options);
                            result.HeightMap = Serialize.JaggedTo2DArray(jagged)
                                ?? throw new JsonException("Failed to deserialize JSON AutolevelMap");
                        }

                        result.HasHeightData = true;
                        result.CalcMinMax();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error loading autolevel data: " + ex.Message);
            }
        }
        return result;
    }
}