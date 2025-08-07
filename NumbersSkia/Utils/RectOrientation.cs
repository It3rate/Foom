using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersSkia.Utils;

[Flags]
public enum RectOrientation
{
    None = 0,
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8,

    BottomLeft = Bottom | Left,
    BottomRight = Bottom | Right,
    TopLeft = Top | Left,
    TopRight = Top | Right,
}
