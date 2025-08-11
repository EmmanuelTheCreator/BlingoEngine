using LingoEngine.Bitmaps;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a button control.
    /// </summary>
    public class LingoGfxButton : LingoGfxNodeBase<ILingoFrameworkGfxButton>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }
        public ILingoImageTexture? IconTexture { get => _framework.IconTexture; set => _framework.IconTexture = value; }

        public event Action? Pressed
        {
            add { _framework.Pressed += value; }
            remove { _framework.Pressed -= value; }
        }
    }
}
