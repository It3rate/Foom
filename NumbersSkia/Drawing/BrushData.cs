namespace Numbers.Utils;
public class BrushData
{
    public float BrushSize = 20f; // can't be property as used as ref parameter
    public float Feather = 0f;
    public byte Transparency = 255;
    public uint ColorValue = 0xFF0072B2;

    public BrushData()
    {
    }
    public BrushData(float brushSize, float feather, byte transparency, uint colorValue)
    {
        BrushSize = brushSize;
        Feather = feather;
        Transparency = transparency;
        ColorValue = colorValue;
    }
    public BrushData Clone()
    {
        return new BrushData(BrushSize, Feather, Transparency, ColorValue);
    }
}
