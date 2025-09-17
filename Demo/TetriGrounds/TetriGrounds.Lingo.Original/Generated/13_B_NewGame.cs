using System;
using LingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class NewGameBehavior : LingoSpriteBehavior, IHasMouseWithinEvent, IHasMouseLeaveEvent, IHasMouseUpEvent
{
    public NewGameBehavior(ILingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void MouseUp(LingoMouseEvent mouse)
{
    Cursor = -1;
    _Movie.GoTo("Game");
}

public void MouseWithin(LingoMouseEvent mouse)
{
    Cursor = 280;
}

public void MouseLeave(LingoMouseEvent mouse)
{
    Cursor = -1;
}

Cursor = -1;
_Movie.GoTo("Game");
// error
public void MouseWithin(LingoMouseEvent mouse)
{
    Cursor = 280;
}

public void MouseLeave(LingoMouseEvent mouse)
{
    Cursor = -1;
}

}
