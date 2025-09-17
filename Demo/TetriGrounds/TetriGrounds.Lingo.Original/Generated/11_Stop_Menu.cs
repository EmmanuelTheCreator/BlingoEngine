using System;
using LingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class Stop_MenuBehavior : LingoSpriteBehavior, IHasExitFrameEvent
{
    public Stop_MenuBehavior(ILingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void ExitFrame()
{
    _Movie.GoTo(_Movie.CurrentFrame);
}

_Movie.GoTo(_Movie.CurrentFrame);
// error
}
