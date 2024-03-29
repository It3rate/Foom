namespace NumbersCore.CoreConcepts.Tactile;
using NumbersCore.Primitives;

public class TactileTrait : Trait
{
    private TactileTrait() : base("Tactile") { }

    public static TactileTrait CreateIn(Knowledge knowledge) => (TactileTrait)knowledge.Brain.AddTrait(new TactileTrait());
    public override Trait Clone() => CopyPropertiesTo(new TactileTrait());
}
