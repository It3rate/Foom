using Numbers.Agent;
using Numbers.Mappers;
using NumbersCore.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersSkia.Mappers;
public class SkMaskedNumberMapper : SKNumberMapper
{
    public MaskedNumber MaskedNumber => (MaskedNumber)MathElement;
    public SKDomainMapper DomainMapper => WorkspaceMapper.GetDomainMapper(MaskedNumber.Domain);

    public SkMaskedNumberMapper(MouseAgent agent, NumberGroup numberSet) : base(agent, numberSet)
    {
    }
}
