using Numbers.Agent;
using Numbers.Mappers;
using NumbersCore.Primitives;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersSkia.Mappers;
public class SkMaskedNumberMapper : SKNumberMapper
{
    public MaskedNumber MaskedNumber => (MaskedNumber)MathElement;

    public SkMaskedNumberMapper(MouseAgent agent, MaskedNumber maskedNumber) : base(agent, maskedNumber)
    {
    }
    public override void DrawNumber(float offset)
    {
        EnsureSegment();
        var pts = new List<long>();
        var isSelected = IsSelected();
        var startState = MaskedNumber.StartState;
        _drawNumEndCap = false;
        _drawNumStartCap = true;
        var postions = MaskedNumber.Positions().ToArray();
        var index = 0;
        foreach (var pos in postions)
        {
            if (startState.IsTrue())
            {
                pts.Add(pos);
            }
            else
            {
                var focal = new Focal(pts[0], pos);
                //var val = DomainMapper.Domain.GetValueOf(focal, Polarity);
                var num = new Number(focal, Polarity);
                num.Domain = DomainMapper.Domain;
                _drawNumEndCap = index >= postions.Length - 1;
                DrawNumber(num, offset, isSelected);
                _drawNumStartCap = false;
                pts.Clear();
            }
            index++;
            startState = startState.Invert();
        }
    }
}
