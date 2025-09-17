using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class NewGameBehavior : BlingoSpriteBehavior, IHasMouseWithinEvent, IHasMouseLeaveEvent, IHasMouseUpEvent
{
    public NewGameBehavior(IBlingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void MouseUp(BlingoMouseEvent mouse)
{
    Cursor = -1;
    _Movie.GoTo("Game");
}

public void MouseWithin(BlingoMouseEvent mouse)
{
    Cursor = 280;
}

public void MouseLeave(BlingoMouseEvent mouse)
{
    Cursor = -1;
}

Cursor = -1;
_Movie.GoTo("Game");
// error
public void MouseWithin(BlingoMouseEvent mouse)
{
    Cursor = 280;
}

public void MouseLeave(BlingoMouseEvent mouse)
{
    Cursor = -1;
}

}
