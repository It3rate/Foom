namespace MathDemo.Controls;

using System;
using System.Collections.Generic;
using Numbers.Agent;
using Numbers.Controls;
using Numbers.Mappers;
using Numbers.Renderer;
using NumbersCore.CoreConcepts.Optical;
using NumbersCore.CoreConcepts.Spatial;
using NumbersCore.Primitives;
using NumbersCore.Utils;
using SkiaSharp;

public class FruitControl : UIControlBase
{
    public ExtentDomain Position { get; private set; } = new ExtentDomain(500, 500);
    public Number Size { get; private set; }
    public Number AspectRatio { get; private set; }
    public HSLDomain Fill { get; private set; } = new HSLDomain();
    public NumberGroup Convexity { get;private set; }
    private SpatialDomain _convexityDomain = SpatialDomain.Get2DCenteredDomain(32, 1, "Convexity");

    private float[] _samplesX => _samples[0];
    private float[] _samplesY => _samples[1];
    private float[] _samplesSize => _samples[2];
    private float[] _samplesAspectRatio => _samples[3];
    private float[] _samplesHue => _samples[4];
    private float[] _samplesSaturation => _samples[5];
    private float[] _samplesLightness => _samples[6];
    private float[] _samplesConvexity => _samples[7];
    public FruitControl(MouseAgent agent, int top, int left, int width, int height, long count = 20) : base(agent, count)
    {
        _sampleCounter.SetValue(20);

        AddPolyProperty(Position, left, left + width, top, top + height);

        Size = CreateProperty("Size", 40, 50, 0, 100);

        var aspectDomain = Domain.CreateDomain("Aspect", 50, -1, 1, 0, "Aspect", false);
        AspectRatio = aspectDomain.CreateNumber(new PRange(0.2f, 0.2f));

        Convexity = new NumberGroup(_convexityDomain);
        Convexity.AddPosition(32, 32);
        AddPolyProperty(Fill, 0, 360, 70, 100, 40, 60);

        CreateDomainMaps();
        Update();
    }
    public string UpdateToIndex(int index)
    {
        index = (index < 0 || index >= FruitData.Count) ? 0 : index;
        var (name, data) = FruitData[index];
        SetValuesFromString(data);
        IsDirty = true;
        Update();
        return name;
    }

    protected override void CreateDomainMaps()
    {
        _numbers.Clear();

        _numbers.Add(Position.Horizontal);
        _numbers.Add(Position.Vertical);
        _numbers.Add(Size);
        _numbers.Add(AspectRatio);
        _numbers.Add(Fill.Hue);
        _numbers.Add(Fill.Saturation);
        _numbers.Add(Fill.Lightness);
        _numbers.Add(Convexity);

        _samples = new float[_numbers.Count][];
    }

    private SKPaint _defaultStroke = CorePens.GetPen(SKColors.Black, 2);
    protected override void GeneratePath(long idx)
    {
        var aspect = _samplesAspectRatio[idx];
        var rect = new SKRect(_samplesX[idx], _samplesY[idx], _samplesX[idx] + _samplesSize[idx], _samplesY[idx] + (_samplesSize[idx]  + _samplesSize[idx] * aspect));
        var path = SKPathMapper.GenerateArcOval(rect, _samplesConvexity[idx]);
        _paths[idx].Reset();
        _paths[idx].AddPath(path);
        _fills[idx] = CorePens.GetBrush(SKColor.FromHsl(_samplesHue[idx], _samplesSaturation[idx], _samplesLightness[idx]));
        _strokes[idx] = _defaultStroke;
    }

    // apple orange banana lemon lime cherry blueberry kiwi cherry peach

    public static List<(string, string)> FruitData = new List<(string, string)>()
    {
        ("Random", " -150.00,1050.00, -150.00,450.00, -40.00,50.00, 0.20,0.20, 0.00,360.00, -70.00,100.00, -40.00,60.00, -1.00,1.00"),//-150.00,1050.00, -150.00,450.00, -27.00,99.00, 0.82,0.76, 0.00,360.00, -70.00,100.00, -18.00,84.00, 0.66,1.00" ),
        ("Orange", "-150.00,1050.00, -150.00,450.00, -34.00,47.00, 0.04,0.04, -18.00,40.00, -90.00,100.00, -49.00,58.00, -1.00,1.00"),
        ("Grape", "-150.00,1050.00, -150.00,450.00, -14.00,24.00, -0.10,0.30, -238.00,332.00, -70.00,100.00, -28.00,47.00, -1.00,1.00"),
        ("Apple", "-150.00,1050.00, -150.00,450.00, -43.00,50.00, 0.18,0.02, 1.00,16.00, -70.00,100.00, -44.00,58.00, -1.00,1.00"),
        ("Papaya", "-150.00,1050.00, -150.00,450.00, -54.00,64.00, -0.12,0.28, -29.00,90.00, -70.00,100.00, -42.00,50.00, -0.56,0.88"),
        ("Cherry", "-150.00,1050.00, -150.00,450.00, -20.00,30.00, 0.06,0.02, 0.00,8.00, -80.00,100.00, -42.00,50.00, -1.00,1.00"),
        ("Kiwi", "-150.00,1050.00, -150.00,450.00, -28.00,30.00, -0.22,0.40, -38.00,43.00, -70.00,100.00, -31.00,35.00, -1.00,1.00"),
        ("Lime", "-150.00,1050.00, -150.00,450.00, -30.00,35.00, -0.10,0.28, -94.00,119.00, -70.00,100.00, -48.00,55.00, -0.81,0.91"),
        ("Lemon", "-150.00,1050.00, -150.00,450.00, -36.00,45.00, -0.10,0.28, -58.00,60.00, -100.00,100.00, -50.00,49.00, -0.63,0.91"),
        ("Banana", "-150.00,1050.00, -150.00,450.00, -72.00,98.00, 0.48,0.24, -53.00,66.00, -90.00,100.00, -50.00,53.00, 0.69,-0.38"),
    };
}
