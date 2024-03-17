using System;
using System.Collections.Generic;
using NumbersAPI.Commands;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersAPI.CommandEngine
{
	public interface ICommandStack
    {
	    CommandAgent Agent { get; }
	    Brain Brain { get; }
        Workspace Workspace { get; }
        bool CanUndo { get; }
		int UndoSize { get; }
        bool CanRedo { get; }
		int RedoSize { get; }
		bool CanRepeat { get; }

		void Do(ICommand command);
		ICommand PreviousCommand();
		void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);

        bool Undo();
		void UndoAll();
		void UndoToIndex(int index);
		bool Redo();
		void RedoAll();
		void RedoToIndex(int index);
		void Clear();
	}

	public class CommandStack : ICommandStack
    {
	    public CommandAgent Agent { get; }
	    public Brain Brain => Agent.Brain;
	    public Workspace Workspace => Agent.Workspace;

        private int _stackIndex = 0;
		private readonly List<ICommand> _stack = new List<ICommand>(4096);

		private readonly List<ICommand> _toAdd = new List<ICommand>();
		private readonly List<ICommand> _toAddAfterDelay = new List<ICommand>();
        private readonly List<ICommand> _liveCommands = new List<ICommand>();
		private readonly List<ICommand> _toCommit = new List<ICommand>(); // changes
		private readonly List<ICommand> _toDelete = new List<ICommand>(); // commands that natrually end
		private readonly List<ICommand> _toTerminate = new List<ICommand>(); // commands that didn't naturally finish

		public readonly MillisecondNumber LastTime = MillisecondNumber.Zero(false);

        public bool CanUndo => _stackIndex > 0;
		public int UndoSize => _stackIndex;
        public bool CanRedo => RedoSize > 0;
		public int RedoSize => _stack.Count - _stackIndex;

		public CommandStack(CommandAgent agent)
		{
			Agent = agent;
		}

		public void Do(ICommand command)
		{
			command.Stack = this;
			command.Agent = Agent;

			if (command.LiveTimeSpan == null)
			{
				command.LiveTimeSpan = MillisecondNumber.Create(-LastTime.EndTicks + command.DefaultDelay, command.DefaultDuration);
			}

			if (command.DefaultDelay == 0)
			{
                if (command.CanUndo)
                {
	                AddStackCommand(command);
                }
                else
                {
	                AddLiveCommand(command);
                }
            }
			else
            {
	            _toAddAfterDelay.Add(command);
            }
		}

        public void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
		{
            // remove terminated commands (live commands in the toTerminate array)
            // copy live commands
            // loop commands
            //   if anim command iscomplete, add to toDelete list, remove handlers
            //   update each command
            // apply commits (on copy) if there are any
            // remove completed commands
            // add new commands and clear toAdd

            AddNewCommands(currentTime);
            UpdateLiveCommands(currentTime, deltaTime);
            PerformCommits();
            RemoveCompletedCommands();

            LastTime.SetWith(currentTime);
		}


        private void AddNewCommands(MillisecondNumber currentTime)
        {
	        var delayed = _toAddAfterDelay.ToArray();
	        foreach (var command in delayed)
	        {
		        if (currentTime.EndValue >= -command.LiveTimeSpan.StartValue)
		        {
			        if (command.CanUndo)
                    {
                        AddStackCommand(command);
			        }
			        else
			        {
				        AddLiveCommand(command);
			        }
			        _toAddAfterDelay.Remove(command);
		        }
	        }
        }

        private void AddStackCommand(ICommand command)
        {
	        RemoveRedoCommands();
            bool merged = AttemptToMerge(command);
	        if (!merged)
	        {
		        _stack.Add(command);
		        if (command.IsContinuous)
		        {
                    _liveCommands.Add(command);
		        }

		        _stackIndex++;
		        command.Execute();
	        }
        }
        private void AddLiveCommand(ICommand command)
        {
	        if (command.IsContinuous)
	        {
		        _liveCommands.Add(command);
	        }
	        command.Execute();
        }


        private void RemoveRedoCommands()
        {
	        if (CanRedo)
	        {
		        _stack.RemoveRange(_stackIndex, RedoSize);
	        }
        }
        private bool AttemptToMerge(ICommand command) =>  PreviousCommand()?.TryMergeWith(command) ?? false;
        private bool UpdateLiveCommands(MillisecondNumber currentTime, MillisecondNumber deltaTime)
        {
	        bool result = false;
	        var commands = _liveCommands.ToArray();
	        foreach (var command in commands)
	        {
		        if (command.IsContinuous)
		        {
			        if (command.IsComplete())
			        {
				        _toDelete.Add(command);
                        command.Completed();
				        // fire end animation event
				        // remove end animation event handlers
			        }
			        else
			        {
				        command.Update(currentTime, deltaTime);
                    }
			        result = true;
		        }
	        }
            return result;
        }
        private bool PerformCommits() { return true; }

        private bool RemoveCompletedCommands()
        {
	        var result = false;
	        if (_toDelete.Count > 0)
	        {
		        var commands = _toDelete.ToArray();
		        foreach (var command in commands)
		        {
			        _liveCommands.Remove(command);
		        }
		        _toDelete.Clear();
		        result = true;
	        }
	        return result;
        }

        public ICommand PreviousCommand() => CanUndo ? _stack[_stackIndex - 1] : default;

		public bool Undo()
		{
			var result = false;
			if (CanUndo)
			{
				_stackIndex--;
				_stack[_stackIndex].Unexecute();
				result = true;
			}

			return result;
		}

		public void UndoAll()
		{
			while (CanUndo)
			{
				Undo();
			}
		}

		public void UndoToIndex(int index)
		{
			while (_stackIndex >= index)
			{
				Undo();
			}
		}

		public bool Redo()
		{
			var result = false;
			if (CanRedo)
			{
				_stack[_stackIndex].Execute();
				_stackIndex++;
				result = true;
			}

			return result;
		}

		public void RedoAll()
		{
			while (CanRedo)
			{
				Redo();
			}
		}

		public void RedoToIndex(int index)
		{
			while (_stackIndex < index)
			{
				Redo();
			}
		}

		public bool Repeat()
		{
			return false;
		}

		public void Clear()
		{
            _liveCommands.Clear();
			_stack.Clear();
			_toAddAfterDelay.Clear();
			_toDelete.Clear();
			_toCommit.Clear(); 
            _toTerminate.Clear();

			_stackIndex = 0;
		}

		public bool CanRepeat => true;

		public List<ICommand> GetRepeatable()
		{
			return null;
		}

		private void AddAndExecuteCommand(ICommand command)
		{
			command.Agent = Agent;
			RemoveRedoCommands();
			_stackIndex++;
			_stack.Add(command);
			command.Execute();
		}

		// saving
		// temporal command clock, elements and updates.
	}
}