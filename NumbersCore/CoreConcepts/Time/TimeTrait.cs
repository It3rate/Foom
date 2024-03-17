using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Time
{
	public class TimeTrait : Trait
    {
        private static readonly TimeTrait _instance = new TimeTrait();
        public static TimeTrait Instance => (TimeTrait)(Brain.ActiveBrain.GetBrainsVersionOf(_instance));
        public static TimeTrait InstanceFrom(Knowledge knowledge) => (TimeTrait)knowledge.Brain.GetBrainsVersionOf(_instance);

        private TimeTrait() : base("Time") { }

        public override Trait Clone() => CopyPropertiesTo(new TimeTrait());


    }
}
