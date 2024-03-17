namespace Numbers.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
}
