using LingoEngine.Inputs.Events;
using LingoEngine.Sprites;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using AbstUI.Primitives;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class ButtonStartGameBehavior : LingoSpriteBehavior, IHasMouseDownEvent, IHasKeyDownEvent
    {
        private readonly GlobalVars _global;

        public ButtonStartGameBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

       

        public void MouseDown(LingoMouseEvent mouse)
        {
            StartNewGame();
        }
        public void KeyDown(LingoKeyEvent key)
        {
            //var code = key.Key;
            //Console.WriteLine($"Key Down: {code}: {key.KeyCode} ");
            if (key.Key == "Enter")
                StartNewGame(); 
        }

        private void StartNewGame()
        {
           // var keyboard = CreateJoystickKeyboard(c => c.Title = "Type your name",AbstUI.Inputs.AbstJoystickKeyboard.KeyboardLayoutType.Azerty,true,new APoint(40,30));
            
           //// keyboard.Open();
           // return;
            if (_global.GameIsRunning)
                return;
            _Player.SoundPlayBtnStart();
            _Player.RunDelayed(_Player.SoundPlayGo, 100);
            
            _Player.SoundStopNature();
            Sprite(9).Visibility = false; // hide start button
            Sprite(11).Visibility = false; // hide start button
            _global.MousePointer!.HideMouse();
            SendSprite<BgScriptBehavior>(4, s => s.NewGame());
        }


    }
}
