using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks;
public class CreateBrainTask : TaskBase, ICreateTask
{
    public Brain CreatedBrain;

    public override bool IsValid => true;

    public CreateBrainTask()
    {
    }
    public override void RunTask()
    {
        if (CreatedBrain == null)
        {
            CreatedBrain = new Brain();
        }
        else
        {
            Brain.Brains.Add(CreatedBrain);
        }
    }

    public override void UnRunTask()
    {
        Brain.Brains.Remove(CreatedBrain);
    }
}
