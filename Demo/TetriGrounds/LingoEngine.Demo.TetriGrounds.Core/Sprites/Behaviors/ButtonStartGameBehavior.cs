using LingoEngine.Inputs.Events;
using LingoEngine.Sprites;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
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
            if (_global.GameIsRunning)
                return;
            _Player.SoundPlayBtnStart();
            _Player.SoundPlayGo();


            SendSprite<BgScriptBehavior>(4, s => s.NewGame());
        }


    }
}
