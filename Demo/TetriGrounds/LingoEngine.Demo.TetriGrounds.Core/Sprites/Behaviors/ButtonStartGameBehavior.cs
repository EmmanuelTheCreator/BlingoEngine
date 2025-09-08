using LingoEngine.Inputs.Events;
using LingoEngine.Sprites;
using LingoEngine.Events;
using LingoEngine.Movies;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class ButtonStartGameBehavior : LingoSpriteBehavior, IHasMouseDownEvent
    {
        public ButtonStartGameBehavior(ILingoMovieEnvironment env) : base(env)
        {
        }

        public void MouseDown(LingoMouseEvent mouse)
        {
            SendSprite<BgScriptBehavior>(4, s => s.NewGame());
        }
    }
}
