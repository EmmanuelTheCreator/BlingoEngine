using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Primitives;
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
        /// Returns or sets the cast member underneath the mouse pointer.
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
    /// Provides access to a user’s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface ILingoMouse
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
        LingoPoint MouseLoc { get; }

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
        ILingoCursor Cursor { get; }

      
        ILingoMouseSubscription OnMouseDown(Action<LingoMouseEvent> handler);
        ILingoMouseSubscription OnMouseUp(Action<LingoMouseEvent> handler);
        ILingoMouseSubscription OnMouseMove(Action<LingoMouseEvent> handler);
        ILingoMouseSubscription OnMouseEvent(Action<LingoMouseEvent> handler);
    }
    public interface ILingoMouseSubscription
    {
        void Release();
    }

    public class LingoStageMouse : LingoMouse, ILingoStageMouse
    {
        private readonly LingoStage _lingoStage;
        private HashSet<ILingoMouseEventHandler> _subscriptions = new();
        public LingoSprite2D? ClickOn => _lingoStage.GetSpriteUnderMouse();
        public LingoMember? MouseMember { get => _lingoStage.MouseMemberUnderMouse; }
        public LingoStageMouse(LingoStage lingoMovieStage, ILingoFrameworkMouse frameworkMouse)
            :base(frameworkMouse)
        {
            _lingoStage = lingoMovieStage;
        }
      
        protected override void OnDoOnAll(LingoMouseEvent eventMouse, Action<ILingoMouseEventHandler, LingoMouseEvent> action)
        {
            foreach (var subscription in _subscriptions)
            {
                action(subscription, eventMouse);
                if (!eventMouse.ContinuePropation) return;
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

       
    }






    public class LingoMouse : ILingoMouse
    {
        private bool _lastMouseDownState = false; // Previous mouse state (used to detect "StillDown")
        private readonly List<LingoMouseSubscription> _mouseUps = new();
        private readonly List<LingoMouseSubscription> _mouseDowns = new();
        private readonly List<LingoMouseSubscription> _mouseMoves = new();
        private readonly List<LingoMouseSubscription> _mouseEvents = new();
        
        
        private readonly LingoCursor _cursor;
        private ILingoFrameworkMouse _frameworkObj;
        public T Framework<T>() where T : ILingoFrameworkMouse => (T)_frameworkObj;

        
        public LingoPoint MouseLoc => new LingoPoint(MouseH, MouseV);

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
        public ILingoCursor Cursor => _cursor;


        public LingoMouse(ILingoFrameworkMouse frameworkMouse)
        {
            _frameworkObj = frameworkMouse;
            _cursor = new LingoCursor(_frameworkObj);
        }
        public void ReplaceFrameworkObj(ILingoFrameworkMouse mouseFrameworkObj)
        {
            _frameworkObj.Release();
            _frameworkObj = mouseFrameworkObj;
            mouseFrameworkObj.ReplaceMouseObj(this);
        }

        /// <summary>
        /// Called from communiction framework mouse
        /// </summary>
        public virtual void DoMouseUp() => DoOnAll(_mouseUps, (x, e) => x.RaiseMouseUp(e), LingoMouseEventType.MouseUp);

        public virtual void DoMouseDown() => DoOnAll(_mouseDowns, (x, e) => x.RaiseMouseDown(e), LingoMouseEventType.MouseDown);

        public virtual void DoMouseMove() => DoOnAll(_mouseMoves, (x, e) => x.RaiseMouseMove(e), LingoMouseEventType.MouseMove);
        

        private void DoOnAll(List<LingoMouseSubscription> subscriptions, Action<ILingoMouseEventHandler, LingoMouseEvent> action, LingoMouseEventType type)
        {
            var eventMouse = new LingoMouseEvent(this, type);
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
        protected virtual void OnDoOnAll(LingoMouseEvent eventMouse, Action<ILingoMouseEventHandler, LingoMouseEvent> action)
        { }

        
        // Method to update the mouse state at the end of each frame
        internal void UpdateMouseState()
        {
            _lastMouseDownState = MouseDown;  // Save current mouse state for next frame
        }
       

       
        public ILingoMouseSubscription OnMouseDown(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseDowns.Remove(s));_mouseDowns.Add(sub); return sub; }
        public ILingoMouseSubscription OnMouseUp(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s =>_mouseUps.Remove(s));_mouseUps.Add(sub); return sub; }
        public ILingoMouseSubscription OnMouseMove(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseMoves.Remove(s)); _mouseMoves.Add(sub); return sub; }
        public ILingoMouseSubscription OnMouseEvent(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseEvents.Remove(s)); _mouseEvents.Add(sub); return sub; }

        private class LingoMouseSubscription : ILingoMouseSubscription
        {
            private readonly Action<LingoMouseEvent> _handler;
            private readonly Action<LingoMouseSubscription> _onRelease;
            public LingoMouseSubscription(Action<LingoMouseEvent> handler,Action<LingoMouseSubscription> onRelease)
            {
                _handler = handler;
                _onRelease = onRelease;
            }
            internal void Do(LingoMouseEvent mouseEvent)
            {
                _handler(mouseEvent);
            }
            public void Release()
            {
                _onRelease(this);
            }

        }
    }
}
