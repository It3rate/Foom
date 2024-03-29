namespace Numbers.Mappers;
using Numbers.Agent;
using Numbers.Renderer;
using SkiaSharp;

public interface IDrawableElement
{
    int Id { get; set; }
    MouseAgent Agent { get; }
    SKWorkspaceMapper WorkspaceMapper { get; }
    CoreRenderer Renderer { get; }
    SKCanvas Canvas { get; }
    void Draw();
}
