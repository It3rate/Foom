using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class PowerOperation : OperationBase
{
  public override OperationKind OperationKind => OperationKind.Power;

  public PowerOperation(Number left, Number right) : base(left, right)
  {
  }

  public override void Calculate()
  {
    Left.SetWith(LeftInput);
    Left.Pow(Right); // todo: left should be equation here, allowing for sin(x)^3 etc
  }
}
