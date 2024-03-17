using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateTraitTask : TaskBase, ICreateTask
    {
	    public Trait Trait;
	    public string Name;
        public override bool IsValid => true;
	    public CreateTraitTask(string name = "")
	    {
		    Name = name;
	    }
	    public override void RunTask()
	    {
		    if (Trait == null)
		    {
			    Trait = Trait.CreateIn(Agent.Brain, Name);
		    }
		    else
		    {
			    Agent.Brain.AddTrait(Trait);
		    }
	    }

	    public override void UnRunTask()
	    {
		    Agent.Brain.RemoveTrait(Trait);
        }
    }
}
