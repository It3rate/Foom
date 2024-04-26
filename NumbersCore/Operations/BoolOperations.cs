using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersCore.Operations;
public class BoolOperations
{
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
