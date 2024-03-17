using NumbersAPI.Commands;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersAPI.CommandEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class CommandBase : ICommand
    {
	    public int Id { get; }
	    public CommandAgent Agent { get; set; } // Agent is set by the command stack.

	    public Brain Brain => Agent.Brain;
	    public Workspace Workspace => Agent.Workspace;

	    public List<ITask> Tasks { get; } = new List<ITask>();
        protected int _taskIndex = 0;

        public virtual ICommandStack Stack { get; set; }

        public CommandBase()
        {
        }
	    public virtual bool AppendElements()
	    {
		    throw new NotImplementedException();
	    }
	    public virtual bool RemoveElements()
	    {
		    throw new NotImplementedException();
	    }
	    public virtual bool IsMergableWith(ICommand command)
	    {
		    throw new NotImplementedException();
	    }
	    public virtual bool TryMergeWith(ICommand command) => false;

	    public MillisecondNumber LiveTimeSpan { get; set; }
	    public long DurationMS => LiveTimeSpan.TickCount;
	    public long DefaultDelay { get; set; }
	    public long DefaultDuration { get; set; }

        public virtual int RepeatCount { get; }
	    public virtual int RepeatIndex { get; }

	    public virtual bool IsActive { get; protected set; }
	    public virtual bool IsContinuous => false;
	    public virtual bool CanUndo => true;
	    public virtual bool IsRetainedCommand => true;

        public virtual bool IsRepeatable() => false;
	    public virtual bool IsComplete() => true;
	    public virtual bool Evaluate() => true;

        public virtual void Execute()
        {
            // remember selection state
            // stamp times
            // run tasks
            // select new element
            foreach (var task in Tasks)
            {
                task.Agent = Agent;
                task.RunTask();
                _taskIndex++;
            }
        }
        public virtual void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
        {
        }
        public virtual void Unexecute()
        {
	        while (_taskIndex > 0)
	        {
		        _taskIndex--;
		        Tasks[_taskIndex].UnRunTask();
	        }
            Tasks.Clear();
        }
        public virtual void Completed() { }

        public void AddTask(ITask task)
        {
	        task.Agent = Agent;
	        Tasks.Add(task);
        }
        public void AddTasks(params ITask[] tasks)
        {
	        foreach (var task in tasks)
	        {
		        AddTask(task);
	        }
        }
        public void AddTaskAndRun(ITask task)
        {
	        AddTask(task);
	        RunToEnd();
        }
        public void AddTasksAndRun(params ITask[] tasks)
        {
	        foreach (var task in tasks)
	        {
		        AddTask(task);
	        }
	        RunToEnd();
        }
        protected void RunToEnd()
        {
	        while (_taskIndex < Tasks.Count)
	        {
		        Tasks[_taskIndex].RunTask();
		        _taskIndex++;
	        }
        }

        public virtual ICommand Duplicate()
        {
	        return null;
        }
    }
}
