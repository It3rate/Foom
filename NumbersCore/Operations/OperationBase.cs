using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public abstract class OperationBase
{
	public abstract OperationKind OperationKind { get; }

	public Number Left { get; }
	public Number Right { get; }
	public Number Result{ get; protected set; }

	public OperationBase(Number left, Number right)
	{
		Left = left;
		Right = right; 
		Result = left.Clone();
	}

}
