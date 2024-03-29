namespace NumbersCore.CoreConcepts.Optical;
using NumbersCore.Primitives;

public class HSLDomain : PolyDomain
{
    public OpticalDomain HueDomain;
    public OpticalDomain SaturationDomain;
    public OpticalDomain LightnessDomain;

    public NumberGroup Hue;
    public NumberGroup Saturation;
    public NumberGroup Lightness;
    public HSLDomain() : base(OpticalDomain.GetHueDomain(), OpticalDomain.GetSaturationDomain(), OpticalDomain.GetLightnessDomain())
    {
        HueDomain = (OpticalDomain)Domains[0];
        SaturationDomain = (OpticalDomain)Domains[1];
        LightnessDomain = (OpticalDomain)Domains[2];

        Hue = _numberGroup[0];
        Saturation = _numberGroup[1];
        Lightness = _numberGroup[2];
    }
    public void AddHsl(Number h, Number s, Number l)
    {
        AddPositions(h, s, l);
    }
    public void AddHsl(Focal h, Focal s, Focal l)
    {
        AddPosition(h, s, l);
    }
    public void AddHsl(long hi, long hr, long si, long sr, long li, long lr)
    {
        AddPosition(hi, hr, si, sr, li, lr);
    }
    public void AddIncrementalHsl(long x, long y)
    {
        AddIncrementalPosition(x, y);
    }
    public (Number, Number) HslAt(int index)
    {
        var result = NumbersAt(index);
        return (result[0], result[1]);
    }
    public (Focal, Focal) HslFocalsAt(int index)
    {
        var result = FocalsAt(index);
        return (result[0], result[1]);
    }
}
