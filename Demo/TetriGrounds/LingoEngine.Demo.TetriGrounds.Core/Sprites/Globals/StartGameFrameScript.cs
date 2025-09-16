// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Bitmaps;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals
{
    public class StartGameBehavior : LingoSpriteBehavior, IHasExitFrameEvent, IHasBeginSpriteEvent
    {
        private GlobalVars _global;
        private LingoMemberBitmap? _memberBg;
        public StartGameBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void BeginSprite()
        {
            _memberBg = Member<LingoMemberBitmap>("MyBG");
        }

        public void ExitFrame()
        {



        }
    }
}
