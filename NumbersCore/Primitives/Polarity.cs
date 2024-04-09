using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersCore.Primitives;

public enum Polarity { None, Unknown, Aligned, Inverted };//, Zero, Max }

public static class PolarityExtension
{
    public static bool HasPolarity(this Polarity polarity) => polarity == Polarity.Aligned || polarity == Polarity.Inverted;
    public static bool IsTrue(this Polarity polarity) => polarity == Polarity.Aligned;
}