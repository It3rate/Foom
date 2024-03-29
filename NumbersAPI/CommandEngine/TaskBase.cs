using NumbersAPI.Commands;
using NumbersAPI.Motion;
using NumbersCore.CoreConcepts.Time;

namespace NumbersAPI.CommandEngine;
public abstract class TaskBase : ITask
{
    private static int _idCounter = 1;
    public int Id { get; }

    public ICommand Command { get; }
    public CommandAgent Agent { get; set; }
    public TaskTimer Timer { get; }

    public abstract bool IsValid { get; }
    public virtual void Initialize() { }

    protected TaskBase()
    {
        Id = _idCounter++;
        Timer = new TaskTimer(MillisecondNumber.Create(0));
    }
    public virtual void RunTask() { }
    public virtual void UnRunTask() { }

}
