// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using AbstUI.Inputs;
using AbstUI.Primitives;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;
using System.Xml.Linq;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class EnterHighScoreBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent
    {
        private readonly GlobalVars _global;
        private ILingoMemberTextBase? _inputText;
        private ILingoMemberTextBase? _popupTitle;
        private IAbstJoystickKeyboard? _keyboard;
        private IEnumerable<int> _spriteNums = [];
        private string _name = "";
        private Action<string>? _onNameEntered;

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
        public string GetName() => _name;

        public void Show(Action<string> onNameEntered)
        {
            _onNameEntered = onNameEntered;
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
            _name = "";
            _inputText.Text = "";
            _popupTitle.Text = "New highscore! Type your name:";
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = true;
        }

        private void EnterPressed()
        {
            if (_keyboard == null) return;
            _name = _keyboard.Text;
            _global.PlayerName = _keyboard.Text;
            _keyboard.Close();
            _onNameEntered?.Invoke(_name);
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
