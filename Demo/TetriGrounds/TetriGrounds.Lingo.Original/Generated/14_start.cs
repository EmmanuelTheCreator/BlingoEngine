using System;
using LingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class startBehavior : LingoSpriteBehavior, IHasExitFrameEvent
{
    public startBehavior(ILingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void ExitFrame()
{
    _Movie.GoTo("Game");
}

_Movie.GoTo("Game");
// error
}
