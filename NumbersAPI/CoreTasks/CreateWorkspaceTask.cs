using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateWorkspaceTask : TaskBase, ICreateTask
    {
	    public Workspace CreatedWorkspace;

	    public override bool IsValid => true;

	    public CreateWorkspaceTask()
        {
	    }
	    public override void RunTask()
	    {
		    if (CreatedWorkspace == null)
		    {
			    CreatedWorkspace = new Workspace(Agent.Brain);
		    }
		    else
		    {
			    Agent.Brain.Workspaces.Add(CreatedWorkspace);
		    }
	    }

	    public override void UnRunTask()
	    {
		    Agent.Brain.Workspaces.Remove(CreatedWorkspace);
	    }
    }
}
