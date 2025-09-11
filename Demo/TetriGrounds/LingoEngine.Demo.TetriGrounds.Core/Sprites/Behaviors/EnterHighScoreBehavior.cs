using LingoEngine.Sprites;
using LingoEngine.Movies;
using AbstUI.Primitives;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;
using AbstUI.Inputs;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class EnterHighScoreBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent
    {
        private readonly GlobalVars _global;
        private ILingoMemberTextBase? _inputText;
        private ILingoMemberTextBase? _popupTitle;
        private IAbstJoystickKeyboard? _keyboard;
        private IEnumerable<int> _spriteNums;

        public EnterHighScoreBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void BeginSprite()
        {
            _inputText = Member<ILingoMemberTextBase>("T_InputText")!;
            _popupTitle = Member<ILingoMemberTextBase>("T_PopupTitle")!;
            _inputText.Text = "";
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = false;
        }
        public void EndSprite()
        {
            if (_keyboard != null)
            {
                _keyboard.Close();
                _keyboard = null!;
            }
        }

        public void Show()
        {
            if (_inputText == null || _popupTitle == null || _inputText ==null) return;
            _keyboard = CreateJoystickKeyboard(c => c.Title = "Type your name", AbstJoystickKeyboard.KeyboardLayoutType.Azerty, false, new APoint(170, 310));
            _keyboard.BackgroundColor = AColor.FromHex("#42432dAA");
            _keyboard.TextColor = AColor.FromHex("#c5c528");
            _keyboard.SelectedBackgroundColor = AColor.FromHex("#37362a");
            _keyboard.UpdateStyle();
            _keyboard.MaxLength = 15;
            _keyboard.KeySelected += KeySelected;
            _keyboard.Closed += Closed;
            _keyboard.EnterPressed += EnterPressed;
            
            _popupTitle.Text = "New highscore! Type your name:";
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = true;
        }

        private void EnterPressed()
        {
            if (_keyboard == null) return;
            _keyboard.Close();
        }

        private void Closed()
        {
            if (_keyboard == null) return;
            //_keyboard.Close();
            _keyboard.KeySelected -= KeySelected;
            _keyboard.Closed -= Closed;
            _keyboard.EnterPressed -= EnterPressed;
            _keyboard = null;
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = false;
        }

        private void KeySelected(string chara)
        {
            if (_inputText == null || _keyboard == null) return;
            _inputText.Text = _keyboard.Text;
            
            //_keyboard.UpdateStyle();
        }

        internal void SetSpriteNums(IEnumerable<int> spritenums) => _spriteNums = spritenums;  
    }
}
