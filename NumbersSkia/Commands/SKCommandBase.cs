using Numbers.Agent;
using Numbers.Mappers;
using NumbersAPI.CommandEngine;
using NumbersAPI.Motion;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace Numbers.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Numbers.Drawing;

    public class SKCommandBase : CommandBase
    {
	    public List<int> PreviousSelection { get; }

	    public SKMapper Mapper { get; protected set; }
	    public SKSegment Guideline { get; }
	    public MouseAgent MouseAgent => (MouseAgent) Agent;
        public Evaluation HaltCondition { get; set; }

	    public SKCommandBase(SKSegment guideline)
	    {
            Guideline = guideline;
	    }
	    public override void Execute()
	    {
		    base.Execute();
	    }

	    public override void Unexecute()
	    {
		    base.Unexecute();
	    }

	    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
	    {
		    base.Update(currentTime, deltaTime);
	    }

	    public override bool IsComplete() => !HaltCondition?.EvaluateFlags() ?? false;

	    public override void Completed()
	    {
		    base.Completed();
        }
    }
}
