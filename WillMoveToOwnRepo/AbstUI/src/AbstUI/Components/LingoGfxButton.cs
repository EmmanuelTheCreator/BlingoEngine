using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a button control.
    /// </summary>
    public class AbstUIGfxButton : AbstUIGfxNodeBase<IAbstUIFrameworkGfxButton>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }
        private IAbstUITextureUserSubscription? _iconTextureSubscription;
        public IAbstUITexture2D? IconTexture
        {
            get => _framework.IconTexture;
            set
            {
                _iconTextureSubscription?.Release();
                _iconTextureSubscription = value?.AddUser(this);
                _framework.IconTexture = value;
            }
        }

        public event Action? Pressed
        {
            add { _framework.Pressed += value; }
            remove { _framework.Pressed -= value; }
        }

        public override void Dispose()
        {
            _iconTextureSubscription?.Release();
            _iconTextureSubscription = null;
            _framework.IconTexture = null;
            base.Dispose();
        }
    }
}
