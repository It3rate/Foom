using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;

namespace NumbersAPI.CoreTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateDomainTask : TaskBase, ICreateTask
    {
	    public Domain Domain;

	    public Trait Trait { get; }
	    public Focal BasisFocal { get; }
        public Focal MinMax { get; }
        public string Name { get; }

        public override bool IsValid => true;

	    public CreateDomainTask(Trait trait, Focal basisFocal, Focal minMax, string name)
	    {
		    Trait = trait;
		    BasisFocal = basisFocal;
		    MinMax = minMax;
            Name = name;
	    }
	    public override void RunTask()
	    {
		    if (Domain == null)
		    {
			    Domain = new Domain(Trait, BasisFocal, MinMax, Name);
		    }
		    else
		    {
                Trait.DomainStore.Add(Domain.Id, Domain);
		    }
		    Agent.Workspace.AddDomains(false, Domain);
	    }

	    public override void UnRunTask()
	    {
		    Agent.Workspace.RemoveDomains(false, Domain);
		    Trait.DomainStore.Remove(Domain.Id);
        }
    }
}
