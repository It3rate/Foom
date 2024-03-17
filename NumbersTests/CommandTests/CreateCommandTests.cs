using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersAPI.CommandEngine;
using NumbersAPI.Commands;
using NumbersAPI.CoreCommands;
using NumbersAPI.CoreTasks;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersTests.CommandTests
{
    [TestClass]
	public class CreateCommandTests
	{
		private CommandStack _stack;
		private Brain _brain;
		private Workspace _workspace;
		private CommandAgent _agent;
        private Trait _trait;

		[TestInitialize]
		public void Init()
		{
			_brain = Brain.ActiveBrain;
			_workspace = new Workspace(_brain);
			_agent = new CommandAgent(_workspace);
            _stack = new CommandStack(_agent);
			_trait = Trait.CreateIn(_brain, "create command tests");
        }

		[TestMethod]
		public void WorkspaceCommandTests()
		{
			var command = new CreateWorkspaceCommand();
			_stack.Do(command);
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(1, command.Tasks.Count);
			Assert.AreEqual(command.Workspace.Id, _brain.Workspaces[command.Workspace.Id].Id);
			_stack.Undo();
			Assert.AreEqual(0, _stack.UndoSize);
			Assert.AreEqual(0, command.Tasks.Count);
			_stack.Redo();
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(1, command.Tasks.Count);
		}

		[TestMethod]
		public void TraitCommandTests()
		{
			var command = new CreateTraitCommand("TraitTest");
			_stack.Do(command);
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(1, command.Tasks.Count);
			Assert.AreEqual("TraitTest", _brain.TraitStore[command.Trait.Id].Name);
			_stack.Undo();
			Assert.AreEqual(0, _stack.UndoSize);
			Assert.AreEqual(0, command.Tasks.Count);
			_stack.Redo();
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(1, command.Tasks.Count);
		}
        [TestMethod]
		public void DomainCommandTests()
		{
			var command = new CreateDomainCommand(_trait, 0, 10, -1000, 1000, "DomainCommandTests");
			_stack.Do(command);
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(0, _stack.RedoSize);
            Assert.AreEqual(3, command.Tasks.Count);
			_stack.Undo();
			Assert.AreEqual(0, _stack.UndoSize);
			Assert.AreEqual(1, _stack.RedoSize);
            Assert.AreEqual(0, command.Tasks.Count);
			_stack.Redo();
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(3, command.Tasks.Count);
        }

		[TestMethod]
		public void NumberCommandTests()
		{
			var domainCommand = new CreateDomainCommand(_trait, 0, 10, -1000, 1000, "NumberCommandTests");
			_stack.Do(domainCommand);

			var command = new CreateNumberCommand(domainCommand.Domain, 100, 200);
			_stack.Do(command);
			Assert.AreEqual(2, _stack.UndoSize);
			Assert.AreEqual(0, _stack.RedoSize);
			Assert.AreEqual(100, command.Number.Focal.StartPosition);
			Assert.AreEqual(200, command.Number.Focal.EndPosition);
			_stack.Undo();
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(1, _stack.RedoSize);
			Assert.AreEqual(0, command.Tasks.Count);
			_stack.Redo();
			Assert.AreEqual(2, _stack.UndoSize);
			Assert.AreEqual(1, command.Tasks.Count);
		}

		[TestMethod]
		public void DelayCommandTests()
		{
			var domainCommand = new CreateDomainCommand(_trait, 0, 10, -1000, 1000, "DelayCommandTests");
			_stack.Do(domainCommand);
			var command = new CreateNumberCommand(domainCommand.Domain, 100, 200) {DefaultDelay = -500};
			_stack.Do(command);
			Assert.AreEqual(1, _stack.UndoSize);
			Assert.AreEqual(0, _stack.RedoSize);
            var time = MillisecondNumber.Create(0, 600);
            _stack.Update(time, time);
            Assert.AreEqual(2, _stack.UndoSize);
            Assert.AreEqual(0, _stack.RedoSize);
            _stack.Undo();
            Assert.AreEqual(1, _stack.UndoSize);
            Assert.AreEqual(1, _stack.RedoSize);
            _stack.Redo();
            Assert.AreEqual(2, _stack.UndoSize);
            Assert.AreEqual(0, _stack.RedoSize);
        }
    }
}
