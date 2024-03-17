namespace MathDemo.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Agent;
    using Numbers.Controls;
    using Numbers.Mappers;
    using Numbers.Renderer;
    using NumbersCore.CoreConcepts.Counter;
    using NumbersCore.CoreConcepts.Optical;
    using NumbersCore.CoreConcepts.Spatial;
    using NumbersCore.Primitives;
    using NumbersCore.Utils;
    using SkiaSharp;

    public class ShapeControl : UIControlBase
    {
        // radius
        // inner radius offset
        // outline color
        // fill color
        // points
        // rotation
        // x range
        // y range
        public Number Radius { get; private set; }
        public Number RadiusOffset { get; private set; }
        public HSLDomain Fill { get; private set; } = new HSLDomain();
        public HSLDomain Stroke { get; private set; } = new HSLDomain();
        public ExtentDomain Position { get; private set; } = new ExtentDomain(500, 500);
        public Number StrokeWidth { get; private set; }
        public Number Rotation { get; private set; }
        public Number Points { get; private set; }



        private float[] _samplesRadius => _samples[0];
        private float[] _samplesRadiusOffset => _samples[1];
        private float[] _samplesHue => _samples[2];
        private float[] _samplesSaturation => _samples[3];
        private float[] _samplesLightness => _samples[4];
        private float[] _samplesStrokeHue => _samples[5];
        private float[] _samplesStrokeSaturation => _samples[6];
        private float[] _samplesStrokeLightness => _samples[7];
        private float[] _samplesX => _samples[8];
        private float[] _samplesY => _samples[9];
        private float[] _samplesStrokeWidth => _samples[10];
        private float[] _samplesRotation => _samples[11];
        private float[] _samplesPoints => _samples[12];

        public ShapeControl(MouseAgent agent, int top, int left, int width, int height, long count = 20) : base(agent, count)
        {
            Radius = CreateProperty("Radius", 25, 50, 0, 50);
            RadiusOffset = CreateProperty("Radius Offset", 20, 100, 0, 200);
            AddPolyProperty(Fill, 0, 360, 50, 80, 40, 60);
            AddPolyProperty(Stroke, 0, 10, 0, 10, 0, 10);
            StrokeWidth = CreateProperty("StrokeWidth", 0, 5, 0, 10);
            AddPolyProperty(Position, left, left + width, top, top + height);
            Rotation = CreateProperty("Rotation", 0, 360, 0, 360);
            Points = CreateProperty("Points", 3, 8, 0, 15, 3);

            CreateDomainMaps();
            Update();
        }
        protected override void CreateDomainMaps()
        {
            _numbers.Clear();

            _numbers.Add(Radius);
            _numbers.Add(RadiusOffset);
            _numbers.Add(Fill.Hue);
            _numbers.Add(Fill.Saturation);
            _numbers.Add(Fill.Lightness);
            _numbers.Add(Stroke.Hue);
            _numbers.Add(Stroke.Saturation);
            _numbers.Add(Stroke.Lightness);
            _numbers.Add(Position.Horizontal);
            _numbers.Add(Position.Vertical);
            _numbers.Add(StrokeWidth);
            _numbers.Add(Rotation);
            _numbers.Add(Points);

            _samples = new float[_numbers.Count][];
        }

        protected override void GeneratePath(long idx)
        {
            var pts = SKPathMapper.GenerateStar(_samplesX[idx], _samplesY[idx], _samplesRadius[idx], _samplesRadius[idx], (int)_samplesPoints[idx], _samplesRadiusOffset[idx] / 100f);
            _paths[idx].Reset();
            _paths[idx].AddPoly(pts);
            _fills[idx] = CorePens.GetBrush(SKColor.FromHsl(_samplesHue[idx], _samplesSaturation[idx], _samplesLightness[idx]));
            _strokes[idx] = CorePens.GetPen(SKColor.FromHsl(_samplesStrokeHue[idx], _samplesStrokeSaturation[idx], _samplesStrokeLightness[idx]), _samplesStrokeWidth[idx]);
        }
    }
}
