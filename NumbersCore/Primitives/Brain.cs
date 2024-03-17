using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NumbersCore.CoreConcepts;
using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
    // Brain -> Traits, transforms

    public class Brain
    {
	    public static List<Brain> Brains { get; } = new List<Brain>();
	    public static Brain ActiveBrain => BrainA;
	    public static Brain BrainA = new Brain();
	    //public static Brain BrainB = new Brain();
	    public Brain()
	    {
		    Brains.Add(this);
	    }

        private Knowledge _knowledge;
        public Knowledge Knowledge => _knowledge;
        public void EnsureKnowledge()
        {
            if (_knowledge == null)
            {
                _knowledge = new Knowledge(this);
            }
        }

        public List<Workspace> Workspaces { get; } = new List<Workspace>();

        public readonly Dictionary<int, Network> NetworkStore = new Dictionary<int, Network>(); 
        public readonly Dictionary<int, Formula> FormulaStore = new Dictionary<int, Formula>();
        public readonly Dictionary<int, Definition> DefinitionStore = new Dictionary<int, Definition>();
        public readonly Dictionary<int, Trait> TraitStore = new Dictionary<int, Trait>();
	    public readonly Dictionary<int, Transform> TransformStore = new Dictionary<int, Transform>();

        private int _brainCounter = 1 + (int)MathElementKind.Brain;
	    private int _formulaCounter = 1 + (int)MathElementKind.Formula;
	    private int _definitionCounter = 1 + (int)MathElementKind.Definition;
	    private int _transformCounter = 1 + (int)MathElementKind.Transform;
        public int NextNetworkId() => _brainCounter++;
        public int NextFormulaId() => _formulaCounter++;
        public int NextDefinitionId() => _definitionCounter++;
	    public int NextTransformId() => _transformCounter++;

        public Trait GetLastTrait() => (_lastTrait == null) ? GetOrCreateTrait("trait") : _lastTrait;
        private Trait _lastTrait = null;
        public Trait GetOrCreateTrait(string traitName)
        {
            Trait trait = null;
            foreach(var t in TraitStore.Values)
            {
                if(t.Name == traitName)
                {
                    trait = t;
                    break;
                }
            }
            if(trait == null)
            {
                trait = Trait.CreateIn(this, traitName);
            }
            _lastTrait = trait;
            return trait;
        }
        public Trait GetBrainsVersionOf(Trait trait)
        {
            if (!TraitStore.ContainsKey(trait.Id))
            {
                // the Id doesn't change on clone, will be constant for given trait name.
                AddTrait(trait.Clone());
            }
            return TraitStore[trait.Id];
        }
        public Trait AddTrait(Trait trait)
	    {
		    trait.MyBrain = this;
            if (!TraitStore.ContainsKey(trait.Id))
            {
                TraitStore.Add(trait.Id, trait);
            }
		    return trait;
	    }
	    public bool RemoveTrait(Trait trait)
	    {
		    trait.MyBrain = null;
		    return TraitStore.Remove(trait.Id);
	    }
        public Transform AddTransform(Number left, Number repeats, OperationKind kind)
	    {
		    var result = new Transform(left, repeats, kind);
		    TransformStore.Add(result.Id, result);
		    return result;
	    }
	    public IEnumerable<Transform> Transforms()
	    {
		    foreach (var transform in TransformStore.Values)
		    {
			    yield return transform;
		    }
	    }

        public void ClearAll()
	    {
		    foreach (var workspace in Workspaces)
            {
                workspace.ClearAll();
            }
            Workspaces.Clear();


            NetworkStore.Clear();
            FormulaStore.Clear();
            TraitStore.Clear();
            TransformStore.Clear();
        }

        public Trait TraitAt(int index)
        {
	        var id = index + (int)MathElementKind.Trait;
	        TraitStore.TryGetValue(id, out var result);
	        return result;
        }
    }
}
