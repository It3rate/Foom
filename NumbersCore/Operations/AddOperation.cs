using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class AddOperation : OperationBase
{

  public AddOperation(Number left, Number right, OperationKind operationKind) : base(left, right, operationKind)
  {
  }

  public override void Calculate()
  {
    switch (OperationKind)
    {
      case OperationKind.Add:
        Result.Add(Right);
        break;
      case OperationKind.Subtract:
        Result.Subtract(Right);
        break;
    }
  }
}
