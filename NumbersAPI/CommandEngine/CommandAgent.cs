using NumbersCore.CoreConcepts;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace NumbersAPI.CommandEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CommandAgent : IAgent
    {
	    public Brain Brain => Workspace.Brain;
	    public Knowledge Knowledge => Brain.Knowledge;
        public Workspace Workspace { get; }

	    public CommandStack Stack { get; }

	    public CommandAgent(Workspace workspace)
	    {
		    Workspace = workspace;
		    Stack = new CommandStack(this);
        }

	    public void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
	    {
		    //Workspace.Update(currentTime, deltaTime);
            Stack.Update(currentTime, deltaTime);
	    }

        public virtual void ClearAll()
	    {
		    Stack.Clear();
        }
    }
}
