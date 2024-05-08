using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class PowerOperation : OperationBase
{
  public PowerOperation(Number left, Number right, OperationKind operationKind) : base(left, right, operationKind)
  {
  }

  public override void Calculate()
  {
    Result.Pow(Right); // todo: left should be equation here, allowing for sin(x)^3 etc
  }
}
