using System;
using LingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class startBehavior : LingoSpriteBehavior, IHasExitFrameEvent
{
    public startBehavior(ILingoMovieEnvironment env) : base(env) { }
public void ExitFrame()
{
    _Movie.GoTo("Game");
}

}
