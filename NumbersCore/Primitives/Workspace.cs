using NumbersCore.CoreConcepts.Time;
using NumbersCore.Utils;

namespace NumbersCore.Primitives;

public class Workspace : IMathElement
{
    private static int _idCounter = 0;
    public MathElementKind Kind => MathElementKind.Workspace;
    public int Id { get; }
    public int CreationIndex => Id - (int)Kind;

    public Brain Brain { get; }

    public bool IsDirty { get; set; } = true;
    private HashSet<int> ActiveIds { get; } = new HashSet<int>();
    public int ActiveElementCount => ActiveIds.Count;

    public bool IsActive { get; set; } = true;

    public Workspace(Brain brain)
    {
        Brain = brain;
        brain.EnsureKnowledge();
        Id = _idCounter++;
        Brain.Workspaces.Add(this);
    }

    public void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
        //var num = lastDomain?.Numbers().ElementAt(2);
        //if (num != null)
        //{
        // num.Focal.EndTickPosition = num.Focal.EndTickPosition + test;
        //       if(Math.Abs(num.Focal.EndTickPosition) > 70){test = -test;}
        //}
    }

    public bool IsElementActive(int id) => ActiveIds.Contains(id);
    public bool IsElementActive(IMathElement element) => ActiveIds.Contains(element.Id);

    public void AddElements(params IMathElement[] elements)
    {
        foreach (var element in elements)
        {
            ActiveIds.Add(element.Id);
        }
    }
    public void RemoveElements(params IMathElement[] elements)
    {
        foreach (var element in elements)
        {
            ActiveIds.Remove(element.Id);
        }
    }
    public void AddElementsById(params int[] elementIds)
    {
        foreach (var id in elementIds)
        {

            ActiveIds.Add(id);
        }
    }
    public void RemoveElementsById(params int[] elementIds)
    {
        foreach (var id in elementIds)
        {
            ActiveIds.Remove(id);
        }
    }

    public void AddTraits(bool includeChildren, params Trait[] traits)
    {
        foreach (var trait in traits)
        {
            AddElementsById(trait.Id);
            if (includeChildren)
            {
                AddDomains(includeChildren, trait.DomainStore.Values.ToArray());
            }
        }
    }
    public void RemoveTraits(bool includeChildren, params Trait[] traits)
    {
        foreach (var trait in traits)
        {
            RemoveElementsById(trait.Id);
            if (includeChildren)
            {
                RemoveDomains(includeChildren, trait.DomainStore.Values.ToArray());
            }
        }
    }

    private Domain lastDomain;
    public void AddDomains(bool includeChildren, params Domain[] domains)
    {
        foreach (var domain in domains)
        {
            lastDomain = domain;
            AddElementsById(domain.Id);
            if (includeChildren)
            {
                AddElementsById(domain.NumberIds());
            }
        }
    }
    public void RemoveDomains(bool includeChildren, params Domain[] domains)
    {
        foreach (var domain in domains)
        {
            RemoveElementsById(domain.Id);
            if (includeChildren)
            {
                RemoveElementsById(domain.NumberIds());
            }
        }
    }

    public IEnumerable<Domain> ActiveSiblingDomains(Domain domain)
    {
        var ds = domain.Trait.DomainStore;
        var domainIds = ds.Keys.Where(key => ActiveIds.Contains(key));
        foreach (var id in domainIds)
        {
            if (id != domain.Id)
            {
                yield return ds[id];
            }
        }
    }

    public IEnumerable<Domain> DomainsWithSharedBasis(Domain domain)
    {
        var ds = domain.Trait.DomainStore;
        var basisId = domain.BasisFocal.Id;
        var domainIds = ds.Keys.Where(key => ActiveIds.Contains(key) && ds[key] is Domain dm && dm.BasisFocal.Id == basisId);
        foreach (var id in domainIds)
        {
            yield return ds[id];
        }
    }
    public IEnumerable<Number> NumbersWithSharedBasis(Domain domain)
    {
        var ds = domain.Trait.DomainStore;
        var basisId = domain.BasisFocal.Id;
        var domainIds = ds.Keys.Where(key => ActiveIds.Contains(key) && ds[key] is Domain dm && dm.BasisFocal.Id == basisId);
        foreach (var id in domainIds)
        {
            var d = ds[id];
            foreach (var num in d.Numbers())
            {
                yield return num;
            }
        }
    }
    public void AdjustFocalTickSizeBy(Domain domain, int ticks)
    {
        var ranges = new List<PRange>();
        foreach (var num in NumbersWithSharedBasis(domain))
        {
            ranges.Add(num.Value);
        }

        ticks = domain.BasisIsReciprocal ? -ticks : ticks;
        if (domain.BasisFocal.LengthInTicks + ticks <= 0)
        {
            ticks = -ticks;
            domain.BasisIsReciprocal = !domain.BasisIsReciprocal;
        }

        domain.BasisFocal.EndPosition += ticks;

        var index = 0;
        foreach (var num in NumbersWithSharedBasis(domain))
        {
            num.Value = ranges[index++];
        }
    }
    public int AdjustMinMaxBy(Domain domain, int units)
    {
        var result = 0;
        var mmVal = domain.MinMaxNumber.Value;
        if ((mmVal.Start + units >= 1) && (mmVal.End + units >= 1))
        {
            mmVal += new PRange(units, units);
            domain.MinMaxNumber.Value = mmVal;
            result = units;
        }
        return result;
    }
    public Transform TransformInvolving(Number num)
    {
        Transform result = null;
        foreach (var tm in Brain.Transforms())
        {
            if (tm.Involves(num))
            {
                result = tm;
                break;
            }
        }
        return result;
    }
    public void ClearAll()
    {
        ActiveIds.Clear();
    }

}
