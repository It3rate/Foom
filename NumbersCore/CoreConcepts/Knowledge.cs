using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts
{
	public class Knowledge
    {
        public static Knowledge Instance { get; private set; } // todo: integrate knowledge at the brain level.
	    public Brain Brain { get; }

	    public Knowledge(Brain brain)
	    {
		    Brain = brain;
		    Instance = this;
            Initialize();
	    }

	    private void Initialize()
	    {
            // these typed domains will be loaded from a file or something.
            TimeTrait = TimeTrait.InstanceFrom(this);
		    MillisecondTimeDomain = MillisecondTimeDomain.MinMax;
        }

	    public TimeTrait TimeTrait { get; private set; }
	    public MillisecondTimeDomain MillisecondTimeDomain { get; private set; }

    }
}
