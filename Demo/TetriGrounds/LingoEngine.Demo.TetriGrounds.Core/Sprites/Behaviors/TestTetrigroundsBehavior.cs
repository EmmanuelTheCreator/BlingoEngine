using LingoEngine.Sprites;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class TestTetrigroundsBehavior : LingoSpriteBehavior, IHasExitFrameEvent, IHasBeginSpriteEvent
    {
        private readonly GlobalVars _global;
        private int _counter;
        private int _counterText;
        private ILingoMemberTextBase? _member;

        public TestTetrigroundsBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }
        public void BeginSprite()
        {
            _member = Member<ILingoMemberTextBase>("T_data");
        }

        public void ExitFrame()
        {
            _counter++;
            if (_counter == 50)
            {
                _counter = 0;
                _counterText += 30;
                _member.FontStyle = LingoTextStyle.Italic;
                _member!.Text = _counterText.ToString();

            }
            _Movie.GoTo(_Movie.CurrentFrame);
        }

     
    }
}
