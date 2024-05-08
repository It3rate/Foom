using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersCore.Operations;
public abstract class OperationBase
{
  // operations have a transform flow that is measured in a time unit of 1. Other time units are powers of this transform basis.
  // There is an equality of all operations that are the same ops independent of parameterization, as they have the same base graph over time.

  // an operation is a type of Join, this may need a formal class. A join can merge or connect two numbers.
  // Merge would be like in multiplication, where two numbers form a single acceleration.
  // Or an area, where one value is forced larger by another.
  // Connect would be like a polyline joint, where a new direction and force send the line in a new direction.
  // Focus on defining primitive drawing elements and extrapolate these to object connections and interactions.
  // These channel calculus. Mirror, Scale, Move, Rotate -> negate, mult, add, merge mult. *i is change perspective?
  // it may be useful to have all angles defined relative to some horizontal/vertical as these matter, and can be lost in a relative polyline.
  // coordinates could likewise be absolute in some frame of reference, at least in low resolution.

  // merges are parallel per step (curves), transitions are sequential step groups (joints).
  // parallel means any actions that run on the same step. You do not need a future step's info, but you still may not be able to skip steps.
  // Q. do these need to be separate classes?
  // an equation is made of a tree of merges, transitions, conditionals and random, the powers specify the time scales (repeats).
  // rates can be compounding or from a reference. Sin/cos is additive compounding (the change amount is added to each time)
  // They should have things like 'to the power of while false..', like with curve fitting to a given resolution.


  public OperationKind OperationKind { get; }

  public Number Left { get; }
  public Number Right { get; }

  public MaskedNumber Result { get; }

  // Note, Power is a separate op, allowing for powers of compound equations.
  // this is the power, which is the time domain (x^2 is stretching at a rate of x for 2 time units)
  //protected Number _t = MillisecondNumber.Create(0, 1000);// MillisecondNumber.Zero();
  //protected Number T
  //{
  //	get => _t; 
  //	set
  //	{
  //		_t = value;
  //		Calculate();
  //	}
  //}

  public OperationBase(Number left, Number right, OperationKind operationKind)
  {
    Left = left;
    //Left = new MaskedNumber(left);
    Right = right;
    OperationKind = operationKind;

    Result = new MaskedNumber(Left);
  }
  public abstract void Calculate();


  //public abstract Number Derivative();
  public void ApplyStart()
  {
    Result.SetWith(Left);
    if (Result.Domain == null)
    {
      Left.Domain.AddNumber(Result);
    }
  }
  public void ApplyPartial(long tickOffset) {  }
  public void ApplyEnd()
  {
    Calculate();
  }

  public static OperationBase? CreateOperation(Number left, Number right, OperationKind operationKind)
  {
    OperationBase result = null;
    if (operationKind.IsBoolOp())
    {
      result = new BoolOperation(left, right, operationKind);
    }
    else if (operationKind.IsBoolCompare())
    {
      result = new CompareOperation(left, right, operationKind);
    }
    else
    {
      switch (operationKind)
      {
        case OperationKind.None:
          break;
        case OperationKind.Add:
        case OperationKind.Subtract:
          result = new AddOperation(left, right, operationKind);
          break;
        case OperationKind.Multiply:
        case OperationKind.Divide:
          result = new MultiplyOperation(left, right, operationKind);
          break;
        case OperationKind.Power:
          break;
        case OperationKind.Root:
          break;
        case OperationKind.Negate:
          //Negate();
          break;
        case OperationKind.Reciprocal:
          break;
        case OperationKind.FlipPolarityInPlace: // switch polarity only, arrow same
          break;
        case OperationKind.FlipPolarity:
          break;
        case OperationKind.MirrorOnUnit:
          break;
        case OperationKind.MirrorOnUnot:
          break;
        case OperationKind.MirrorOnStart:
          break;
        case OperationKind.MirrorOnEnd:
          break;
        case OperationKind.FilterUnit:
          break;
        case OperationKind.FilterUnot:
          break;
        case OperationKind.FilterStart:
          break;
        case OperationKind.FilterEnd:
          break;
        case OperationKind.NegateInPlace:
          //_focalGroup.ComputeWith(Focal, operationKind); // change arrow dir
          break;
        case OperationKind.Wedge:
          break;
        case OperationKind.DotProduct:
          break;
        case OperationKind.GeometricProduct:
          break;
        case OperationKind.Blend:
          break;
      }
    }
    return result;
  }

}
