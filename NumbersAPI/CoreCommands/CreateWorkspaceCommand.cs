using NumbersAPI.CommandEngine;
using NumbersAPI.CoreTasks;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateWorkspaceCommand : CommandBase
    {
	    private CreateWorkspaceTask WorkspaceTask;
	    public new Workspace Workspace => WorkspaceTask?.CreatedWorkspace;

	    public CreateWorkspaceCommand()
	    {
	    }

	    public override void Execute()
	    {
		    base.Execute();
		    if (WorkspaceTask == null)
		    {
			    WorkspaceTask = new CreateWorkspaceTask();
		    }
		    AddTaskAndRun(WorkspaceTask);
	    }

	    public override void Unexecute()
	    {
		    base.Unexecute();
	    }

	    public override void Completed()
	    {
	    }
    }
}
