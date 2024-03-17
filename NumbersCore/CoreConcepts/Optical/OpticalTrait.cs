namespace NumbersCore.CoreConcepts.Optical
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Spatial;
    using NumbersCore.CoreConcepts.Temperature;
    using NumbersCore.Primitives;

    public class OpticalTrait : Trait
    {

        private static readonly OpticalTrait _instance = new OpticalTrait();
        public static OpticalTrait Instance => (OpticalTrait)Brain.ActiveBrain.GetBrainsVersionOf(_instance);
        public static OpticalTrait InstanceFrom(Knowledge knowledge) => (OpticalTrait)knowledge.Brain.GetBrainsVersionOf(_instance);
        private OpticalTrait() : base("Optical") { }

        public static OpticalTrait CreateIn(Knowledge knowledge) => (OpticalTrait)knowledge.Brain.AddTrait(new OpticalTrait());
        public override Trait Clone() => CopyPropertiesTo(new OpticalTrait());

    }
}
