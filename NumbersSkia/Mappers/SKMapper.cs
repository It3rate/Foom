using Numbers.Agent;
using Numbers.Drawing;
using Numbers.Renderer;
using NumbersCore.Primitives;
using NumbersCore.Utils;
using SkiaSharp;
// using OpenTK.Graphics.ES30;

namespace Numbers.Mappers
{
    public abstract class SKMapper : IDrawableElement
	{
		public int Id { get; set; }
        protected static int idCounter = 0;

		public MouseAgent Agent { get; }
		public Brain Brain => Agent.Brain;
		public Workspace Workspace => Agent.Workspace;
		public SKWorkspaceMapper WorkspaceMapper => Agent.WorkspaceMapper;
        public CoreRenderer Renderer => Agent.Renderer;
        public SKCanvas Canvas => Renderer.Canvas;
        protected CorePens Pens => Renderer.Pens;

        public bool Do2DRender { get; set; } = false;

        public IMathElement MathElement { get; protected set; }
        public SKSegment Guideline { get; set; } = new SKSegment(0, 0, 1, 1);
        public SKSegment InvertedGuideline => new SKSegment(Guideline.StartPoint, Guideline.StartPoint - (Guideline.EndPoint - Guideline.StartPoint));

        public SKPoint StartPoint
        {
	        get => Guideline.StartPoint;
	        set => Reset(value, EndPoint);
        }
        public SKPoint MidPoint => Guideline.Midpoint;
        public SKPoint EndPoint
        {
	        get => Guideline.EndPoint;
	        set => Reset(StartPoint, value);
        }
        public SKPoint[] EndPoints => new SKPoint[] { StartPoint, EndPoint };

        protected SKMapper(MouseAgent agent, IMathElement element, SKSegment guideline = default)
        {
            Id = element.Id; // use wrapped element id
            idCounter++; // just to track creation count

            Agent = agent;
	        MathElement = element;
	        Guideline = guideline ?? new SKSegment(0, 0, 1, 1);
        }
        public virtual void Reset(SKPoint startPoint, SKPoint endPoint)
        {
	        Guideline.Reset(startPoint, endPoint);
        }
        public virtual void Reset(SKSegment segment)
        {
	        Reset(segment.StartPoint, segment.EndPoint);
        }

        //public abstract SKPoint StartPoint { get; set; }
        //public abstract SKPoint MidPoint { get; }
        //   public abstract SKPoint EndPoint { get; set; }

        public abstract void Draw();


        public abstract SKPath GetHighlightAt(Highlight highlight);

    }
}
