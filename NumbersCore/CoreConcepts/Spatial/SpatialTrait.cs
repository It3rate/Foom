namespace NumbersCore.CoreConcepts.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Counter;
    using NumbersCore.CoreConcepts.Tactile;
    using NumbersCore.CoreConcepts.Temperature;
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
}
