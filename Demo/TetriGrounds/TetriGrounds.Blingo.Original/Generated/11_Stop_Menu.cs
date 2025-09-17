using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class Stop_MenuBehavior : BlingoSpriteBehavior, IHasExitFrameEvent
{
    public Stop_MenuBehavior(IBlingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void ExitFrame()
{
    _Movie.GoTo(_Movie.CurrentFrame);
}

_Movie.GoTo(_Movie.CurrentFrame);
// error
}

