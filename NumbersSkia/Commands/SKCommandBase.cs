using Numbers.Agent;
using Numbers.Mappers;
using NumbersAPI.CommandEngine;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace Numbers.Commands;

using Numbers.Drawing;
using System.Collections.Generic;

public class SKCommandBase : CommandBase
{
    public List<int>? PreviousSelection { get; }

    public MouseAgent MouseAgent => (MouseAgent)Agent;
    public SKMapper? Mapper { get; protected set; }
    public SKSegment? Guideline { get; }
    public Transform? HaltCondition { get; set; }

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

    public override bool IsComplete => HaltCondition?.IsFalse ?? false;

    public override void Completed()
    {
        base.Completed();
    }
}
