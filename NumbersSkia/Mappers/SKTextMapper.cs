namespace Numbers.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Agent;
    using Numbers.Drawing;
    using Numbers.Primitives;
    using NumbersCore.CoreConcepts.Spatial;
    using SkiaSharp;

    public class SKTextMapper : SKMapper
    {
        public TextElement TextElement => (TextElement)MathElement;
        public List<string> Lines { get => TextElement.Lines; set => TextElement.Lines = value; }
        public SKPaint Pen { get; set; }
        private SKPaint _ghostPen { get; set; }
        private int _penChangeIndex = 0; // make this more elaborate as needed with focals etc. For now, one change.

        public SKTextMapper(MouseAgent agent, string[] lines, SKSegment guideline) : base(agent, new TextElement(lines), guideline)
        {
            Pen = Pens.TextBrush;
        }
        public void GhostTextBefore(int index, SKPaint pen)
        {
            _penChangeIndex = index;
            _ghostPen = pen;
        }

        public override SKPath GetHighlightAt(Highlight highlight)
        {
            throw new NotImplementedException();
        }
        public override void Draw()
        {
            var sp = Guideline.StartPoint;
            int index = 0;
            foreach (var line in Lines)
            {
                var pen = index >= _penChangeIndex ? Pen : _ghostPen;
                Renderer.DrawTextAt(sp, line, pen);
                sp.Y += 30;
                index++;
            }
        }
    }
}
