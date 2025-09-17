// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Inputs.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Events;
using BlingoEngine.Movies;
using BlingoEngine.Demo.TetriGrounds.Core.ParentScripts;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class ButtonStartGameBehavior : BlingoSpriteBehavior, IHasMouseDownEvent, IHasKeyDownEvent
    {
        private readonly GlobalVars _global;

        public ButtonStartGameBehavior(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void MouseDown(BlingoMouseEvent mouse)
        {
            StartNewGame();
        }
        public void KeyDown(BlingoKeyEvent key)
        {
            //var code = key.Key;
            //Console.WriteLine($"Key Down: {code}: {key.KeyCode} ");
            if (key.Key == "Enter")
                StartNewGame(); 
        }

        private void StartNewGame()
        {
            //SendSprite<EnterHighScoreBehavior>(38, x => x.Show(t => { }));
            //return;
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

