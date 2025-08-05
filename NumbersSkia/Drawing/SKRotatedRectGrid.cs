using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace NumbersSkia.Drawing;
public class SKRotatedRectGrid
{
    public SKRotatedRect RotatedRect { get; }
    public SKPoint[,] Grid { get; private set; }
    public int ColumnCount { get; set; } // Horizontal points (left to right)
    public int RowCount { get; set; }    // Vertical points (bottom to top)
    public float XMin { get; set; }
    public float XMax { get; set; }
    public float YMin { get; set; }
    public float YMax { get; set; }
    public float ZMin { get; set; }
    public float ZMax { get; set; }
    private float[,] _heightMap { get; set; }

    private float _minStepSize = -1;
    public SKRotatedRectGrid(SKRotatedRect rect, int columnCount, int rowCount)
    {
        RotatedRect = rect;
        ColumnCount = columnCount;
        RowCount = rowCount;
        GenerateGrid();
    }
    public SKRotatedRectGrid(SKRotatedRect rect, float minStepSize)
    {
        RotatedRect = rect;
        GenerateCounts(minStepSize);
        GenerateGrid();
    }

    public void AddValue(int columnIndex, int rowIndex, float value)
    {
        if (columnIndex >= 0 && columnIndex < ColumnCount && rowIndex >= 0 && rowIndex < RowCount)
        {
            _heightMap[rowIndex, columnIndex] = value;
        }
    }
    public void AddValues(float[,] heightMap)
    {
        if (heightMap.GetLength(0) == RowCount && heightMap.GetLength(1) == ColumnCount)
        {
            heightMap.CopyTo(_heightMap, 0);
        }
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

        float z00 = _heightMap[row, col];
        float z10 = _heightMap[row, col + 1];
        float z01 = _heightMap[row + 1, col];
        float z11 = _heightMap[row + 1, col + 1];

        return (1 - tx) * (1 - ty) * z00 + tx * (1 - ty) * z10 + (1 - tx) * ty * z01 + tx * ty * z11;
    }
    private void GenerateCounts(float minStepSize)
    {
        _minStepSize = minStepSize;

        var leftLine = RotatedRect.LeftLineUpward;
        var bottomLine = RotatedRect.BottomLineLeftward;
        if (leftLine.Length < minStepSize || bottomLine.Length < minStepSize)
        {
            ColumnCount = 2;
            RowCount = 2;
        }
        else
        {
            ColumnCount = (int)Math.Ceiling(bottomLine.Length / (double)_minStepSize);
            RowCount = (int)Math.Ceiling(leftLine.Length / (double)_minStepSize);
        }
    }
    private void GenerateGrid()
    {
        Grid = new SKPoint[RowCount, ColumnCount];
        var r = RotatedRect;

        for (int row = 0; row < RowCount; row++)
        {
            float v = (float)row / (RowCount - 1);

            for (int col = 0; col < ColumnCount; col++)
            {
                float u = (float)col / (ColumnCount - 1);

                // Lerp bottom: BottomLeft to BottomRight
                float bottomX = r.BottomLeft.X + u * (r.BottomRight.X - r.BottomLeft.X);
                float bottomY = r.BottomLeft.Y + u * (r.BottomRight.Y - r.BottomLeft.Y);

                // Lerp top: TopLeft to TopRight
                float topX = r.TopLeft.X + u * (r.TopRight.X - r.TopLeft.X);
                float topY = r.TopLeft.Y + u * (r.TopRight.Y - r.TopLeft.Y);

                // Lerp between bottom and top
                float pointX = bottomX + v * (topX - bottomX);
                float pointY = bottomY + v * (topY - bottomY);

                Grid[row, col] = new SKPoint(pointX, pointY);
            }
        }
        _heightMap = new float[RowCount, ColumnCount];
    }
}