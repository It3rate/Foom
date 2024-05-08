using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class CompareOperation : OperationBase
{

  public CompareOperation(Number left, Number right, OperationKind operationKind) : base(left, right, operationKind)
  {
  }

  public override void Calculate()
  {
    ComputeBoolCompare();
  }
  public void ComputeBoolCompare()
  {
    // todo: all operations need a 'direction', A:B, B:A, A:1/4:B
    // todo: calc with ranges or normalize domain as domains may differ
    var maxA = Left.Focal.MaxExtent;
    var minA = Left.Focal.MinExtent;
    var maxB = Right.Focal.Max;// startB > endB ? startB : endB;
    var minB = Right.Focal.Min;// startB < endB ? startB : endB;
    long resultStart = 0;
    long resultEnd = 0;
    Result.ClearInternalPositions();
    switch (OperationKind)
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
      Result.AddPosition(resultStart, resultEnd);
    }
  }
}
