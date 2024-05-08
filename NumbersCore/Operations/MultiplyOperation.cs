using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class MultiplyOperation : OperationBase
{
	public override OperationKind OperationKind => OperationKind.Multiply;

	public MultiplyOperation(Number left, Number right) : base(left, right)
	{
	}

	public override void Calculate()
	{
		Left.SetWith(LeftInput);
		Left.Multiply(Right);
	}
}
