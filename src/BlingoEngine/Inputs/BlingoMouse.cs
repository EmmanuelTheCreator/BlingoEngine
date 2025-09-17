using AbstUI.Inputs;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Events;
using BlingoEngine.Members;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;

namespace BlingoEngine.Inputs
{
    /// <summary>
    /// Provides access to a userâ€™s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface IBlingoStageMouse : IBlingoMouse
    {
        /// <summary>
        /// Mouse property; returns the last active sprite clicked by the user. Read-only.
        /// </summary>
        BlingoSprite2D? ClickOn { get; }
        /// <summary>
        /// Mouse property; returns the cast member underneath the mouse pointer. Read-only.
        /// </summary>
        BlingoMember? MouseMember { get; }
        /// <summary>
        /// Subscribe to mouse events.
        /// </summary>
        IBlingoMouse Subscribe(IBlingoMouseEventHandler handler);

        /// <summary>
        /// Unsubscribe from mouse events.
        /// </summary>
        IBlingoMouse Unsubscribe(IBlingoMouseEventHandler handler);
    }
    /// <summary>
    /// Lingo Framework Mouse interface.
    /// </summary>
    public interface IBlingoFrameworkMouse : IAbstFrameworkMouse
    {
        void SetCursor(BlingoMemberBitmap? image);
        void SetOffset(int x, int y);
    }

    /// <summary>
    /// Provides access to a userâ€™s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface IBlingoMouse : IAbstMouse
    {

        IBlingoCursor Cursor { get; }

        IAbstMouseSubscription OnMouseDown(Action<BlingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseUp(Action<BlingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseMove(Action<BlingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseWheel(Action<BlingoMouseEvent> handler);
        IAbstMouseSubscription OnMouseEvent(Action<BlingoMouseEvent> handler);
    }

    public class BlingoStageMouse : BlingoMouse, IBlingoStageMouse
    {
        private readonly BlingoStage _blingoStage;
        private HashSet<IBlingoMouseEventHandler> _subscriptions = new();
        public BlingoSprite2D? ClickOn => _blingoStage.GetSpriteUnderMouse();
        public BlingoMember? MouseMember { get => _blingoStage.MouseMemberUnderMouse; }
        public BlingoStageMouse(BlingoStage blingoMovieStage, IBlingoFrameworkMouse frameworkMouse)
            : base(frameworkMouse)
        {
            Name = "BlingoStageMouse";
            _blingoStage = blingoMovieStage;
        }

        public void SetOffset(int x, int y) => _blingoFrameworkObj.SetOffset(x, y);


        protected override void OnDoOnAll(BlingoMouseEvent eventMouse, Action<IAbstMouseEventHandler<BlingoMouseEvent>, BlingoMouseEvent> action)
        {
            base.OnDoOnAll(eventMouse, action);
            foreach (var subscription in _subscriptions.ToArray())
            {
                action(subscription, eventMouse);
                if (!eventMouse.ContinuePropagation) return;
            }
        }
        /// <summary>
        /// Subscribe to mouse events
        /// </summary>
        public IBlingoMouse Subscribe(IBlingoMouseEventHandler handler)
        {
            if (_subscriptions.Contains(handler)) return this;
            _subscriptions.Add(handler);
            return this;
        }

        /// <summary>
        /// Unsubscribe from mouse events
        /// </summary>
        public IBlingoMouse Unsubscribe(IBlingoMouseEventHandler handler)
        {
            _subscriptions.Remove(handler);
            return this;
        }
        internal bool IsSubscribed(BlingoSprite2D sprite) => _subscriptions.Contains(sprite);

        // <summary>
        /// Creates a proxy mouse that forwards events within the bounds supplied by <paramref name="provider"/>.
        /// </summary>

        public new virtual BlingoStageMouse CreateNewInstance(IAbstMouseRectProvider provider)
            => new ProxyBlingoMouse(this, provider);
        public virtual BlingoStageMouse DisplaceMouse(IAbstMouseRectProvider provider)
        {
            var newStageMouse = CreateNewInstance(provider);
            ReplaceFrameworkObj(newStageMouse.Framework<IBlingoFrameworkMouse>());
            return newStageMouse;
        }
        private sealed class ProxyBlingoMouse : BlingoStageMouse, IDisposable
        {
            private readonly BlingoStageMouse _parent;
            private readonly IAbstMouseRectProvider _provider;
            private readonly IAbstMouseSubscription _downSub;
            private readonly IAbstMouseSubscription _upSub;
            private readonly IAbstMouseSubscription _moveSub;
            private readonly IAbstMouseSubscription _wheelSub;

            internal ProxyBlingoMouse(BlingoStageMouse parent, IAbstMouseRectProvider provider)
                : base(parent._blingoStage, parent.Framework<IBlingoFrameworkMouse>())
            {
                Name = "BlingoStageMouseProxy";
                _parent = parent;
                _provider = provider;
                _downSub = parent.OnMouseDown(HandleDown);
                _upSub = parent.OnMouseUp(HandleUp);
                _moveSub = parent.OnMouseMove(HandleMove);
                _wheelSub = parent.OnMouseWheel(HandleWheel);
            }

            protected override ARect GetMouseOffset() => _provider.MouseOffset;

            private bool ShouldForward(BlingoMouseEvent e)
            {
                if (!_provider.IsActivated) return false;
                var r = _provider.MouseOffset;
                return e.MouseH >= r.Left && e.MouseH < r.Left + r.Width &&
                       e.MouseV >= r.Top && e.MouseV < r.Top + r.Height;
            }

            private void UpdateFromParent(BlingoMouseEvent e)
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


            private void HandleDown(BlingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseDown();
                UpdateMouseState();
            }

            private void HandleUp(BlingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseUp();
                UpdateMouseState();
            }

            private void HandleMove(BlingoMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseMove();
                UpdateMouseState();
            }

            private void HandleWheel(BlingoMouseEvent e)
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






    public class BlingoMouse : AbstMouse<BlingoMouseEvent>, IBlingoMouse, IAbstMouseInternal
    {
        protected IBlingoFrameworkMouse _blingoFrameworkObj;

        private readonly BlingoCursor _cursor;

        public IBlingoCursor Cursor => _cursor;


        public BlingoMouse(IBlingoFrameworkMouse frameworkMouse)
            : base((p, type) => new BlingoMouseEvent((BlingoMouse)p, type), frameworkMouse)
        {
            Name = "BlingoMouse";
            _blingoFrameworkObj = frameworkMouse;
            _cursor = new BlingoCursor(frameworkMouse);
        }


        public override void SetCursor(AMouseCursor cursorType)
        {
            if (_cursor.CursorType == cursorType) return;
            _cursor.CursorType = cursorType;
            base.SetCursor(cursorType);
            _blingoFrameworkObj.SetCursor(cursorType);
        }

    }
}

