// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Inputs.Events;
using LingoEngine.Movies;
using LingoEngine.Sprites;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 13_B_NewGame.ls
    public class NewGameBehavior : LingoSpriteBehavior, IHasMouseUpEvent, IHasMouseWithinEvent, IHasMouseLeaveEvent
    {
        public NewGameBehavior(ILingoMovieEnvironment env) : base(env) {}

        public void MouseUp(LingoMouseEvent mouse)
        {
            Cursor = -1;
            _Movie.GoTo("Game");
        }

        public void MouseWithin(LingoMouseEvent mouse) => Cursor = 280;
        public void MouseLeave(LingoMouseEvent mouse) => Cursor = -1;
    }
}
