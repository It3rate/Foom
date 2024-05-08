using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class MultiplyOperation : OperationBase
{
	private bool _isCompounding;
  public MultiplyOperation(Number left, Number right, OperationKind operationKind) : base(left, right, operationKind)
  {
  }

	public override void Calculate()
	{
		switch (OperationKind)
    {
      case OperationKind.Multiply:
        Result.Multiply(Right);
        break;
      case OperationKind.Divide:
        Result.Divide(Right);
        break;
    }
	}
}
