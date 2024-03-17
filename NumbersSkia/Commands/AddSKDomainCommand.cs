using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numbers.Agent;
using Numbers.Drawing;
using Numbers.Mappers;
using NumbersAPI.CommandEngine;
using NumbersAPI.Commands;
using NumbersAPI.CoreCommands;
using NumbersAPI.CoreTasks;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace Numbers.Commands
{
    public class AddSKDomainCommand : SKCommandBase
    {
	    public SKDomainMapper DomainMapper => (SKDomainMapper)Mapper;

	    public CreateDomainTask CreateDomainTask { get; private set; }
	    public Domain Domain => ExistingDomain ?? CreateDomainTask?.Domain;
        public Domain ExistingDomain { get; }
	    public Domain CreatedDomain => CreateDomainTask?.Domain;

	    public SKSegment UnitSegment { get; }

        //public AddSKDomainCommand(MouseAgent agent, Domain domain, SKSegment guideline, SKSegment unitSegment = null) : base(guideline)
        //{
	       // ExistingDomain = domain;
        //    UnitSegment = unitSegment;
        //}
	    public AddSKDomainCommand(Trait trait, long basisStart, long basisEnd, long minMaxStart, long minMaxEnd, SKSegment guideline, SKSegment unitSegment, string name) : base(guideline)
	    {
            CreateDomainTask = new CreateDomainTask(trait, new Focal(basisStart, basisEnd), new Focal(minMaxStart, minMaxEnd), name);
		    UnitSegment = unitSegment;
        }

	    public override void Execute()
	    {
            if(CreateDomainTask != null)
            {
                Tasks.Add(CreateDomainTask);
            }
		    base.Execute();

            Mapper = MouseAgent.WorkspaceMapper.GetOrCreateDomainMapper(Domain, Guideline, UnitSegment);
            if (Guideline.StartPoint.X > Guideline.EndPoint.X)
            {
                DomainMapper.FlipRenderPerspective();
            }
        }

	    public override void Unexecute()
	    {
		    base.Unexecute();
		    MouseAgent.WorkspaceMapper.RemoveDomainMapper(DomainMapper);
        }

	    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
	    {
		    base.Update(currentTime, deltaTime);
	    }

        public override void Completed()
	    {
	    }
    }
}
