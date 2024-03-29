namespace Numbers.Primitives;

using NumbersCore.Utils;
using System.Collections.Generic;

public class TextElement : IMathElement
{
    public MathElementKind Kind => MathElementKind.None;
    private static int _idCounter = 1;
    public int Id { get; }
    public int CreationIndex => Id - (int)Kind;
    public bool IsDirty { get; set; } = true;

    public List<string> Lines { get; set; } = new List<string>();
    // font, color etc
    public TextElement(params string[] lines)
    {
        Id = _idCounter++;
        Lines.AddRange(lines);
    }
}
