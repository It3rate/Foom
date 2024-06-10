namespace NumbersSkia.Commands;
using NumbersSkia.Agent;
using NumbersSkia.Drawing;
using NumbersSkia.Mappers;
using NumbersCore.CoreConcepts.Time;
using SkiaSharp;

public class AddSKPathCommand : SKCommandBase
{
    public SKPathMapper PathMapper;

    private MouseAgent _agent;
    private SKPaint _paint;

    public AddSKPathCommand(MouseAgent agent, SKPaint paint = null, SKSegment guideline = null) : base(guideline)
    {
        _agent = agent;
        _paint = paint;
    }

    public override void Execute()
    {
        if (PathMapper == null)
        {
            PathMapper = new SKPathMapper(_agent, Guideline);
            if (_paint != null)
            {
                PathMapper.Pen = _paint;
            }
        }
        base.Execute(); // no tasks, but call anyway
        _agent.WorkspaceMapper.AddPathMapper(PathMapper);
    }

    public override void Unexecute()
    {
        base.Unexecute();
        _agent.WorkspaceMapper.RemovePathMapper(PathMapper);
    }

    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
        base.Update(currentTime, deltaTime);
    }

    public override void Completed()
    {
    }
}
