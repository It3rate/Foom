namespace NumbersCore.CoreConcepts.Spatial;
using NumbersCore.Primitives;

public class ExtentDomain : PolyDomain
{
    // Q. Can numbers combine as separate domains within a single domain like this, or does every dimension require it's own (related) domain?
    // Two numbers in one domain combine through operations creating a result. X and Y have domains so similar they feel the same, but probably aren't.
    // Need a PolyDomain, that abstractly combines multiple domains in certain ways, and then populate (eg with RGB, XYZ, mph...)
    // These can combine upwards to 'organism' level complexity? The domain determines the bounds of behavior, the organism navigates it.

    // ** polynumbers on the same domain are branches or alternate paths, like TYLOX (local dimensions). On separate domains are combinations of properties.

    // Q. is there a difference between two numbers on a line and a branch? (like result of a NAND)? Maybe that is how you define a branch. 'T' would be three numbers like: _=
    // the difference is a result needs to be a single directional ordered line, divided. A branch can be any two numbers.
    // But a branch can come from a result, and a branch can OP transform to a result.
    // The poly domains aren't specific to traits, but maybe can require matching traits (like X,Y must both be distance, but the combo works for e.g. gear ratios as well.)
    public SpatialDomain HorizontalDomain { get; }
    public SpatialDomain VerticalDomain { get; }
    public NumberGroup Horizontal { get; }
    public NumberGroup Vertical { get; }
    public ExtentDomain(int xSize, int ySize) : base(
            SpatialDomain.GetPixelDomain(xSize, "Horizontal"),
            SpatialDomain.GetPixelDomain(ySize, "Vertical"))
    {
        HorizontalDomain = (SpatialDomain)GetDomainByName("Horizontal");
        VerticalDomain = (SpatialDomain)GetDomainByName("Vertical");
        Horizontal = GetGroupByName("Horizontal");
        Vertical = GetGroupByName("Vertical");
    }
    public void AddExtent(Number horz, Number vert)
    {
        AddPositions(horz, vert);
    }
    public void AddExtent(Focal horz, Focal vert)
    {
        AddPosition(horz, vert);
    }
    public void AddExtent(long horzi, long horzr, long verti, long vertr)
    {
        AddPosition(horzi, horzr, verti, vertr);
    }
    public void AddIncrementalExtent(long horz, long vert)
    {
        AddIncrementalPosition(horz, vert);
    }
    public (Number, Number) ExtentAt(int index)
    {
        var result = NumbersAt(index);
        return (result[0], result[1]);
    }
    public (Focal, Focal) ExtentFocalsAt(int index)
    {
        var result = FocalsAt(index);
        return (result[0], result[1]);
    }

}
