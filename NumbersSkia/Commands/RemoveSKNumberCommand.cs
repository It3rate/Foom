﻿namespace NumbersSkia.Commands;
using NumbersSkia.Drawing;
using NumbersSkia.Mappers;
using NumbersAPI.CoreTasks;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

public class RemoveSKNumberCommand : SKCommandBase
{
    public SKNumberMapper NumberMapper => (SKNumberMapper)Mapper;

    public SelectionDeleteTask DeleteTask { get; private set; }
    public SKNumberMapper Number { get; }
    public Domain Domain { get; }
    public SKSegment UnitSegment { get; }

    public RemoveSKNumberCommand(SKNumberMapper numberMapper) : base(numberMapper.Guideline)
    {
        Mapper = numberMapper;
        Agent = numberMapper.Agent;
        Tasks.Add(new RemoveNumberTask(numberMapper.Number));
    }

    public override void Execute()
    {
        NumberMapper.MouseAgent.ClearHighlights();
        NumberMapper.DomainMapper.RemoveNumberMapper(NumberMapper);
        base.Execute();
    }

    public override void Unexecute()
    {
        base.Unexecute();
        NumberMapper.DomainMapper.AddNumberMapper(NumberMapper);
    }

    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
        base.Update(currentTime, deltaTime);
    }

    public override void Completed()
    {
    }
}
