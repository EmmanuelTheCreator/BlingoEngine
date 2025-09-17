using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class startBehavior : BlingoSpriteBehavior, IHasExitFrameEvent
{
    public startBehavior(IBlingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void ExitFrame()
{
    _Movie.GoTo("Game");
}

_Movie.GoTo("Game");
// error
}
