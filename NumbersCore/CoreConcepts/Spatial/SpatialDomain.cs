namespace NumbersCore.CoreConcepts.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.CoreConcepts.Counter;
    using NumbersCore.CoreConcepts.Optical;
    using NumbersCore.CoreConcepts.Temperature;
    using NumbersCore.Primitives;
    using NumbersCore.Utils;

    public class SpatialDomain : Domain
    {
        //public static TemperatureDomain PixelDomain { get; } = CreateDomain(9, 22, 38, 0, "Celsius");
        public static SpatialDomain GetPixelDomain(int size, string name) => CreateDomain(Common.PixelScope(0, size), name, false);
        public static SpatialDomain Get2DCenteredDomain(int ticks, int size, string name) => CreateDomain(Common.TickScope(ticks, -size, size), name, false);

        public SpatialDomain(Focal basisFocal, Focal maxFocal, string name) : base(CounterTrait.Instance, basisFocal, maxFocal, name)
        {
            IsVisible = false;
        }
        // methods like getSpatialSize, center, bounds etc. Work in 1D, 2D, 3D, polar, bezier etc

        public static SpatialDomain CreateDomain(DomainScope df, string name, bool isVisible = true)
        {
            var domain = new SpatialDomain(df.Basis, df.MinMax, name);
            return domain;
        }

    }
}
