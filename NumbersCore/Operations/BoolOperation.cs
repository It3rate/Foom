using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public class BoolOperation : OperationBase
{
  public BoolOperation(Number left, Number right, OperationKind operationKind) : base(left, right, operationKind)
  {
  }

  public override void Calculate()
  {
    ComputeBoolOp();
  }
  public void ComputeBoolOp()
  {
    // todo: all bool/compare ops need to use normalized basis', or for now ranges. 
    // really numbers should never be used in bool ops, eventually combine maskedNumber with Number and this goes away
    var (_, table) = SegmentedTable(Left.Domain, true, Left, Right);
    var (focals, polarities, dir) = ApplyOpToSegmentedTable(table, OperationKind);
    Result.SetWith(focals, polarities, !dir);
  }

  /// <summary>
  /// NumberGroups can have overlapping numbers, so this segmented version returns all partial ranges for each possible segment.
  /// Assumes aligned domains.
  /// </summary>
  public static (long[], List<Number[]>) SegmentedTable(Domain domain, bool allowOverlap, params Number[] numbers)
  {
    var result = new List<Number[]>();
    var internalNumberSets = new List<Number[]>();
    var sPositions = new SortedSet<long>();
    foreach (var number in numbers)
    {
      if (number.IsValid)
      {
        internalNumberSets.Add(number.InternalNumbers().ToArray());
        sPositions.UnionWith(number.GetPositions());
      }
      else
      {
        internalNumberSets.Add([]);
      }
    }
    var positions = sPositions.ToArray();

    for (int i = 1; i < positions.Length; i++)
    {
      var focal = new Focal(positions[i - 1], positions[i]);
      var matches = new List<Number>();
      foreach (var numSet in internalNumberSets)
      {
        if (numSet.Length == 0)
        {
          matches.Add(CreateSubsegment(domain, focal, Polarity.None));
        }
        else
        {
          foreach (var number in numSet)
          {
            var intersection = Focal.Intersection(number.Focal, focal);
            if (intersection != null)
            {
              matches.Add(CreateSubsegment(domain, intersection, number.Polarity));
            }
            else if (allowOverlap)
            {
              matches.Add(CreateSubsegment(domain, focal, Polarity.None)); // False is polarity none
            }
          }
        }
      }
      result.Add(matches.ToArray());
    }
    return (positions, result);
  }
  protected static Number CreateSubsegment(Domain domain, Focal focal, Polarity polarity = Polarity.None)
  {
    var result = new Number(focal.Clone(), polarity); // false
    result.Domain = domain;
    return result;
  }
  protected static (Focal[], Polarity[], bool) ApplyOpToSegmentedTable(List<Number[]> data, OperationKind operation)
  {
    var focals = new List<Focal>();
    var polarities = new List<Polarity>();
    var dirResult = false;
    Focal? lastFocal = null;
    Polarity lastPolarity = Polarity.Unknown;

    foreach (var seg in data)
    {
      if (seg.Length == 0)
      {
      }
      else
      {
        var first = seg[0];
        var opResult = false;
        var polResult = false;
        var hasUndefSeg = false;
        var segDir = false;
        for (int i = 0; i < seg.Length; i++)
        {
          var curNum = seg[i];
          if (!curNum.Polarity.HasPolarity())
          {
            hasUndefSeg = true; // undefined polarity is not considered for direction
          }
          var func = BoolOperation.GetFunc(operation);
          if (i == 0)
          {
            polResult = curNum.IsAligned;
            segDir = curNum.IsUnitPositivePointing;
            opResult = curNum.HasPolairty;
          }
          else
          {
            polResult = func(polResult, curNum.IsAligned); ;
            segDir = func(segDir, curNum.IsUnitPositivePointing);
            opResult = func(opResult, curNum.HasPolairty);
          }
        }
        if (!hasUndefSeg)
        {
          dirResult = segDir;
        }

        if (opResult)
        {
          var focal = new Focal(first.MinTickPosition, first.MaxTickPosition);// dirResult ? new Focal(first.MinTickPosition, first.MaxTickPosition) : new Focal(first.MaxTickPosition, first.MinTickPosition);
          var polarity = polResult ? Polarity.Aligned : Polarity.Inverted;
          if (lastFocal != null)// && lastPolarity == polarity && lastFocal.IsPositiveDirection == focal.IsPositiveDirection) // merge continuous segments
          {
            lastFocal.EndPosition = focal.EndPosition;
            //if (lastFocal.IsPositiveDirection)
            //{
            //    lastFocal.EndPosition = focal.EndPosition;
            //}
            //else
            //{
            //    lastFocal.StartPosition = focal.StartPosition;
            //}
          }
          else
          {
            focals.Add(focal);
            polarities.Add(polarity);
            lastFocal = focal;
            lastPolarity = polarity;
          }
        }
        else
        {
          lastFocal = null;
        }
      }
    }

    return (focals.ToArray(), polarities.ToArray(), dirResult);

  }

  // FALSE (output is always false)
  private static Func<bool, bool, bool> FALSE = (x, y) => false;

  // AND (true if both inputs are true)
  private static Func<bool, bool, bool> AND = (x, y) => x && y;

  // AND-NOT (true if the first input is true and the second is false)
  private static Func<bool, bool, bool> AND_NOT = (x, y) => x && !y;

  // FIRST INPUT (output is the first input)
  private static Func<bool, bool, bool> A = (x, y) => x;

  // NOT-AND (true if the first input is false and the second is true) equivalent to Select-and-Complement
  private static Func<bool, bool, bool> NOT_AND = (x, y) => !x && y;

  // SECOND INPUT (output is the second input)
  private static Func<bool, bool, bool> B = (x, y) => y;

  // XOR (true if inputs are different)
  private static Func<bool, bool, bool> XOR = (x, y) => x ^ y;

  // OR (true if at least one input is true)
  private static Func<bool, bool, bool> OR = (x, y) => x || y;

  // NOR (true if both inputs are false)
  private static Func<bool, bool, bool> NOR = (x, y) => !(x || y);

  // XNOR (true if inputs are the same)
  private static Func<bool, bool, bool> XNOR = (x, y) => !(x ^ y);

  // NOT SECOND INPUT (output is the negation of the second input)
  private static Func<bool, bool, bool> NOT_B = (x, y) => !y;

  // IF-THEN (true if the first input is false or both are true) equivalent to logical implication
  private static Func<bool, bool, bool> A_OR_NOT_B = (x, y) => x || !y;

  // NOT FIRST INPUT (output is the negation of the first input)
  private static Func<bool, bool, bool> NOT_A = (x, y) => !x;

  // THEN-IF (true if the second input is false or both are true) equivalent to converse implication
  private static Func<bool, bool, bool> NOT_A_OR_B = (x, y) => !x || y;

  // NAND (true if at least one input is false)
  private static Func<bool, bool, bool> NAND = (x, y) => !(x && y);

  // TRUE (output is always true)
  private static Func<bool, bool, bool> TRUE = (x, y) => true;
  public static Func<bool, bool, bool> GetFunc(OperationKind kind)
  {
    var result = A;
    switch (kind)
    {
      case OperationKind.FALSE:
        result = FALSE;
        break;

      case OperationKind.AND:
        result = AND;
        break;

      case OperationKind.AND_NOT:
        result = AND_NOT;
        break;

      case OperationKind.A:
        result = A;
        break;

      case OperationKind.NOT_AND:
        result = NOT_AND;
        break;

      case OperationKind.B:
        result = B;
        break;

      case OperationKind.XOR:
        result = XOR;
        break;

      case OperationKind.OR:
        result = OR;
        break;

      case OperationKind.NOR:
        result = NOR;
        break;

      case OperationKind.XNOR:
        result = XNOR;
        break;

      case OperationKind.NOT_B:
        result = NOT_B;
        break;

      case OperationKind.A_OR_NOT_B:
        result = A_OR_NOT_B;
        break;

      case OperationKind.NOT_A:
        result = NOT_A;
        break;

      case OperationKind.NOT_A_OR_B:
        result = NOT_A_OR_B;
        break;

      case OperationKind.NAND:
        result = NAND;
        break;

      case OperationKind.TRUE:
        result = TRUE;
        break;
    }
    return result;
  }

}
