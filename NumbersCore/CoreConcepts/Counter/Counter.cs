using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter;

// Q: Do typed numbers have to exist, as the domains are typed?
public class Counter : Number
{
    public long Resolution => Domain.BasisFocal.AbsLengthInTicks;
    public long StepSize { get; }
    public bool IsDirectionUp { get; set; } = true;

    public Counter(long maxCount, long resolution = 1, long stepSize = 1) :
        this(new CounterDomain(new Focal(0, resolution), new Focal(0, maxCount * resolution), "Counter"), stepSize)
    { }


    public Counter(Domain domain, long stepSize = 1) : base(new Focal(0, 0))
    {
        Domain = domain;
        Domain.AddNumber(this, false);
        StepSize = stepSize;
    }
    public bool IsComplete => Focal.EndPosition >= Domain.MinMaxFocal.Max;

    public long Step => Focal.EndPosition;
    public void Increment()
    {
        // never increment up to max value, so a max value is never reached (meaning always continue if in repeat until cycle)
        if (Focal.EndPosition < long.MaxValue - 2 && Focal.EndPosition > 0)
        {
            Focal.EndPosition += IsDirectionUp ? StepSize : -StepSize;
        }
    }

    public void SetToEnd()
    {
        Focal.EndPosition = Domain.MinMaxFocal.EndPosition;
    }
    public void Reset(long resetTicks = 0)
    {
        Focal.EndPosition = resetTicks;
    }

}
