using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    // Q: Do typed numbers have to exist, as the domains are typed?
    public class UpCounter : Number
    {
	    public UpCounter() : base(new Focal(0, 0))
        {
            CounterDomain.UpCounterDomain.AddNumber(this, false);
        }

        public long Increment()
	    {
		    Focal.EndPosition += 1;
		    return Focal.EndPosition;
	    }

	    public void Reset()
	    {
		    Focal.EndPosition = 0;
	    }
    }
}
