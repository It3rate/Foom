using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Optical;
    using NumbersCore.CoreConcepts.Temperature;

    public class CounterTrait : Trait
    {
        private static readonly CounterTrait _instance = new CounterTrait();
        public static CounterTrait Instance => (CounterTrait)Brain.ActiveBrain.GetBrainsVersionOf(_instance);
        public static CounterTrait InstanceFrom(Knowledge knowledge) => (CounterTrait)knowledge.Brain.GetBrainsVersionOf(_instance);

        private CounterTrait() : base("Counter") { }
        public override Trait Clone() => CopyPropertiesTo(new CounterTrait());

    }
}
