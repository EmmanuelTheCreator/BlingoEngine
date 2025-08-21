using AbstUI.Inputs;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Stages;

namespace LingoEngine.Inputs
{
    /// <summary>
    /// Provides access to a user’s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface ILingoStageMouse : ILingoMouse
    {
        /// <summary>
        /// Mouse property; returns the last active sprite clicked by the user. Read-only.
        /// </summary>
        LingoSprite2D? ClickOn { get; }
        /// <summary>
        /// Mouse property; returns the cast member underneath the mouse pointer. Read-only.
        /// </summary>
        LingoMember? MouseMember { get; }
        /// <summary>
        /// Subscribe to mouse events.
        /// </summary>
        ILingoMouse Subscribe(ILingoMouseEventHandler handler);

        /// <summary>
        /// Unsubscribe from mouse events.
        /// </summary>
        ILingoMouse Unsubscribe(ILingoMouseEventHandler handler);
    }
    /// <summary>
    /// Lingo Framework Mouse interface.
    /// </summary>
    public interface ILingoFrameworkMouse : IAbstFrameworkMouse
    {
        void SetCursor(LingoMemberBitmap? image);
        void SetOffset(int x, int y);
    }

    /// <summary>
    /// Provides access to a user’s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface ILingoMouse : IAbstMouse
    {

        ILingoCursor Cursor { get; }

        IAbstMouseSubscription OnMouseDown(Action<LingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseUp(Action<LingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseMove(Action<LingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseWheel(Action<LingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseEvent(Action<LingoMouseEvent> handler);
    }

    public class LingoStageMouse : LingoMouse, ILingoStageMouse
    {
        private readonly LingoStage _lingoStage;
        private HashSet<ILingoMouseEventHandler> _subscriptions = new();
        public LingoSprite2D? ClickOn => _lingoStage.GetSpriteUnderMouse();
        public LingoMember? MouseMember { get => _lingoStage.MouseMemberUnderMouse; }
        public LingoStageMouse(LingoStage lingoMovieStage, ILingoFrameworkMouse frameworkMouse)
            : base(frameworkMouse)
        {
            Name = "LingoStageMouse";
            _lingoStage = lingoMovieStage;
        }

        public void SetOffset(int x, int y) => _lingoFrameworkObj.SetOffset(x, y);


        protected override void OnDoOnAll(LingoMouseEvent eventMouse, Action<IAbstMouseEventHandler<LingoMouseEvent>, LingoMouseEvent> action)
        {
            base.OnDoOnAll(eventMouse, action);
            foreach (var subscription in _subscriptions)
            {
                action(subscription, eventMouse);
                if (!eventMouse.ContinuePropagation) return;
            }
        }
        /// <summary>
        /// Subscribe to mouse events
        /// </summary>
        public ILingoMouse Subscribe(ILingoMouseEventHandler handler)
        {
            if (_subscriptions.Contains(handler)) return this;
            _subscriptions.Add(handler);
            return this;
        }

        /// <summary>
        /// Unsubscribe from mouse events
        /// </summary>
        public ILingoMouse Unsubscribe(ILingoMouseEventHandler handler)
        {
            _subscriptions.Remove(handler);
            return this;
        }
        internal bool IsSubscribed(LingoSprite2D sprite) => _subscriptions.Contains(sprite);

        // <summary>
        /// Creates a proxy mouse that forwards events within the bounds supplied by <paramref name="provider"/>.
        /// </summary>

        public new virtual LingoStageMouse CreateNewInstance(IAbstMouseRectProvider provider)
            => new ProxyLingoMouse(this, provider);
        public virtual LingoStageMouse DisplaceMouse(IAbstMouseRectProvider provider)
        {
            var newStageMouse = CreateNewInstance(provider);
            ReplaceFrameworkObj(newStageMouse.Framework<ILingoFrameworkMouse>());
            return newStageMouse;
        }
        private sealed class ProxyLingoMouse : LingoStageMouse, IDisposable
        {
            private readonly LingoStageMouse _parent;
            private readonly IAbstMouseRectProvider _provider;
            private readonly IAbstMouseSubscription _downSub;
            private readonly IAbstMouseSubscription _upSub;
            private readonly IAbstMouseSubscription _moveSub;
            private readonly IAbstMouseSubscription _wheelSub;

            internal ProxyLingoMouse(LingoStageMouse parent, IAbstMouseRectProvider provider)
                : base(parent._lingoStage, parent.Framework<ILingoFrameworkMouse>())
            {
                Name = "LingoStageMouseProxy";
                _parent = parent;
                _provider = provider;
                _downSub = parent.OnMouseDown(HandleDown);
                _upSub = parent.OnMouseUp(HandleUp);
                _moveSub = parent.OnMouseMove(HandleMove);
                _wheelSub = parent.OnMouseWheel(HandleWheel);
            }

            protected override ARect GetMouseOffset() => _provider.MouseOffset;

            private bool ShouldForward(LingoMouseEvent e)
            {
                if (!_provider.IsActivated) return false;
                var r = _provider.MouseOffset;
                return e.MouseH >= r.Left && e.MouseH < r.Left + r.Width &&
                       e.MouseV >= r.Top && e.MouseV < r.Top + r.Height;
            }

            private void UpdateFromParent(LingoMouseEvent e)
            {
                var r = _provider.MouseOffset;
                MouseH = e.MouseH - r.Left;
                MouseV = e.MouseV - r.Top;
                MouseDown = _parent.MouseDown;
                MouseUp = _parent.MouseUp;
                RightMouseDown = _parent.RightMouseDown;
                RightMouseUp = _parent.RightMouseUp;
                LeftMouseDown = _parent.LeftMouseDown;
                MiddleMouseDown = _parent.MiddleMouseDown;
                DoubleClick = _parent.DoubleClick;
            }


            private void HandleDown(LingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseDown();
                UpdateMouseState();
            }

            private void HandleUp(LingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseUp();
                UpdateMouseState();
            }

            private void HandleMove(LingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseMove();
                UpdateMouseState();
            }

            private void HandleWheel(LingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                WheelDelta = e.WheelDelta;
                base.DoMouseWheel(e.WheelDelta);
                UpdateMouseState();
            }

            public void Dispose()
            {
                _downSub.Release();
                _upSub.Release();
                _moveSub.Release();
                _wheelSub.Release();
            }
        }
    }






    public class LingoMouse : AbstMouse<LingoMouseEvent>, ILingoMouse, IAbstMouseInternal
    {
        protected ILingoFrameworkMouse _lingoFrameworkObj;

        private readonly LingoCursor _cursor;

        public ILingoCursor Cursor => _cursor;


        public LingoMouse(ILingoFrameworkMouse frameworkMouse)
            : base((p, type) => new LingoMouseEvent((LingoMouse)p, type), frameworkMouse)
        {
            Name = "LingoMouse";
            _lingoFrameworkObj = frameworkMouse;
            _cursor = new LingoCursor(frameworkMouse);
        }


        public override void SetCursor(AMouseCursor cursorType)
        {
            if (_cursor.CursorType == cursorType) return;
            _cursor.CursorType = cursorType;
            base.SetCursor(cursorType);
            _lingoFrameworkObj.SetCursor(cursorType);
        }

    }
}
