using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersCore.Utils;
/// <summary>
/// Ratio is used to allow numbers and their resolution to be portable during normalization and calculation.
/// It also represents the coorelation between left and right in an operation, or the ordered/random ratio in a property.
/// Will eventually replace PRange.
/// </summary>
public struct Ratio
{
    public long Start { get; set; }
    public long End { get; set; }
    public long Resolution { get; set; }

    public long Length => End - Start;

    public double StartValue => Start / (double)Resolution;
	public double EndValue => End / (double)Resolution;
	public double LengthValue => Length / (double)Resolution;

	public static (Ratio, Ratio) Normalize(Ratio left, Ratio right)
    {
        throw new NotImplementedException();
    }
}
