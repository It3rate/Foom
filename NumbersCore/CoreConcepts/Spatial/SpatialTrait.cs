namespace NumbersCore.CoreConcepts.Spatial;
using NumbersCore.Primitives;

public class SpatialTrait : Trait
{
    private static readonly SpatialTrait _instance = new SpatialTrait();
    public static SpatialTrait Instance => (SpatialTrait)Brain.ActiveBrain.GetBrainsVersionOf(_instance);
    public static SpatialTrait InstanceFrom(Knowledge knowledge) => (SpatialTrait)knowledge.Brain.GetBrainsVersionOf(_instance);

    private SpatialTrait() : base("Spatial") { }

    public static SpatialTrait CreateIn(Knowledge knowledge) => (SpatialTrait)knowledge.Brain.AddTrait(new SpatialTrait());
    public override Trait Clone() => CopyPropertiesTo(new SpatialTrait());
}
