using NumbersCore.Primitives;

namespace NumbersCore.CoreConcepts.Counter;
public class CounterTrait : Trait
{
    private static readonly CounterTrait _instance = new CounterTrait();
    public static CounterTrait Instance => (CounterTrait)Brain.ActiveBrain.GetBrainsVersionOf(_instance);
    public static CounterTrait InstanceFrom(Knowledge knowledge) => (CounterTrait)knowledge.Brain.GetBrainsVersionOf(_instance);

    private CounterTrait() : base("Counter") { }
    public override Trait Clone() => CopyPropertiesTo(new CounterTrait());

}
