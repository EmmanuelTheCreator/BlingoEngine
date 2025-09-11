using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using LingoEngine.VerboseLanguage;
using static System.Runtime.InteropServices.JavaScript.JSType;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 2_Bg Script.ls
    public class BgScriptBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasEndSpriteEvent
    {
        private PlayerBlockScript? myPlayerBlock;
        private GfxScript? myGfx;
        private BlocksScript? myBlocks;
        private ScoreManagerScript? myScoreManager;
        private readonly GlobalVars _global;

        private int myWidth;
        private int myHeight;
        public BgScriptBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void BeginSprite()
        {
            if (myPlayerBlock != null)
                myPlayerBlock.HidePause();
            _Player.SoundPlayGong();
            
        }

        public void ExitFrame()
        {
        }

        public void ActionKey(object val)
        {
            // debug output
            this.Put(val).ToLog();
        }

        public void KeyAction(int val, int val2)
        {
            if (myPlayerBlock == null) return;
            myPlayerBlock.Keyyed(val);
        }

        public void PauseGame() => myPlayerBlock?.PauseGame();

        public void NewGame()
        {
            if (myPlayerBlock != null)
            {
                var _pause = myPlayerBlock.GetPause();
                if(_pause==false) { 
                    TeminateGame(); 
                    StartNewGame(); 
                }
            }
            else
            {
                TeminateGame();
                StartNewGame();
            }
        }

        public void EndSprite()
        {
            TeminateGame();
        }

        public void StartNewGame()
        {
            _global.GameIsRunning = true;
            if (_global.SpriteManager == null)
            {
                _global.SpriteManager = new SpriteManager(_env);
                _global.SpriteManager.Init(100);
            }
            myWidth = 11;
            myHeight = 22;
            myGfx = new GfxScript(_env);
            myScoreManager = new ScoreManagerScript(_env, _global);
            myBlocks = new BlocksScript(_env, _global, myGfx, myScoreManager, myWidth, myHeight);
            myPlayerBlock = new PlayerBlockScript(_env, _global, myGfx, myBlocks, myScoreManager, myWidth, myHeight);
            myPlayerBlock.CreateBlock();
        }

        public void TeminateGame()
        {
            _global.GameIsRunning = false;
            myPlayerBlock?.Destroy();
            myBlocks?.Destroy();
            myGfx?.Destroy();
            myScoreManager?.Destroy();
            myPlayerBlock = null;
            myBlocks = null;
            myGfx = null;
            myScoreManager = null;
        }

        public void SpaceBar() => myPlayerBlock?.LetBlockFall();

       


       
    }
}
