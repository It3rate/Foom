using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SkiaSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NumbersSkia.Utils;
public class Serialize
{
    public static bool WriteToDisk(string data, string targetDir, string filename)
    {
        bool result = false;
        if (Directory.Exists(targetDir))
        {
            var path = Path.Combine(targetDir, filename);
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(path, data);
                result = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error writing to disk: {ex.Message}");
            }
        }
        else
        {
            Trace.WriteLine("Data folder not found for: " + filename);

        }
        return result;
    }
    public static string ReadFromDisk(string targetDir, string filename)
    {
        string result = "";
        var path = Path.Combine(targetDir, filename);
        if (File.Exists(path))
        {
            result = File.ReadAllText(path);
        }
        else
        {
            Trace.WriteLine("Folder not found: " + path);
        }
        return result;
    }
    public static int GetInt(Dictionary<string, object> dict, string name)
    {
        int result = 0;
        if (dict.TryGetValue(name, out var val))
        {
            if (val is JsonElement jsonElement)
            {
                result = jsonElement.GetInt32();
            }
        }
        return result;
    }
    public static float GetFloat(Dictionary<string, object> dict, string name)
    {
        float result = 0;
        if (dict.TryGetValue(name, out var val))
        {
            if (val is JsonElement jsonElement)
            {
                result = jsonElement.GetSingle();
            }
        }
        return result;
    }
    public static float[] GetFloatArray(Dictionary<string, object> dict, string name)
    {
        float[] result = [];
        if (dict.TryGetValue(name, out var val))
        {
            if (val is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                result = jsonElement.EnumerateArray()
                    .Where(element => element.ValueKind == JsonValueKind.Number)
                    .Select(element => element.GetSingle())
                    .ToArray();
            }
        }
        return result;
    }
    public static SKPoint GetSKPoint(Dictionary<string, object> dict, string name)
    {
        SKPoint result = SKPoint.Empty;
        float[] vals = [];
        if (dict.TryGetValue(name, out var val))
        {
            if (val is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                vals = jsonElement.EnumerateArray()
                    .Where(element => element.ValueKind == JsonValueKind.Number)
                    .Select(element => element.GetSingle())
                    .ToArray();
            }
        }
        if (vals.Length >= 2)
        {
            result = new SKPoint(vals[0], vals[1]);
        }
        return result;
    }

    public static SKPoint[] GetSKPoints(Dictionary<string, object> dict, string name)
    {
        var result = new List<SKPoint>();
        if (dict.TryGetValue(name, out var val))
        {
            if (val is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var pointElement in jsonElement.EnumerateArray())
                {
                    if (pointElement.ValueKind == JsonValueKind.Array)
                    {
                        var vals = pointElement.EnumerateArray()
                            .Where(element => element.ValueKind == JsonValueKind.Number)
                            .Select(element => element.GetSingle())
                            .ToArray();
                        if (vals.Length >= 2)
                        {
                            result.Add(new SKPoint(vals[0], vals[1]));
                        }
                    }
                }
            }
        }
        return result.ToArray();
    }

    public static float[] SKPointToFloatArray(SKPoint point)
    {
       return [ point.X, point.Y ];
    }

    public static object[] SKPointsToFloatArray(SKPoint[] points)
    {
        List<float[]> pointArrays = new List<float[]>();
        for (int i = 0; i < points.Length; i++)
        {
            pointArrays.Add(SKPointToFloatArray(points[i]));
        }
        return pointArrays.ToArray();
    }
    public static SKPoint[] ParseFloat2DToSKPoints(string json)
    {
        var points = JsonSerializer.Deserialize<float[][]>(json);
        var result = new SKPoint[points.Length];
        for (var i = 0; i < points.GetLength(0); i++)
        {
            result[i] = new SKPoint(points[i][0], points[i][1]);
        }
        return result;
    }
    public static float[][] SKPointsToFloat2D(SKPoint[] points)
    {
        float[,] result = new float[points.Length, 2];
        for (var i = 0; i < points.GetLength(0); i++)
        {
                result[i, 0] = points[i].X;
                result[i, 1] = points[i].Y;
        }
        return Serialize.ToJaggedArray(result);
    }
    public static SKPoint[,] ParseFloat3DToSKPoints(string json)
    {
        var points = JsonSerializer.Deserialize<float[][][]>(json);
        if (points.Length < 1 && points[0].Length < 1)
        {
            return new SKPoint[0, 0];
        }
        int d0 = points.Length;
        int d1 = points[0].Length;
        var result = new SKPoint[d0, d1];
        for (var row = 0; row < d0; row++)
        {
            for (int col = 0; col < d1; col++)
            {
                result[row, col] = new SKPoint(points[row][col][0], points[row][col][1]);
            }
        }
        return result;
    }
    public static float[][][] SKPointsToFloat3D(SKPoint[,] points)
    {
        float[,,] result = new float[points.GetLength(0), points.GetLength(1), 2];
        for (var row = 0; row < points.GetLength(0); row++)
        {
            for (int col = 0; col < points.GetLength(1); col++)
            {
                var ar = SKPointToFloatArray(points[row, col]);
                result[row, col, 0] = ar[0];
                result[row, col, 1] = ar[1];
            }
        }
        return Serialize.ToJaggedArray(result);
    }
    public static float[][] ToJaggedArray(float[,] array2D)
    {
        int d1 = array2D.GetLength(0);
        int d2 = array2D.GetLength(1);
        var jagged = new float[d1][];
        for (int i = 0; i < d1; i++)
        {
            jagged[i] = new float[d2];
            for (int j = 0; j < d2; j++)
            {
                jagged[i][j] = array2D[i, j];
            }
        }
        return jagged;
    }
    public static float[][][] ToJaggedArray(float[,,] array3D)
    {
        int d1 = array3D.GetLength(0);
        int d2 = array3D.GetLength(1);
        int d3 = array3D.GetLength(2);
        var jagged = new float[d1][][];
        for (int i = 0; i < d1; i++)
        {
            jagged[i] = new float[d2][];
            for (int j = 0; j < d2; j++)
            {
                jagged[i][j] = new float[d3];
                for (int k = 0; k < d3; k++)
                {
                    jagged[i][j][k] = array3D[i, j, k];
                }
            }
        }
        return jagged;
    }
    public static float[,]? JaggedTo2DArray(float[][] jagged)
    {
        float[,]? result = null;
        if (jagged == null || jagged.Length == 0)
            Trace.WriteLine("Jagged array is null or empty");

        int rows = jagged.Length;
        int cols = jagged[0]?.Length ?? 0;
        if (jagged.Any(row => row == null || row.Length != cols))
            Trace.WriteLine("Inconsistent column count in jagged array");

        result = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = jagged[i][j];
            }
        }
        return result;
    }

}
