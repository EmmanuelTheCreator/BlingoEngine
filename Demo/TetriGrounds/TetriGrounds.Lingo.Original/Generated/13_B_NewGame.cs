using System;
using LingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class NewGameBehavior : LingoSpriteBehavior, IHasMouseWithinEvent, IHasMouseLeaveEvent, IHasMouseUpEvent
{
    public NewGameBehavior(ILingoMovieEnvironment env) : base(env) { }
public void MouseUp(LingoMouseEvent mouse)
{
    Cursor = -1;
    _Movie.GoTo("Game");
}

public void MouseWithin(LingoMouseEvent mouse)
{
    Cursor = 280;
}

public void Mouseleave(LingoMouseEvent mouse)
{
    Cursor = -1;
}

}
