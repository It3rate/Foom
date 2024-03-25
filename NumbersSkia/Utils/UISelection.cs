using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersCore.Utils;

namespace NumbersSkia.Utils;

public class UISelection
{
    public List<int> SelectedIds { get; } = new List<int>();
    public UISelectionKind SelectedKind { get; }
    public UISelection( UISelectionKind kind, params int[] selectedIds)
    {
        SelectedIds.AddRange(selectedIds);
        SelectedKind = kind;
    }
}
public enum UISelectionKind
{
    None,
    DomainMapper,
    NumberMapper,
    TransformMapper,
    JointMapper,
    PathMapper,
    ImageMapper,
    ShapeMapper,
}
