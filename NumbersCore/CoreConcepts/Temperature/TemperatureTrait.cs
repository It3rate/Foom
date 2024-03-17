namespace NumbersCore.CoreConcepts.Temperature
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Counter;
    using NumbersCore.CoreConcepts.Time;
    using NumbersCore.Primitives;

    public class TemperatureTrait : Trait
    {
        private static readonly TemperatureTrait _instance = new TemperatureTrait();
        public static TemperatureTrait Instance => (TemperatureTrait)Brain.ActiveBrain.GetBrainsVersionOf(_instance);
        public static TemperatureTrait InstanceFrom(Knowledge knowledge) => (TemperatureTrait)knowledge.Brain.GetBrainsVersionOf(_instance);


        private TemperatureTrait() : base("Temperature") { }
        public override Trait Clone() => CopyPropertiesTo(new TemperatureTrait());
    }
}
