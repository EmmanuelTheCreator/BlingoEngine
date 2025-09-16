// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 11_Stop Menu.ls
    public class StopMenuBehavior : LingoSpriteBehavior, IHasExitFrameEvent
    {
        public StopMenuBehavior(ILingoMovieEnvironment env) : base(env){}
        public void ExitFrame()
        {
            _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}
