using Numbers.Drawing;
using Numbers.Mappers;
using NumbersCore.Primitives;
using SkiaSharp;

namespace Numbers.Agent
{
    public class HighlightSet
    {
        public Highlight ActiveHighlight { get; set; }
	    public SKPoint Position => ActiveHighlight?.OriginalPoint ?? SKPoint.Empty;
	    public SKPoint SnapPosition => ActiveHighlight?.SnapPoint ?? SKPoint.Empty;
	    //public List<Highlight> Highlights { get; set; } // todo: make selections multiple sub-highlights
	    public bool HasHighlight => ActiveHighlight?.Mapper != null;

        // copied values from start of change transaction, probably need a separate class as abilities expand
	    public SKSegment OriginalSegment { get; set; }
	    public Focal OriginalFocal { get; set; }

        public SKNumberMapper GetNumberMapper()
        {
            SKNumberMapper result = null;
            if (ActiveHighlight?.Mapper is SKNumberMapper nm)
            {
                result = nm;
            }
            return result;
        }

        public void Reset()
	    {
		    ActiveHighlight = null;
	    }
	    public void Set(Highlight activeHighlight)
	    {
		    ActiveHighlight = activeHighlight;
	    }

	    public void Clear()
	    {
		    ActiveHighlight = null;
            OriginalSegment = null;
            OriginalFocal = null;
	    }
    }
}
