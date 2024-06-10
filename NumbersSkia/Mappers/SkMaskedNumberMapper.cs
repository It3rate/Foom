using NumbersSkia.Agent;
using NumbersSkia.Mappers;
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
        RenderSegment = Guideline.ShiftOffLine(offset);

        var pts = new List<long>();
        var isSelected = IsSelected();
        var startState = MaskedNumber.StartState;
        _drawNumEndCap = false;
        _drawNumStartCap = true;
        var postions = MaskedNumber.Positions().ToArray();
        var index = 0;
        var isStarted = false;
        foreach (var pos in postions)
        {
            if (startState.IsTrue())
            {
                pts.Add(pos);
                isStarted = true;
            }
            else if(isStarted)
            {
                var focal = new Focal(pts[0], pos);
                //var pol = startState == BoolState.True ? Polarity.Aligned : Polarity.Inverted;
                var num = new Number(focal, Polarity);
                num.Domain = DomainMapper.Domain;
                _drawNumEndCap = index >= postions.Length - 1;
                DrawNumber(num, offset, isSelected);
                _drawNumStartCap = false;
                pts.Clear();
                //pts.Add(pos);
            }
            index++;
            startState = startState.Invert();
        }
    }
    public override string ToString()
    {
        return "mnm:" + Number.ToString();
    }
}
