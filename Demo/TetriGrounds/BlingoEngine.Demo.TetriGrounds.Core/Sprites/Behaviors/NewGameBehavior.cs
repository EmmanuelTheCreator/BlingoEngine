// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 13_B_NewGame.ls
    public class NewGameBehavior : BlingoSpriteBehavior, IHasMouseUpEvent, IHasMouseWithinEvent, IHasMouseLeaveEvent
    {
        public NewGameBehavior(IBlingoMovieEnvironment env) : base(env) {}

        public void MouseUp(BlingoMouseEvent mouse)
        {
            Cursor = -1;
            _Movie.GoTo("Game");
        }

        public void MouseWithin(BlingoMouseEvent mouse) => Cursor = 280;
        public void MouseLeave(BlingoMouseEvent mouse) => Cursor = -1;
    }
}

