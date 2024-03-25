using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter;

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

    public long Step => Focal.EndPosition;
    public long Increment()
	    {
        // never increment up to max value, so a max value is never reached (meaning always continue if in repeat until cycle)
        if (Focal.EndPosition < long.MaxValue - 2)
        {
            Focal.EndPosition += 1;
        }
		    return Focal.EndPosition;
	    }

	    public void Reset()
	    {
		    Focal.EndPosition = 0;
	    }
}
