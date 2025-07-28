using System;
using LingoEngine.Primitives;
using LingoEngine.Shapes;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Shapes
{
    public class SdlMemberShape : ILingoFrameworkMemberShape, IDisposable
    {
        public bool IsLoaded { get; private set; }
        public LingoList<LingoPoint> VertexList { get; } = new();
        public LingoShapeType ShapeType { get; set; } = LingoShapeType.Rectangle;
        public LingoColor FillColor { get; set; } = LingoColor.FromRGB(255, 255, 255);
        public LingoColor EndColor { get; set; } = LingoColor.FromRGB(255, 255, 255);
        public LingoColor StrokeColor { get; set; } = LingoColor.FromRGB(0, 0, 0);
        public int StrokeWidth { get; set; } = 1;
        public bool Closed { get; set; } = true;
        public bool AntiAlias { get; set; } = true;
        public float Width {get;set;}
        public float Height {get;set;}
        public bool Filled {get;set;}

        public void CopyToClipboard() { }
        public void Erase() { VertexList.Clear(); }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void Preload() { IsLoaded = true; }
        public void Unload() { IsLoaded = false; }
        public void Dispose() { }

        public void ReleaseFromSprite(LingoSprite lingoSprite) { }
    }
}
