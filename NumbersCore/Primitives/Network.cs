using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
	public class Network
    {
	    //public MathElementKind Kind => MathElementKind.Network;
        public int Id { get; }
        public Brain Brain { get; }

        public Network(Brain brain)
        {
	        Brain = brain;
	       // Id = Brain.NextNetworkId();
        }
    }
}
