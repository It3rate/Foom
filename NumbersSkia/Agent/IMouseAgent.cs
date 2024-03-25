
using NumbersAPI.CommandEngine;
using NumbersCore.Utils;
using NumbersSkia.Agent;

namespace Numbers.Agent;

	public interface IMouseAgent : IAgent
{
    CommandStack Stack { get; }

    bool IsPaused { get; set; }

    HighlightSet SelBegin { get; }
    HighlightSet SelCurrent { get; }
    HighlightSet SelHighlight { get; }
    HighlightSet SelSelection { get; }

    bool MouseDown(MouseArgs e);
	    bool MouseMove(MouseArgs e);
	    bool MouseUp(MouseArgs e);
	    bool KeyDown(KeyArgs e);
	    bool KeyUp(KeyArgs e);
	    bool MouseDoubleClick(MouseArgs e);
	    bool MouseWheel(MouseArgs e);
}
