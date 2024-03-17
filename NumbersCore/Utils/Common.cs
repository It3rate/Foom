namespace NumbersCore.Utils
{
    using System;
    using System.Collections.Generic;
    using NumbersCore.Primitives;

    public class Common
    {
        public static DomainScope DegreeScope()
        {
            var basis = new Focal(0, 1);
            var minMax = new Focal(0, 360);
            return new DomainScope(basis, minMax);
        }
        public static DomainScope ByteScope()
        {
            var basis = new Focal(0, 1);
            var minMax = new Focal(0, 0xFF);
            return new DomainScope(basis, minMax);
        }
        public static DomainScope CentScope()
        {
            var basis = new Focal(0, 1);
            var minMax = new Focal(0, 100);
            return new DomainScope(basis, minMax);
        }
        public static DomainScope TickScope(long ticksPerUnit, long min, long max)
        {
            var basis = new Focal(0, ticksPerUnit);
            var minMax = new Focal(min, max);
            return new DomainScope(basis, minMax);
        }
        public static DomainScope PixelScope(long min, long max)
        {
            var basis = new Focal(0, 1);
            var minMax = new Focal(min, max);
            return new DomainScope(basis, minMax);
        }
    }
}
