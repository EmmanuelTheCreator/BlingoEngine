using LingoEngine.Inputs;
using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a window element.
    /// </summary>
    public class LingoGfxWindow : LingoGfxNodeLayoutBase<ILingoFrameworkGfxWindow>, ILingoMouseRectProvider
    {
        private LingoMouse _mouse = null!;
        private LingoKey _key = null!;


        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public LingoColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public bool IsPopup { get => _framework.IsPopup; set => _framework.IsPopup = value; }
        public bool Borderless { get => _framework.Borderless; set => _framework.Borderless = value; }

        public LingoMouse Mouse => _mouse;

        public LingoKey Key => _key;

        public event Action<bool>? OnWindowStateChanged;
        public event Action<float, float>? OnResize;




      

        public void Init(ILingoFrameworkGfxWindow framework, LingoMouse mouse, LingoKey key)
        {
            base.Init(framework);
            _mouse = mouse;
            _key = key;
        }


        public LingoGfxWindow AddItem(ILingoGfxNode node)
        {
            _framework.AddItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
            return this;
        }

        public LingoGfxWindow AddItem(ILingoFrameworkGfxLayoutNode node)
        {
            _framework.AddItem(node);
            return this;
        }

        public void RemoveItem(ILingoGfxNode node) => _framework.RemoveItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void RemoveItem(ILingoFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _framework.GetItems();

        public void Popup() => _framework.Popup();

        public void PopupCentered() => _framework.PopupCentered();

        public void Hide() => _framework.Hide();

        LingoRect ILingoMouseRectProvider.MouseOffset => new LingoRect(X, Y, Width, Height);
        bool ILingoActivationProvider.IsActivated => Visibility;


        public void Resize(int width, int height) => OnResize?.Invoke(width, height);

        public void RaiseWindowStateChanged(bool state) => OnWindowStateChanged?.Invoke(state);
    }
}
