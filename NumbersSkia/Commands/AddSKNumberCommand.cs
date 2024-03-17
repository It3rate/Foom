using Numbers.Agent;
using Numbers.Mappers;
using Numbers.Utils;
using NumbersAPI.CoreCommands;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace Numbers.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersAPI.CoreTasks;

    public class AddSKNumberCommand : SKCommandBase
    {
	    public SKNumberMapper NumberMapper => (SKNumberMapper)Mapper;
	    public SKDomainMapper DomainMapper { get; }

        public override bool IsContinuous => false;//true;

        private CreateNumberByRangeTask NumberByRangeTask;
        //public CreateNumberCommand CreateNumberCommand { get; private set; }
        //public Number Number => ExistingNumber ?? CreatedNumber;
	    public Number CreatedNumber => NumberByRangeTask?.Number;
	    //public Number ExistingNumber { get; }

        public AddSKNumberCommand(SKDomainMapper domainMapper, PRange range) : base(domainMapper.SegmentAlongGuideline(range))
        {
	        DomainMapper = domainMapper;
            NumberByRangeTask = new CreateNumberByRangeTask(domainMapper.Domain, range);
            //DefaultDuration = 2100;
            //DefaultDelay = -900;
        }
	    //public AddSKNumberCommand(SKDomainMapper domainMapper, Number existingNumber) : base(domainMapper.SegmentAlongGuideline(existingNumber.Value))
	    //{
		   // DomainMapper = domainMapper;
		   // ExistingNumber = existingNumber;
	    //}

        public override void Execute()
	    {
            Tasks.Add(NumberByRangeTask);
		    base.Execute();

		    if (Mapper == null)
		    {
			    //_targetNumber = CreateNumberCommand.Number.Clone();
			    //CreateNumberCommand.Number.Value = new PRange(0.0, 1.0);
                // HaltCondition = new Evaluation(CreateNumberCommand.Number, _targetNumber, FilterOperator.B_Implies_A);
                Mapper = new SKNumberMapper(MouseAgent, CreatedNumber);

            }
		    DomainMapper.AddNumberMapper(NumberMapper);
            MouseAgent.Workspace.AddElements(CreatedNumber);
        }

        private Number _targetNumber;

	    public override void Unexecute()
	    {
		    base.Unexecute();
		    MouseAgent.Workspace.RemoveElements(CreatedNumber);
		    DomainMapper.RemoveNumberMapper(NumberMapper);
        }

        private double _t = 0.001;
	    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
	    {
		   base.Update(currentTime, deltaTime);
		   _t = Math.Sin(LiveTimeSpan.TValueOf(currentTime.EndValue));
		  // CreateNumberCommand.Number.InterpolateFromOne(_targetNumber.Value, _t);
        }

	    public override void Completed()
        {
	   //     if (IsComplete())
	   //     {
		  //      _t = 1.0;
				//CreateNumberCommand.Number.Value = _targetNumber.Value;
    //        }
        }
    }
}
