namespace NumbersCore.Utils
{
	public interface IMathElement
    {
	    MathElementKind Kind { get; }
	    int Id { get; }
	    int CreationIndex { get; }
        //string Name { get; }

        bool IsDirty { get; set; }
    }

    public enum MathElementKind
    {
	    None = 0,
        Focal           = 0x01000000,
        FocalChain      = 0x02000000,
        Number          = 0x03000000,
        NumberChain     = 0x04000000,
        PolyDomain      = 0x05000000,
	    Domain          = 0x06000000,
	    Transform       = 0x07000000,
        Formula         = 0x08000000,
        Trait           = 0x09000000,
        Definition      = 0x0A000000,
        Workspace       = 0x0B000000,
        Brain           = 0x0C000000,

        Selection  = 0x10000000,
    }
}  
