using NumbersAPI.CommandEngine;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersAPI.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    // selection range - segment, first, all, not, direction
    // motion range
    // motion type (ideally as a range) prepend, postpend, scale, or, and, not...
    // repeat range
    // duration range
    // termination range & bool
    //WorkspaceKind WorkspaceKind { get; }
    // Commands happen over time, can be repeatable, appendable, and can be evaluated to check for termination.
    // Commands are more or less formulas.
    public interface ICommand
    {
	    int Id { get; }
        CommandAgent Agent { get; set; }
        Brain Brain { get; }
        Workspace Workspace { get; }
        ICommandStack Stack { get; set; }

        // todo: rather than time commands, all commands will be formulas that exit when their stop condition is met.
        MillisecondNumber LiveTimeSpan { get; set; }
	    long DurationMS { get; }
	    long DefaultDelay { get; set; }
	    long DefaultDuration { get; set; }

        bool AppendElements();
	    bool RemoveElements();
	    bool IsMergableWith(ICommand command);
	    bool TryMergeWith(ICommand command);

        int RepeatCount { get; } // Number - this will be Number once default traits are in.
        int RepeatIndex { get; } // Number - this will be Number once default traits are in.

        bool IsActive { get; }
	    bool IsContinuous { get; }
	    bool CanUndo { get; }
	    bool IsRetainedCommand { get; }

        bool IsRepeatable();
        bool IsComplete();

        void Execute();
	    void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);
	    void Completed();
	    bool Evaluate();
	    void Unexecute();

	    List<ITask> Tasks { get; }
	    void AddTask(ITask task);
	    void AddTasks(params ITask[] tasks);
	    void AddTaskAndRun(ITask task);
	    void AddTasksAndRun(params ITask[] tasks);
	    //void RunToEnd();

	    // event EventHandler OnExecute;
	    // event EventHandler OnUpdate;
	    // event EventHandler OnUnexecute;
	    // event EventHandler OnCompleted;

	    ICommand Duplicate();
    }
}
