namespace Numbers.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Agent;
    using Numbers.Drawing;
    using Numbers.Utils;
    using NumbersCore.CoreConcepts.Spatial;
    using NumbersCore.Primitives;
    using SkiaSharp;

    public class SKImageMapper : SKMapper
    {
        public SKRect Bounds { get; set; }
        public Focal AspectRatio => (Focal)MathElement;
        public SKBitmap Bitmap { get; set; }
        public SKPaint BorderPen { get; set; } = null;
        private string _path { get; set; }

        public SKImageMapper(MouseAgent agent, string path, SKSegment guideline = null) : base(agent, new Focal(800, 800), guideline)
        {
            SetBitmap(path);
        }

        public void SetBitmap(string imageName)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imageFolder = "Images";
            _path = Path.Combine(appDirectory, imageFolder, imageName);
            if (_path != null && _path != "" && File.Exists(_path))
            {
                Bitmap = SKBitmap.Decode(_path);
                AspectRatio.Reset(Bitmap.Width, Bitmap.Height);
                var w = Bitmap.Width;
                var h = Bitmap.Height;
                Guideline = (Guideline == null) ? new SKSegment(0, 0, w, h) : Guideline;
                var ratio = Guideline.AbsLength / (float)w;
                var x = Guideline.StartPoint.X;
                var y = Guideline.StartPoint.Y;
                Bounds = new SKRect(x, y, w * ratio + x, h * ratio + y);
            }
        }
        public override SKPath GetHighlightAt(Highlight highlight)
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            if (Bitmap != null)
            {
                Renderer.DrawBitmap(Bitmap, Bounds);
            }
        }

        public void Reset(string path)
        {
            SetBitmap(path);
        }
    }
}
