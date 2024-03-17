using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
	/// <summary>
    /// Traits are measurable properties on objects. They can be composed of multiple domains (like mph or dollars/day or lwh) but do not need to be.
    /// </summary>
    public class Trait : IMathElement
	{
        public Brain MyBrain { get; internal set; }
        public MathElementKind Kind => MathElementKind.Trait;

        private static int _traitCounter = 1 + (int)MathElementKind.Trait;
        public int Id { get; internal set; }
        public int CreationIndex => Id - (int)Kind - 1;

        public bool IsDirty { get; set; } = true;
        public virtual string Name { get; private set; }

        public readonly Dictionary<int, long> PositionStore = new Dictionary<int, long>(4096);
        public readonly Dictionary<int, Focal> FocalStore = new Dictionary<int, Focal>();
        public readonly Dictionary<int, Domain> DomainStore = new Dictionary<int, Domain>();

        private Trait()
        {
        }
        protected Trait(string name) : this()
        {
            Id = _traitCounter++;
	        Name = name;
        }

        public static Trait CreateIn(Brain brain, string name) => brain.AddTrait(new Trait(name));
        public static Trait GetOrCreateTrait(Brain brain, string name)
        {
            return brain.GetOrCreateTrait(name);
        }

        public Domain AddDomain(Focal basis, Focal minMax, string name)
	    {
		    return new Domain(this, basis, minMax, name);
	    }
	    public Domain AddDomain(long basisTicks, string name)
	    {
		    return AddDomain(Focal.CreateZeroFocal(basisTicks), Focal.MinMaxFocal, name);
        }
        public void RemoveDomain(Domain domain)
        {
            if (DomainStore.ContainsKey(domain.Id))
            {
                DomainStore.Remove(domain.Id);
            }
        }
        public IEnumerable<Domain> Domains()
	    {
		    foreach (var domain in DomainStore.Values)
		    {
			    yield return domain;
		    }
	    }

        public Domain DomainAt(int index)
	    {
		    var id = index + (int)MathElementKind.Domain;
		    DomainStore.TryGetValue(id, out var result);
		    return result;
	    }
	    public Focal FocalAt(int index)
	    {
		    var id = index + (int)MathElementKind.Focal;
		    FocalStore.TryGetValue(id, out var result);
		    return result;
	    }
        protected Trait CopyPropertiesTo(Trait trait)
        {
            // don't clone the contents for now, need to work on multiple brains later.
            trait.Id = Id;
            trait.MyBrain = MyBrain;
            trait.Name = Name;
            return trait;
        }
        public virtual Trait Clone() => CopyPropertiesTo(new Trait());
	}
}
