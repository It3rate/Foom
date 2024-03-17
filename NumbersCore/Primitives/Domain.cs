
using NumbersCore.Utils;

namespace NumbersCore.Primitives
{
    // this constrains the significant figures of a measurement, unit/not is the minimum tick size, and range is the max possible.
    // Feels like this should just be the unit chosen - that works from min tick size, but how to specify a max value? On the trait I guess?
    // the trait then needs to 'know' how measurable it is, and the units are calibrated to that, which seems overspecified
    // (eg 'length' knows a nanometer is min and a light year max, cms and inches calibrate to this. Hmm, no).
    // So each 'situation' has sig-fig/precision metadata. Working in metal units vs working in wood units. A metal length trait and a wood length trait, convertible.
    // This is what domains are. BasisFocal size(s), min tick size, max ranges, confidence/probability data at limits of measure (gaussian precision etc).
    // E.g. changing the domain 'tolerance' could change neat writing into messy.

    // Min size is tick size. BasisFocal is start/end point (only one focal allowed for a unit). MinMaxFocal is bounds in ticks. todo: add conversion methods etc.
    public class Domain : IMathElement
    {
        public MathElementKind Kind => MathElementKind.Domain;
        private static int _idCounter = 1 + (int)MathElementKind.Domain;
        public int Id { get; internal set; }
        public int CreationIndex => Id - (int)Kind - 1;
        public bool IsDirty { get; set; } = true;

        public Trait Trait { get; protected set; }
        public Number BasisNumber { get; protected set; }
        public Focal BasisFocal => BasisNumber.Focal;
        public Number MinMaxNumber { get; protected set; }
        private Focal _minMaxFocal;
        public Focal MinMaxFocal
        {
            get => _minMaxFocal; 
            set
            {
                _minMaxFocal = value;
                MinMaxNumber = CreateNumber(_minMaxFocal);
            }
        }
        public string Name { get; protected set; }
        public bool IsVisible { get; set; } = true;

        public readonly Dictionary<int, Number> NumberStore = new Dictionary<int, Number>();
        public readonly Dictionary<int, NumberChain> NumberSetStore = new Dictionary<int, NumberChain>();

        // todo: need a tick size (defaults to 1), that can be overriden by numbers. This allows tick sizes larger than unit where
        // a unit of 1 mile and a tick size of 10 (miles) means you must round to the nearest 10 miles.
        public bool BasisIsReciprocal { get; set; }// True when ticks are larger than the unit basis

        //public int BasisNumberId => BasisNumber.Id;
        //public int BasisFocalId => BasisNumber.BasisFocal.Id;
        //public int MinMaxFocalId => MinMaxNumber.FocalId;
        //public int MinMaxNumberId => MinMaxNumber.Id;

        public PRange MinMaxRange => BasisFocal.RangeAsBasis(MinMaxFocal);
        public double TickToBasisRatio => BasisIsReciprocal ? BasisFocal.NonZeroLength : 1.0 / BasisFocal.NonZeroLength;

        public Domain(Trait trait, Focal basisFocal, Focal minMaxFocal, string name)
        {
            Id = _idCounter++;
            Trait = trait;
            MinMaxFocal = minMaxFocal == default ? Focal.MinMaxFocal : minMaxFocal;
            BasisNumber = CreateNumber(basisFocal);
            //BasisFocal = basisFocal;
            //MinMaxNumber = minMaxFocal == default ? CreateNumber(Focal.MinMaxFocal) : CreateNumber(minMaxFocal);
            Name = name;
            Trait.DomainStore.Add(Id, this);
        }
        public void ChangeDomainName(string newDomainName)
        {
            Name = newDomainName;
        }
        public void SetBasisWithNumber(Number nm)
        {
            BasisNumber = nm;
        }

        public static Domain CreateDomain(string traitName, long unitSize = 8, float rangeSize = 16, string name = "default") =>
            CreateDomain(traitName, unitSize, -rangeSize, rangeSize, 0, name);
        public static Domain CreateDomain(string traitName, long unitSize, float minRange, float maxRange, int zeroPoint, string name = "default", bool isVisible = true)
        {
            Trait trait = Trait.GetOrCreateTrait(Brain.ActiveBrain, traitName);
            var unit = new Focal(zeroPoint, zeroPoint + unitSize);
            var minMaxRange = new Focal((int)(minRange * unitSize + zeroPoint), (int)(maxRange * unitSize + zeroPoint));
            var domain = trait.AddDomain(unit, minMaxRange, name);
            domain.IsVisible = isVisible;
            return domain;
        }
        
        public int[] NumberIds() => NumberStore.Values.Select(num => num.Id).ToArray();
        public Number GetNumber(int numberId)
        {
            NumberStore.TryGetValue(numberId, out var result);
            return result;
        }
        public virtual Number CreateDefaultNumber(bool addToStore = true)
        {
            var num = new Number(new Focal(0, 1));
            return AddNumber(num, addToStore);
        }
        public Number CreateNumber(Focal focal, bool addToStore = true)
        {
            return AddNumber(new Number(focal), addToStore);
        }
        public Number CreateNumber(PRange range, bool addToStore = true)
        {
            var focal = CreateFocalFromRange(range);
            var result = AddNumber(new Number(focal), addToStore);
            result.Polarity = range.Polarity;
            return result;
        }
        public Number CreateNumber(long start, long end, bool addToStore = true)
        {
            var focal = new Focal(start, end);
            return AddNumber(new Number(focal), addToStore);
        }
        public Number CreateNumberFromFloats(float startF, float endF, bool addToStore = true)
        {
            long start = (long)(-startF * BasisFocal.LengthInTicks);
            long end = (long)(endF * BasisFocal.LengthInTicks);
            var focal = new Focal(start, end);
            return AddNumber(new Number(focal), addToStore);
        }
        public Number CreateInvertedNumberFromFloats(float startF, float endF, bool addToStore = true)
        {
            long start = (long)(-startF * BasisFocal.LengthInTicks);
            long end = (long)(endF * BasisFocal.LengthInTicks);
            var focal = new Focal(-start, -end);
            return AddNumber(new Number(focal, Polarity.Inverted), addToStore);
        }

        public int _nextStoreIndex = 0;
        public virtual Number AddNumber(Number number, bool addToStore = true)
        {
            number.Domain = this;
            number.Id = number.Id == 0 ? Number.NextNumberId() : number.Id;
            if (addToStore)
            {
                NumberStore[number.Id] = number;
                number.StoreIndex = _nextStoreIndex++;
            }
            return number;
        }
        public virtual bool RemoveNumber(Number number)
        {
            number.Domain = null;
            return NumberStore.Remove(number.Id);
        }


        private int _numberSetCounter = 1 + (int)MathElementKind.NumberChain;
        public int NextNumberSetId() => _numberSetCounter++ + Id;
        public NumberChain AddNumberSet(NumberChain numberSet, bool addToStore = true)
        {
            numberSet.Domain = this;
            numberSet.Id = numberSet.Id == 0 ? NextNumberSetId() : numberSet.Id;
            if (addToStore)
            {
                NumberSetStore[numberSet.Id] = numberSet;
            }
            return numberSet;
        }
        public bool RemoveNumberSet(NumberChain numberSet)
        {
            numberSet.Domain = null;
            return NumberSetStore.Remove(numberSet.Id);
        }

        public Number Zero() => CreateNumber(BasisFocal.StartPosition, BasisFocal.StartPosition);
        public Number One() => CreateNumber(BasisFocal.StartPosition, BasisFocal.EndPosition);

        public PRange GetValueOf(Focal focal, Polarity polarity) => focal.GetRangeWithBasis(BasisFocal, BasisIsReciprocal, polarity == Polarity.Aligned);
        public PRange GetValueOf(Number num) => num.Focal.GetRangeWithBasis(BasisFocal, BasisIsReciprocal, num.IsAligned);
        public void SetValueOf(Number num, PRange range)
        {
            num.Focal.SetWithRangeAndBasis(range, BasisFocal, BasisIsReciprocal);
            num.Polarity = range.Polarity;
        }

        public PRange ClampToInnerBasis(PRange range) => range.ClampInner();
        public PRange ClampToInnerTick(PRange range) => (range / TickToBasisRatio).ClampInner() * TickToBasisRatio;
        public PRange RoundToNearestBasis(PRange range) => range.Round();
        public PRange RoundToNearestTick(PRange range) => (range / TickToBasisRatio).Round() * TickToBasisRatio;

        public long RoundToNearestTick(long value) => (long)(Math.Round(value / TickToBasisRatio) * TickToBasisRatio);
        public void RoundToNearestTick(Focal focal)
        {
            focal.StartPosition = RoundToNearestTick(focal.StartPosition);
            focal.EndPosition = RoundToNearestTick(focal.EndPosition);
        }

        public Focal CreateFocalFromRange(PRange range)
        {
            var result = new Focal(0, 1);
            result.SetWithRangeAndBasis(range, BasisFocal, BasisIsReciprocal);
            return result;
        }

        public void ClearAll()
        {
            NumberStore.Clear();
        }

        public void AdjustFocalTickSizeBy(int ticks)
        {
            var ranges = new List<PRange>();
            foreach (var num in NumberStore.Values)
            {
                ranges.Add(num.Value);
            }
            BasisFocal.EndPosition += ticks;

            var index = 0;
            foreach (var num in NumberStore.Values)
            {
                num.Value = ranges[index++];
            }
        }

        public IEnumerable<Number> Numbers()
        {
            foreach (var number in NumberStore.Values)
            {
                yield return number;
            }
        }
        public void SaveNumberValues(Dictionary<int, PRange> dict, params int[] ignoreIds)
        {
            dict.Clear();
            foreach (var num in Numbers())
            {
                if (!ignoreIds.Contains(num.Id))
                {
                    dict.Add(num.Id, num.Value);
                }
            }
        }
        public void RestoreNumberValues(Dictionary<int, PRange> values, params int[] ignoreIds)
        {
            foreach (var kvp in values)
            {
                if (!ignoreIds.Contains(kvp.Key))
                {
                    NumberStore[kvp.Key].Value = kvp.Value;
                }
            }
        }

        public Domain Duplicate()
        {
            var result = new Domain(Trait, BasisFocal, MinMaxFocal, Name);
            result.IsVisible = IsVisible;
            return result;
        }
    }
}
