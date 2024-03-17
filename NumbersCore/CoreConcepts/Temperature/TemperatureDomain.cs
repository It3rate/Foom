namespace NumbersCore.CoreConcepts.Temperature
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Counter;
    using NumbersCore.Primitives;

    public class TemperatureDomain : Domain
    {
        public static TemperatureDomain GetCelsiusDomain() => CreateDomain(9, 22, 38, 0, "Celsius");
        public static TemperatureDomain GetFahrenheitDomain() => CreateDomain(5, 8, 100, -32 * 5, "Fahrenheit");

        public TemperatureDomain(Focal basisFocal, Focal maxFocal, string name) : base(TemperatureTrait.Instance, basisFocal, maxFocal, name)
        {
            IsVisible = false;
        }
        public static TemperatureDomain CreateDomain(int unitSize, int minRange, int maxRange, int zeroPoint, string name, bool isVisible = true)
        {
            var basis = new Focal(zeroPoint, zeroPoint + unitSize);
            var minMax = new Focal(-minRange * unitSize + zeroPoint, maxRange * unitSize + zeroPoint);
            var domain = new TemperatureDomain(basis, minMax, name);
            domain.Trait = TemperatureTrait.Instance;
            domain.IsVisible = isVisible;
            return domain;
        }
    }
}
