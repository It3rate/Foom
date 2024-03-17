using System.Collections.Generic;

namespace NumbersCore.Primitives
{
	public class Definition
	{
		public int Id { get; }
		public Brain Brain { get; }
        public List<Number> Numbers; // numbers belong to traits and domains
		public List<Transform> Relations;

		public Definition(Brain brain)
		{
			Brain = brain;
			Id = Brain.NextDefinitionId();
		}
    }
}
