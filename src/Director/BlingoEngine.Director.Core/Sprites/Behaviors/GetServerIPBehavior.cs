using AbstUI.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;

namespace BlingoEngine.Director.Core.Sprites.Behaviors
{
    /// <summary>
    /// Behavior that creates an input text member and waits for the user to press Enter.
    /// </summary>
    public class GetServerIPBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent
    {
        private BlingoMemberField? _inputField;

        public GetServerIPBehavior(IBlingoMovieEnvironment env) : base(env) { }

        public void BeginSprite()
        {
            _inputField = _Movie.New.Member<BlingoMemberField>();
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

