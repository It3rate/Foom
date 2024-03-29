using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks;
public class CreateFocalTask : TaskBase, ICreateTask
{
    public Focal CreatedFocal;

    private Trait Trait { get; }
    public long StartPosition { get; }
    public long EndPosition { get; }

    public override bool IsValid => true;

    public CreateFocalTask(long startPosition, long endPosition)
    {
        StartPosition = startPosition;
        EndPosition = endPosition;
    }
    public override void RunTask()
    {
        if (CreatedFocal == null)
        {
            CreatedFocal = new Focal(StartPosition, EndPosition);
        }

        Trait?.FocalStore.Add(CreatedFocal.Id, CreatedFocal);
    }

    public override void UnRunTask()
    {
        Trait?.FocalStore.Remove(CreatedFocal.Id);
    }
}
