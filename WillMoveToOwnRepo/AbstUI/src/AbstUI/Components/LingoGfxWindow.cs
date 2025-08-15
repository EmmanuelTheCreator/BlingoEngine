using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a window element.
    /// </summary>
    public class AbstUIGfxWindow : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxWindow>, IAbstMouseRectProvider
    {
        private IAbstMouse _mouse = null!;
        private IAbstKey _key = null!;


        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public AColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public bool IsPopup { get => _framework.IsPopup; set => _framework.IsPopup = value; }
        public bool Borderless { get => _framework.Borderless; set => _framework.Borderless = value; }

        public IAbstMouse Mouse => _mouse;

        public IAbstKey Key => _key;

        public event Action<bool>? OnWindowStateChanged;
        public event Action<float, float>? OnResize;






        public void Init(IAbstUIFrameworkGfxWindow framework, IAbstMouse mouse, AbstKey key)
        {
            base.Init(framework);
            _mouse = mouse;
            _key = key;
        }


        public AbstUIGfxWindow AddItem(IAbstUIGfxNode node)
        {
            _framework.AddItem(node.Framework<IAbstUIFrameworkGfxLayoutNode>());
            return this;
        }

        public AbstUIGfxWindow AddItem(IAbstUIFrameworkGfxLayoutNode node)
        {
            _framework.AddItem(node);
            return this;
        }

        public void RemoveItem(IAbstUIGfxNode node) => _framework.RemoveItem(node.Framework<IAbstUIFrameworkGfxLayoutNode>());
        public void RemoveItem(IAbstUIFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstUIFrameworkGfxLayoutNode> GetItems() => _framework.GetItems();

        public void Popup() => _framework.Popup();

        public void PopupCentered() => _framework.PopupCentered();

        public void Hide() => _framework.Hide();

        ARect IAbstMouseRectProvider.MouseOffset => new ARect(X, Y, Width, Height);
        bool IAbstActivationProvider.IsActivated => Visibility;


        public void Resize(int width, int height) => OnResize?.Invoke(width, height);

        public void RaiseWindowStateChanged(bool state) => OnWindowStateChanged?.Invoke(state);
    }
}
