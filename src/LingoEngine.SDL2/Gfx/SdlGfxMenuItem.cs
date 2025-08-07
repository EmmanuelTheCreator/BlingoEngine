using System;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenuItem : ILingoFrameworkGfxMenuItem, IDisposable
    {
        private readonly nint _renderer;
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public bool CheckMark { get; set; }
        public string? Shortcut { get; set; }
        public event Action? Activated;
        public object FrameworkNode => this;

        public SdlGfxMenuItem(nint renderer, string name, string? shortcut)
        {
            _renderer = renderer;
            Name = name;
            Shortcut = shortcut;
        }

        public void Invoke() => Activated?.Invoke();
        public void Dispose() { }
    }
}
