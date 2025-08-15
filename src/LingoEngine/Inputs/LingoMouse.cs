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
    public interface ILingoMouse : IAbstMouse
    {


        ILingoCursor Cursor { get; }
        void SetCursor(AMouseCursor cursorType);


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
            _lingoStage = lingoMovieStage;
        }
        protected override void OnDoOnAll(LingoMouseEvent eventMouse, Action<IAbstMouseEventHandler<LingoMouseEvent>, LingoMouseEvent> action)
        {
            base.OnDoOnAll(eventMouse, action);
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



    public interface ILingoFrameworkMouse : IAbstFrameworkMouse
    {
        void SetCursor(AMouseCursor cursorType);
        void SetCursor(LingoMemberBitmap? image);
    }


    public class LingoMouse : AbstMouse<LingoMouseEvent>, ILingoMouse
    {
        ILingoFrameworkMouse _lingoFrameworkObj;

        private readonly LingoCursor _cursor;

        public ILingoCursor Cursor => _cursor;


        public LingoMouse(ILingoFrameworkMouse frameworkMouse)
            :base((p,type) => new LingoMouseEvent((LingoMouse)p, type), frameworkMouse)
        {
            _lingoFrameworkObj = frameworkMouse;
            _cursor = new LingoCursor(frameworkMouse);
        }
        

        public void SetCursor(AMouseCursor cursorType)
        {
            if (_cursor.CursorType == cursorType) return;
            _cursor.CursorType = cursorType;
            _lingoFrameworkObj.SetCursor(cursorType);
        }

        ///// <summary>
        ///// Called from communiction framework mouse
        ///// </summary>
        //public virtual void DoMouseUp() => DoOnAll(_mouseUps, (x, e) => x.RaiseMouseUp(e), LingoMouseEventType.MouseUp);

        //public virtual void DoMouseDown() => DoOnAll(_mouseDowns, (x, e) => x.RaiseMouseDown(e), LingoMouseEventType.MouseDown);

        //public virtual void DoMouseMove() => DoOnAll(_mouseMoves, (x, e) => x.RaiseMouseMove(e), LingoMouseEventType.MouseMove);

        //public virtual void DoMouseWheel(float delta)
        //{
        //    WheelDelta = delta;
        //    DoOnAll(_mouseWheels, (x, e) => x.RaiseMouseWheel(e), LingoMouseEventType.MouseWheel);
        //}


       
        



        //public virtual IAbstUIMouseSubscription OnMouseDown(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseDowns.Remove(s)); _mouseDowns.Add(sub); return sub; }
        //public virtual IAbstUIMouseSubscription OnMouseUp(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseUps.Remove(s)); _mouseUps.Add(sub); return sub; }
        //public virtual IAbstUIMouseSubscription OnMouseMove(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseMoves.Remove(s)); _mouseMoves.Add(sub); return sub; }
        //public virtual IAbstUIMouseSubscription OnMouseWheel(Action<LingoMouseEvent> handler) { var sub = new LingoMouseSubscription(handler, s => _mouseWheels.Remove(s)); _mouseWheels.Add(sub); return sub; }
        //public virtual IAbstUIMouseSubscription OnMouseEvent(Action<LingoMouseEvent> handler)
        //    => base.OnMouseEvent(e => handler(e));

       
    }
}
