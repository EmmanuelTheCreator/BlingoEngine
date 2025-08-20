using AbstUI.Primitives;

namespace AbstUI.Inputs
{
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
    public interface IAbstMouseInternal : IAbstMouse
    {
        /// <summary>
        /// Returns TRUE if the user double-clicked the mouse; otherwise FALSE.
        /// Read-only. Set by the system when a double-click occurs.
        /// </summary>
        new bool DoubleClick { get; set; }


        /// <summary>
        /// Returns TRUE while the mouse button is held down; otherwise FALSE.
        /// </summary>
        new bool MouseDown { get; set; }

        /// <summary>
        /// Returns the horizontal position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        new float MouseH { get; set; }



        /// <summary>
        /// Returns TRUE on the frame when the mouse button is released.
        /// </summary>
        new bool MouseUp { get; set; }

        /// <summary>
        /// Returns the vertical position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        new float MouseV { get; set; }


        /// <summary>
        /// Returns TRUE while the right mouse button is held down (Windows only).
        /// </summary>
        new bool RightMouseDown { get; set; }

        /// <summary>
        /// Returns TRUE on the frame when the right mouse button is released (Windows only).
        /// </summary>
        new bool RightMouseUp { get; set; }


        new bool LeftMouseDown { get; set; }
        new bool MiddleMouseDown { get; set; }
        new float WheelDelta { get; set; }
        ARect GetMouseOffset();
        void DoMouseUp();

        void DoMouseDown();

        void DoMouseMove();

        void DoMouseWheel(float delta);
    }
}
