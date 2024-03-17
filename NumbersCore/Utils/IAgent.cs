using System.Collections.Generic;
using NumbersCore.CoreConcepts;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersCore.Utils
{
	public interface IAgent
    {
	    Brain Brain { get; }
	    Knowledge Knowledge { get; }
	    Workspace Workspace { get; }

	    void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);

        void ClearAll();
    }
}
