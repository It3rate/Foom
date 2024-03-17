using NumbersAPI.CommandEngine;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateTraitCommand : CommandBase
    {
	    private CreateTraitTask TraitTask;
	    public Trait Trait => TraitTask?.Trait;

	    public string Name { get; }

	    public CreateTraitCommand(string name)
	    {
		    Name = name;
	    }

	    public override void Execute()
	    {
		    base.Execute();
		    if (TraitTask == null)
		    {
			    TraitTask = new CreateTraitTask(Name);
		    }
		    AddTaskAndRun(TraitTask);
	    }

	    public override void Unexecute()
	    {
		    base.Unexecute();
	    }

	    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
	    {
	    }

	    public override void Completed()
	    {
	    }
    }
}
