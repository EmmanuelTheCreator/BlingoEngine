using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.AbstUI.Inputs
{

    /// <summary>
    /// Provides access to a user’s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface IAbstUIMouse
    {


        /// <summary>
        /// Returns TRUE if the user double-clicked the mouse; otherwise FALSE.
        /// Read-only. Set by the system when a double-click occurs.
        /// </summary>
        bool DoubleClick { get; }

        /// <summary>
        /// Returns the character where the mouse was clicked, typically used in text contexts.
        /// </summary>
        char MouseChar { get; }

        /// <summary>
        /// Returns TRUE while the mouse button is held down; otherwise FALSE.
        /// </summary>
        bool MouseDown { get; }

        /// <summary>
        /// Returns the horizontal position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        float MouseH { get; }

        /// <summary>
        /// Returns the line number of text the mouse is over, usually for field members.
        /// </summary>
        int MouseLine { get; }

        /// <summary>
        /// Returns the (H, V) point location of the mouse on the Stage.
        /// </summary>
        APoint MouseLoc { get; }

        /// <summary>
        /// Returns TRUE on the frame when the mouse button is released.
        /// </summary>
        bool MouseUp { get; }

        /// <summary>
        /// Returns the vertical position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        float MouseV { get; }

        /// <summary>
        /// Returns the word that the mouse pointer is over, typically in a field.
        /// </summary>
        string MouseWord { get; }

        /// <summary>
        /// Returns TRUE while the right mouse button is held down (Windows only).
        /// </summary>
        bool RightMouseDown { get; }

        /// <summary>
        /// Returns TRUE on the frame when the right mouse button is released (Windows only).
        /// </summary>
        bool RightMouseUp { get; }

        /// <summary>
        /// Returns TRUE if the mouse is still down from a prior frame.
        /// </summary>
        bool StillDown { get; }
        bool LeftMouseDown { get; }
        bool MiddleMouseDown { get; }


        IAbstUIMouseSubscription OnMouseDown(Action<AbstUIMouseEvent> handler);
        IAbstUIMouseSubscription OnMouseUp(Action<AbstUIMouseEvent> handler);
        IAbstUIMouseSubscription OnMouseMove(Action<AbstUIMouseEvent> handler);
        IAbstUIMouseSubscription OnMouseWheel(Action<AbstUIMouseEvent> handler);
        IAbstUIMouseSubscription OnMouseEvent(Action<AbstUIMouseEvent> handler);
    }
    public interface IAbstUIMouseSubscription
    {
        void Release();
    }






    public class AbstUIMouse : IAbstUIMouse
    {
        private bool _lastMouseDownState = false; // Previous mouse state (used to detect "StillDown")
        private readonly List<AbstUIMouseSubscription> _mouseUps = new();
        private readonly List<AbstUIMouseSubscription> _mouseDowns = new();
        private readonly List<AbstUIMouseSubscription> _mouseMoves = new();
        private readonly List<AbstUIMouseSubscription> _mouseWheels = new();
        private readonly List<AbstUIMouseSubscription> _mouseEvents = new();


        private IAbstUIFrameworkMouse _frameworkObj;
        public T Framework<T>() where T : IAbstUIFrameworkMouse => (T)_frameworkObj;


        public APoint MouseLoc => new APoint(MouseH, MouseV);

        public float MouseH { get; set; }
        public float MouseV { get; set; }
        public bool MouseUp { get; set; }
        public bool MouseDown { get; set; }
        public bool RightMouseUp { get; set; }
        public bool RightMouseDown { get; set; }
        public bool StillDown => MouseDown && _lastMouseDownState;
        public bool DoubleClick { get; set; }
        public char MouseChar => ' ';
        public string MouseWord => "";
        public int MouseLine => 0;


        public bool LeftMouseDown { get; set; }
        public bool MiddleMouseDown { get; set; }
        public float WheelDelta { get; set; }


        public AbstUIMouse(IAbstUIFrameworkMouse frameworkMouse)
        {
            _frameworkObj = frameworkMouse;
        }
        public void ReplaceFrameworkObj(IAbstUIFrameworkMouse mouseFrameworkObj)
        {
            _frameworkObj.Release();
            _frameworkObj = mouseFrameworkObj;
            mouseFrameworkObj.ReplaceMouseObj(this);
        }



        /// <summary>
        /// Creates a proxy mouse that forwards events within the bounds supplied by <paramref name="provider"/>.
        /// </summary>
        public AbstUIMouse CreateNewInstance(IAbstUIMouseRectProvider provider) => new ProxyMouse(this, provider);
        /// <summary>
        /// Called from communiction framework mouse
        /// </summary>
        public virtual void DoMouseUp() => DoOnAll(_mouseUps, (x, e) => x.RaiseMouseUp(e), AbstUIMouseEventType.MouseUp);

        public virtual void DoMouseDown() => DoOnAll(_mouseDowns, (x, e) => x.RaiseMouseDown(e), AbstUIMouseEventType.MouseDown);

        public virtual void DoMouseMove() => DoOnAll(_mouseMoves, (x, e) => x.RaiseMouseMove(e), AbstUIMouseEventType.MouseMove);

        public virtual void DoMouseWheel(float delta)
        {
            WheelDelta = delta;
            DoOnAll(_mouseWheels, (x, e) => x.RaiseMouseWheel(e), AbstUIMouseEventType.MouseWheel);
        }


        private void DoOnAll(List<AbstUIMouseSubscription> subscriptions, Action<IAbstUIMouseEventHandler, AbstUIMouseEvent> action, AbstUIMouseEventType type)
        {
            var eventMouse = new AbstUIMouseEvent(this, type);
            foreach (var subscription in subscriptions)
            {
                subscription.Do(eventMouse);
                if (!eventMouse.ContinuePropation) return;
            }
            foreach (var subscription in _mouseEvents)
            {
                subscription.Do(eventMouse);
                if (!eventMouse.ContinuePropation) return;
            }
            OnDoOnAll(eventMouse, action);
        }
        protected virtual void OnDoOnAll(AbstUIMouseEvent eventMouse, Action<IAbstUIMouseEventHandler, AbstUIMouseEvent> action)
        { }


        // Method to update the mouse state at the end of each frame
        internal void UpdateMouseState()
        {
            _lastMouseDownState = MouseDown;  // Save current mouse state for next frame
        }



        public virtual IAbstUIMouseSubscription OnMouseDown(Action<AbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseDowns.Remove(s)); _mouseDowns.Add(sub); return sub; }
        public virtual IAbstUIMouseSubscription OnMouseUp(Action<AbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseUps.Remove(s)); _mouseUps.Add(sub); return sub; }
        public virtual IAbstUIMouseSubscription OnMouseMove(Action<AbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseMoves.Remove(s)); _mouseMoves.Add(sub); return sub; }
        public virtual IAbstUIMouseSubscription OnMouseWheel(Action<AbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseWheels.Remove(s)); _mouseWheels.Add(sub); return sub; }
        public virtual IAbstUIMouseSubscription OnMouseEvent(Action<AbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseEvents.Remove(s)); _mouseEvents.Add(sub); return sub; }

        private class AbstUIMouseSubscription : IAbstUIMouseSubscription
        {
            private readonly Action<AbstUIMouseEvent> _handler;
            private readonly Action<AbstUIMouseSubscription> _onRelease;
            public AbstUIMouseSubscription(Action<AbstUIMouseEvent> handler, Action<AbstUIMouseSubscription> onRelease)
            {
                _handler = handler;
                _onRelease = onRelease;
            }
            internal void Do(AbstUIMouseEvent mouseEvent)
            {
                _handler(mouseEvent);
            }
            public void Release()
            {
                _onRelease(this);
            }

        }

        private sealed class ProxyMouse : AbstUIMouse, IDisposable
        {
            private readonly AbstUIMouse _parent;
            private readonly IAbstUIMouseRectProvider _provider;
            private readonly IAbstUIMouseSubscription _downSub;
            private readonly IAbstUIMouseSubscription _upSub;
            private readonly IAbstUIMouseSubscription _moveSub;
            private readonly IAbstUIMouseSubscription _wheelSub;

            internal ProxyMouse(AbstUIMouse parent, IAbstUIMouseRectProvider provider)
                : base(parent.Framework<IAbstUIFrameworkMouse>())
            {
                _parent = parent;
                _provider = provider;
                _downSub = parent.OnMouseDown(HandleDown);
                _upSub = parent.OnMouseUp(HandleUp);
                _moveSub = parent.OnMouseMove(HandleMove);
                _wheelSub = parent.OnMouseWheel(HandleWheel);
            }

            private bool ShouldForward(AbstUIMouseEvent e)
            {
                if (!_provider.IsActivated) return false;
                var r = _provider.MouseOffset;
                return e.MouseH >= r.Left && e.MouseH < r.Left + r.Width &&
                       e.MouseV >= r.Top && e.MouseV < r.Top + r.Height;
            }

            private void UpdateFromParent(AbstUIMouseEvent e)
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


            private void HandleDown(AbstUIMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseDown();
                UpdateMouseState();
            }

            private void HandleUp(AbstUIMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseUp();
                UpdateMouseState();
            }

            private void HandleMove(AbstUIMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseMove();
                UpdateMouseState();
            }

            private void HandleWheel(AbstUIMouseEvent e)
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
}
