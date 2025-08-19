using AbstUI.Primitives;

namespace AbstUI.Inputs
{

    /// <summary>
    /// Provides access to a user’s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface IAbstMouse
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
        float WheelDelta { get; set; }

        IAbstMouse CreateNewInstance(IAbstMouseRectProvider provider);
       
        void SetCursor(AMouseCursor cursor);
    }
    public interface IAbstMouse<TAbstUIMouseEvent> : IAbstMouse
        where TAbstUIMouseEvent : AbstMouseEvent
    {

        IAbstMouseSubscription OnMouseDown(Action<TAbstUIMouseEvent> handler);
        IAbstMouseSubscription OnMouseUp(Action<TAbstUIMouseEvent> handler);
        IAbstMouseSubscription OnMouseMove(Action<TAbstUIMouseEvent> handler);
        IAbstMouseSubscription OnMouseWheel(Action<TAbstUIMouseEvent> handler);
        IAbstMouseSubscription OnMouseEvent(Action<TAbstUIMouseEvent> handler);
        T Framework<T>() where T : IAbstFrameworkMouse;
        IAbstMouse CreateNewInstance(IAbstMouseRectProvider provider, Func<IAbstMouse, AbstMouseEventType, TAbstUIMouseEvent> ctorNewEvent);
    }
    public interface IAbstMouseSubscription
    {
        void Release();
    }



    
    public interface IAbstMouseInternal : IAbstMouse
    {
        /// <summary>
        /// Returns TRUE if the user double-clicked the mouse; otherwise FALSE.
        /// Read-only. Set by the system when a double-click occurs.
        /// </summary>
        new bool DoubleClick {get;set; }


        /// <summary>
        /// Returns TRUE while the mouse button is held down; otherwise FALSE.
        /// </summary>
        new bool MouseDown {get;set; }

        /// <summary>
        /// Returns the horizontal position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        new float MouseH {get;set; }



        /// <summary>
        /// Returns TRUE on the frame when the mouse button is released.
        /// </summary>
        new bool MouseUp {get;set; }

        /// <summary>
        /// Returns the vertical position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        new float MouseV {get;set; }


        /// <summary>
        /// Returns TRUE while the right mouse button is held down (Windows only).
        /// </summary>
        new bool RightMouseDown {get;set; }

        /// <summary>
        /// Returns TRUE on the frame when the right mouse button is released (Windows only).
        /// </summary>
        new bool RightMouseUp {get;set; }

       
        new bool LeftMouseDown {get;set; }
        new bool MiddleMouseDown {get;set; }
        new float WheelDelta { get; set; }
        ARect GetMouseOffset();
        void DoMouseUp();

        void DoMouseDown();

        void DoMouseMove();

        void DoMouseWheel(float delta);
    }

    public class AbstMouse : AbstMouse<AbstMouseEvent>
    {
        public AbstMouse(IAbstFrameworkMouse frameworkMouse) : base((m, t) => new AbstMouseEvent(m, t), frameworkMouse)
        {
        }
    }
    public class AbstMouse<TAbstUIMouseEvent> : IAbstMouse<TAbstUIMouseEvent>, IAbstMouseInternal
        where TAbstUIMouseEvent : AbstMouseEvent
    {
        private bool _lastMouseDownState = false; // Previous mouse state (used to detect "StillDown")
        private readonly List<AbstUIMouseSubscription> _mouseUps = new();
        private readonly List<AbstUIMouseSubscription> _mouseDowns = new();
        private readonly List<AbstUIMouseSubscription> _mouseMoves = new();
        private readonly List<AbstUIMouseSubscription> _mouseWheels = new();
        private readonly List<AbstUIMouseSubscription> _mouseEvents = new();
        private readonly Func<IAbstMouse, AbstMouseEventType, TAbstUIMouseEvent> _ctorNewEvent;
        private IAbstFrameworkMouse _frameworkObj;
        public T Framework<T>() where T : IAbstFrameworkMouse => (T)_frameworkObj;

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


        public AbstMouse(Func<IAbstMouse, AbstMouseEventType, TAbstUIMouseEvent> ctorNewEvent, IAbstFrameworkMouse frameworkMouse)
        {
            _ctorNewEvent = ctorNewEvent;
            _frameworkObj = frameworkMouse;
        }
        public void ReplaceFrameworkObj(IAbstFrameworkMouse mouseFrameworkObj)
        {
            _frameworkObj.Release();
            _frameworkObj = mouseFrameworkObj;
            mouseFrameworkObj.ReplaceMouseObj(this);
        }



        /// <summary>
        /// Creates a proxy mouse that forwards events within the bounds supplied by <paramref name="provider"/>.
        /// </summary>
        public virtual IAbstMouse CreateNewInstance(IAbstMouseRectProvider provider) => new ProxyMouse(_ctorNewEvent, this, provider);
        public virtual IAbstMouse CreateNewInstance(IAbstMouseRectProvider provider, Func<IAbstMouse, AbstMouseEventType, TAbstUIMouseEvent> ctorNewEvent)
            => new ProxyMouse(ctorNewEvent, this, provider);
        protected virtual ARect GetMouseOffset() => default;

        ARect IAbstMouseInternal.GetMouseOffset() => GetMouseOffset();
        /// <summary>
        /// Called from communiction framework mouse
        /// </summary>
        public virtual void DoMouseUp() => DoOnAll(_mouseUps, (x, e) => x.RaiseMouseUp(e), AbstMouseEventType.MouseUp);

        public virtual void DoMouseDown() => DoOnAll(_mouseDowns, (x, e) => x.RaiseMouseDown(e), AbstMouseEventType.MouseDown);

        public virtual void DoMouseMove() => DoOnAll(_mouseMoves, (x, e) => x.RaiseMouseMove(e), AbstMouseEventType.MouseMove);

        public virtual void DoMouseWheel(float delta)
        {
            WheelDelta = delta;
            DoOnAll(_mouseWheels, (x, e) => x.RaiseMouseWheel(e), AbstMouseEventType.MouseWheel);
        }


        private void DoOnAll(List<AbstUIMouseSubscription> subscriptions, Action<IAbstMouseEventHandler<TAbstUIMouseEvent>, TAbstUIMouseEvent> action, AbstMouseEventType type)
        {
            var eventMouse = _ctorNewEvent(this, type);// new AbstUIMouseEvent(this, type);
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
        protected virtual void OnDoOnAll(TAbstUIMouseEvent eventMouse, Action<IAbstMouseEventHandler<TAbstUIMouseEvent>, TAbstUIMouseEvent> action)
        { }


        // Method to update the mouse state at the end of each frame
        public void UpdateMouseState()
        {
            _lastMouseDownState = MouseDown;  // Save current mouse state for next frame
        }



        public virtual IAbstMouseSubscription OnMouseDown(Action<TAbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseDowns.Remove(s)); _mouseDowns.Add(sub); return sub; }
        public virtual IAbstMouseSubscription OnMouseUp(Action<TAbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseUps.Remove(s)); _mouseUps.Add(sub); return sub; }
        public virtual IAbstMouseSubscription OnMouseMove(Action<TAbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseMoves.Remove(s)); _mouseMoves.Add(sub); return sub; }
        public virtual IAbstMouseSubscription OnMouseWheel(Action<TAbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseWheels.Remove(s)); _mouseWheels.Add(sub); return sub; }
        public virtual IAbstMouseSubscription OnMouseEvent(Action<TAbstUIMouseEvent> handler) { var sub = new AbstUIMouseSubscription(handler, s => _mouseEvents.Remove(s)); _mouseEvents.Add(sub); return sub; }

        public virtual void SetCursor(AMouseCursor cursorType)
        {
            _frameworkObj.SetCursor(cursorType);
        }

        private class AbstUIMouseSubscription : IAbstMouseSubscription
        {
            private readonly Action<TAbstUIMouseEvent> _handler;
            private readonly Action<AbstUIMouseSubscription> _onRelease;
            public AbstUIMouseSubscription(Action<TAbstUIMouseEvent> handler, Action<AbstUIMouseSubscription> onRelease)
            {
                _handler = handler;
                _onRelease = onRelease;
            }
            internal void Do(TAbstUIMouseEvent mouseEvent)
            {
                _handler(mouseEvent);
            }
            public void Release()
            {
                _onRelease(this);
            }

        }

        private sealed class ProxyMouse : AbstMouse<TAbstUIMouseEvent>, IDisposable
        {
            private readonly AbstMouse<TAbstUIMouseEvent> _parent;
            private readonly IAbstMouseRectProvider _provider;
            private readonly IAbstMouseSubscription _downSub;
            private readonly IAbstMouseSubscription _upSub;
            private readonly IAbstMouseSubscription _moveSub;
            private readonly IAbstMouseSubscription _wheelSub;

            internal ProxyMouse(Func<IAbstMouse, AbstMouseEventType, TAbstUIMouseEvent> ctorNewEvent, AbstMouse<TAbstUIMouseEvent> parent, IAbstMouseRectProvider provider)
                : base(ctorNewEvent, parent.Framework<IAbstFrameworkMouse>())
            {
                _parent = parent;
                _provider = provider;
                _downSub = parent.OnMouseDown(HandleDown);
                _upSub = parent.OnMouseUp(HandleUp);
                _moveSub = parent.OnMouseMove(HandleMove);
                _wheelSub = parent.OnMouseWheel(HandleWheel);
            }

            protected override ARect GetMouseOffset() => _provider.MouseOffset;

            private bool ShouldForward(TAbstUIMouseEvent e)
            {
                if (!_provider.IsActivated) return false;
                var r = _provider.MouseOffset;
                return e.MouseH >= r.Left && e.MouseH < r.Left + r.Width &&
                       e.MouseV >= r.Top && e.MouseV < r.Top + r.Height;
            }

            private void UpdateFromParent(TAbstUIMouseEvent e)
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


            private void HandleDown(TAbstUIMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseDown();
                UpdateMouseState();
            }

            private void HandleUp(TAbstUIMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseUp();
                UpdateMouseState();
            }

            private void HandleMove(TAbstUIMouseEvent e)
            {
                if (!ShouldForward(e)) return;
                UpdateFromParent(e);
                DoMouseMove();
                UpdateMouseState();
            }

            private void HandleWheel(TAbstUIMouseEvent e)
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
