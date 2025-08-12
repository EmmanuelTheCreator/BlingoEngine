using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a window element.
    /// </summary>
    public class LingoGfxWindow : LingoGfxNodeLayoutBase<ILingoFrameworkGfxWindow>, ILingoMouseRectProvider
    {
        private LingoMouse _mouse = null!;
        private LingoKey _key = null!;
        private ILingoMouseSubscription? _mouseDownSub;
        private ILingoMouseSubscription? _mouseUpSub;
        private ILingoMouseSubscription? _mouseMoveSub;
        private readonly KeyHandler _keyHandler;

        private event Action? _onOpen;
        private event Action? _onClose;

        public LingoGfxWindow() => _keyHandler = new KeyHandler(this);

        public void Init(ILingoFrameworkGfxWindow framework, LingoMouse mouse, LingoKey key)
        {
            base.Init(framework);
            _mouse = mouse.CreateNewInstance(this);
            _key = key.CreateNewInstance(this);

            _mouseDownSub = _mouse.OnMouseDown(e => OnMouseDown?.Invoke(e));
            _mouseUpSub = _mouse.OnMouseUp(e => OnMouseUp?.Invoke(e));
            _mouseMoveSub = _mouse.OnMouseMove(e => OnMouseMove?.Invoke(e));
            _key.Subscribe(_keyHandler);

            _framework.OnOpen += () => _onOpen?.Invoke();
            _framework.OnClose += () => _onClose?.Invoke();
        }

        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public LingoColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public bool IsPopup { get => _framework.IsPopup; set => _framework.IsPopup = value; }
        public bool Borderless { get => _framework.Borderless; set => _framework.Borderless = value; }

        public LingoMouse Mouse => _mouse;

        public LingoKey Key => _key;

        public event Action? OnOpen
        {
            add { _onOpen += value; }
            remove { _onOpen -= value; }
        }

        public event Action? OnClose
        {
            add { _onClose += value; }
            remove { _onClose -= value; }
        }

        public event Action<float, float>? OnResize
        {
            add { _framework.OnResize += value; }
            remove { _framework.OnResize -= value; }
        }

        public event Action<LingoMouseEvent>? OnMouseDown;
        public event Action<LingoMouseEvent>? OnMouseUp;
        public event Action<LingoMouseEvent>? OnMouseMove;

        public event Action<LingoKey>? OnKeyDown;
        public event Action<LingoKey>? OnKeyUp;

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

        private class KeyHandler : ILingoKeyEventHandler
        {
            private readonly LingoGfxWindow _owner;
            public KeyHandler(LingoGfxWindow owner) => _owner = owner;
            public void RaiseKeyDown(LingoKey lingoKey)
                => _owner.OnKeyDown?.Invoke(lingoKey);
            public void RaiseKeyUp(LingoKey lingoKey)
                => _owner.OnKeyUp?.Invoke(lingoKey);
        }

        public override void Dispose()
        {
            _mouseDownSub?.Release();
            _mouseUpSub?.Release();
            _mouseMoveSub?.Release();
            (_mouse as IDisposable)?.Dispose();
            _key.Unsubscribe(_keyHandler);
            (_key as IDisposable)?.Dispose();
            base.Dispose();
        }
    }
}
