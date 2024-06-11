using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Shapes;

public interface IShape2D
{
  List<Number> Paths { get; }
  List<ShapeOutline> Outlines { get; }
  Shape2DBase Duplicate();
  void AddOffsetOutline(Number offset);
  void AddRingOutline(Number offsets);
  // ToPoints
  // ToPolyline
  // GetDrawingData
}
public class ShapeOutline
{
  IShape2D InputShape { get; set; }
  Number Offset { get; set; }
  bool BothDirections { get; set; } = false;
  Path2D Path { get; set; }
}
public abstract class Shape2DBase : IShape2D
{
  public List<Number> Paths => throw new NotImplementedException();

  public List<ShapeOutline> Outlines => throw new NotImplementedException();

  public abstract void AddOffsetOutline(Number offset);
  public abstract void AddRingOutline(Number offsets);
  public abstract Shape2DBase Duplicate();

  public static Path2D MergeShapesToOutline(List<Shape2DBase> shapes)
  {
    throw new NotImplementedException();
  }
}
