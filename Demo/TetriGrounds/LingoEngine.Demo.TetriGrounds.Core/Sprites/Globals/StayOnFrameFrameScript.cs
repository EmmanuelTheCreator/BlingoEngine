// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    internal class StayOnFrameFrameScript : LingoSpriteBehavior, IHasEnterFrameEvent
    {
        public StayOnFrameFrameScript(ILingoMovieEnvironment env) : base(env)
        {
        }

        public void EnterFrame()
        {
            _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}
