using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersCore.Primitives
{
    public enum FilterOperator
    {
        // Unary
        Null,
        Transfer,
        Not,
        Identity,

        // Binary
        Never,
        And,
        B_Inhibits_A,
        Transfer_A,
        A_Inhibits_B,
        Transfer_B,
        Xor,
        Or,
        Nor,
        Xnor,
        Not_B,
        B_Implies_A,
        Not_A,
        A_Implies_B,
        Nand,
        Always,

        // Ops
        Add,
        Multiply,

        // Comparisons
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
    }
}
