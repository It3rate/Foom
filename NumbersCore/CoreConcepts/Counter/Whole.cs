using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter;

public class Whole : Number
{
    long _orgValue;
    public Whole(long value) : base(new Focal(0, value))
    {
        _orgValue = value;
        Domain = new CounterDomain(new Focal(0, 1), new Focal(0, long.MaxValue), "Whole");
        Domain.AddNumber(this, false);
    }

    public void Reset()
    {
        Focal.EndPosition = _orgValue;
    }

}
