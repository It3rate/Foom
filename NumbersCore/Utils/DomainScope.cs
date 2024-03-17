namespace NumbersCore.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NumbersCore.Primitives;

    public class DomainScope
    {
        public Focal Basis { get; set; }
        public Focal MinMax { get; set; }
        public DomainScope(Domain domain)
        {
            Basis = domain.BasisFocal;
            MinMax = domain.MinMaxFocal;
        }
        public DomainScope(Focal basisFocal, Focal minMaxFocal)
        {
            Basis = basisFocal;
            MinMax = minMaxFocal;
        }

    }
}
