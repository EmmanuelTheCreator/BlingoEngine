// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 14_start.ls
    public class StartBehavior : BlingoSpriteBehavior, IHasExitFrameEvent
    {
        public StartBehavior(IBlingoMovieEnvironment env) : base(env){}
        public void ExitFrame() => _Movie.GoTo("Game");
    }
}

