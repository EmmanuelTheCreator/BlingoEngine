using System.Numerics;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Inputs.Events;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 12_B_FontRollOver.ls
    public class FontRollOverBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasMouseDownEvent, IHasMouseWithinEvent, IHasMouseExitEvent
    {
        public Vector3 myColor = new(0,0,0);
        public Vector3 myColorOver = new(100,0,0);
        public Vector3 myColorLock = new(150,150,150);
        public bool myLock;
        public string myFunction = "";
        public int mySpriteNum = 4;
        public int myVar1;
        public int myVar2;

        public FontRollOverBehavior(ILingoMovieEnvironment env) : base(env) {}

        public void BeginSprite()
        {
            // Color handling is not implemented in the demo engine yet
        }

        public void MouseDown(LingoMouseEvent mouse)
        {
            if (!myLock)
            {
                if (string.IsNullOrEmpty(myFunction)) return;
                SendSprite(mySpriteNum, s => ((IHasLingoMessage)s)?.HandleMessage(myFunction, myVar1, myVar2));
            }
        }

        public void MouseWithin(LingoMouseEvent mouse)
        {
            if (!myLock)
            {
                Cursor = 280;
            }
        }

        public void MouseExit(LingoMouseEvent mouse)
        {
            if (!myLock)
            {
                Cursor = -1;
            }
        }

        public void Lock()
        {
            myLock = true;
        }

        public void Unlock()
        {
            myLock = false;
        }
    }
}
