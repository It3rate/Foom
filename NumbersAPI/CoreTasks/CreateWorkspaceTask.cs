using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks;
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
