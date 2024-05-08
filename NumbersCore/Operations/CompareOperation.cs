using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class CompareOperation : OperationBase
{
  private OperationKind _operationKind;
  public override OperationKind OperationKind => _operationKind;

  public CompareOperation(Number left, Number right, OperationKind operationKind) : base(left, right)
  {
    _operationKind = operationKind;
  }

  public override void Calculate()
  {
    Left.SetWith(LeftInput);
    Left.ComputeBoolOp(Right, OperationKind);
  }
  public static void ComputeBoolCompare(Number left, Number right, OperationKind operationKind)
  {
    // todo: all operations need a 'direction', A:B, B:A, A:1/4:B
    // todo: calc with ranges or normalize domain as domains may differ
    var maxA = left.Focal.MaxExtent;
    var minA = left.Focal.MinExtent;
    var maxB = right.Focal.Max;// startB > endB ? startB : endB;
    var minB = right.Focal.Min;// startB < endB ? startB : endB;
    long resultStart = 0;
    long resultEnd = 0;
    left.ClearInternalPositions();
    switch (operationKind)
    {
      case OperationKind.GreaterThan: // A all to right of B
        if (minA > maxB) { resultStart = minA; resultEnd = maxA; } else if (maxA > maxB) { resultStart = Math.Max(minA, maxB); resultEnd = maxA; }
        break;
      case OperationKind.GreaterThanOrEqual: // no part of A to left of B
        if (minA >= minB) { resultStart = minA; resultEnd = maxA; } else if (maxA >= minB) { resultStart = Math.Max(minA, minB); resultEnd = maxA; }
        break;
      case OperationKind.GreaterThanAndEqual: // no part of A to left of B, and part to the right of B (A overlap BMax)
        if (minA < maxB && minA > minB && maxA > maxB) { resultStart = minA; resultEnd = maxA; }
        else if (minA <= minB && maxA > maxB) { resultStart = Math.Max(minA, minB); resultEnd = maxA; }
        break;
      case OperationKind.ContainedBy: // A fits inside B
        if (minA >= minB && maxA <= maxB) { resultStart = minA; resultEnd = maxA; }
        else if ((minA < minB && maxA > minB) || (maxA > maxB && minA < maxB)) { resultStart = Math.Max(minA, minB); resultEnd = Math.Min(maxA, maxB); }
        break;
      case OperationKind.Equals: // B matches A
        if (minA == minB && maxA == maxB) { resultStart = minA; resultEnd = maxA; }
        break;
      case OperationKind.Contains: // B fits inside A
        if (minB >= minA && maxB <= maxA) { resultStart = minB; resultEnd = maxB; }
        else if ((minB < minA && maxB > minA) || (maxB > maxA && minB < maxA)) { resultStart = Math.Max(minB, minA); resultEnd = Math.Min(maxB, maxA); }
        break;
      case OperationKind.LessThanAndEqual: // no part of A to right of B, and part to the left of B  (overlap left)
        if (minA < minB && maxA > minB && maxA < maxB) { resultStart = minA; resultEnd = maxA; }
        else if (minA <= minB && maxA > minB) { resultStart = minA; resultEnd = Math.Min(maxA, maxB); }
        break;
      case OperationKind.LessThanOrEqual: // no part of A to right of B
        if (maxA <= maxB) { resultStart = minA; resultEnd = maxA; } else if (minA <= maxB) { resultStart = minA; resultEnd = Math.Min(maxA, maxB); }
        break;
      case OperationKind.LessThan: // A all to left of B
        if (maxA < minB) { resultStart = minA; resultEnd = maxA; } else if (minA < minB) { resultStart = minA; resultEnd = Math.Min(maxA, minB); }
        break;
    }

    if (resultStart - resultEnd != 0) // zero length result not allowed, so this works
    {
      //positions.Add(resultStart);
      //positions.Add(resultStart);
      left.AddPosition(resultStart, resultEnd);
    }
  }
}
