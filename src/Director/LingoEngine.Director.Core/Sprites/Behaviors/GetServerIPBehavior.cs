using AbstUI.Inputs;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;

namespace LingoEngine.Director.Core.Sprites.Behaviors
{
    /// <summary>
    /// Behavior that creates an input text member and waits for the user to press Enter.
    /// </summary>
    public class GetServerIPBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent
    {
        private LingoMemberField? _inputField;

        public GetServerIPBehavior(ILingoMovieEnvironment env) : base(env) { }

        public void BeginSprite()
        {
            _inputField = _Movie.New.Member<LingoMemberField>();
            Me.SetMember(_inputField);
        }

        public void ExitFrame()
        {
            if (!_Key.KeyPressed(AbstUIKeyType.ENTER) && !_Key.KeyPressed(AbstUIKeyType.RETURN))
            {
                _Movie.GoTo(_Movie.CurrentFrame);
            }
        }
    }
}
