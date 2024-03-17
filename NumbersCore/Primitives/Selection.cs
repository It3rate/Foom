using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
    /// <summary>
    /// Selection is a selected set of numbers. This can be manually set, or derived from an operation using an number (eg last two results in fibonacci sequence).
    /// </summary>
    public class Selection : IMathElement
    {
        public MathElementKind Kind => MathElementKind.Selection;
	    public int Id { get; }
	    public int CreationIndex => Id - (int)Kind - 1;
        private static int SelectionCounter = 1 + (int)MathElementKind.Selection;

        public bool IsDirty { get; set; } = true;
        public Number[] SelectedNumbers { get; }
        public int Count => SelectedNumbers.Length; 
        public Number this[int i] => SelectedNumbers[i];

        public Selection(params Number[] numbers)
        {
	        Id = SelectionCounter++;
	        SelectedNumbers = numbers;
        }
    }
}
