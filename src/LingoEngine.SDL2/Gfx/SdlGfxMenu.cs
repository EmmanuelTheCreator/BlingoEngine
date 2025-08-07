using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenu : ILingoFrameworkGfxMenu, IDisposable
    {
        private readonly nint _renderer;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public string Name { get; set; }
        public object FrameworkNode => this;

        public SdlGfxMenu(nint renderer, string name)
        {
            _renderer = renderer;
            Name = name;
        }

        public void AddItem(ILingoFrameworkGfxMenuItem item) { }
        public void ClearItems() { }
        public void PositionPopup(ILingoFrameworkGfxButton button) { }
        public void Popup() { }
        public void Dispose() { }
    }
}
