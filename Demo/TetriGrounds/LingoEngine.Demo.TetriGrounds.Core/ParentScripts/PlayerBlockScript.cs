using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using System.Collections.Generic;
using System.Reflection.Emit;
#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 8_PlayerBlock.ls (simplified)
    public class PlayerBlockScript : LingoParentScript, IHasStepFrameEvent
    {
        private readonly ILingoMovieEnvironment env;
        private readonly GlobalVars _global;
        private readonly GfxScript myGfx;
        private readonly BlocksScript myBlocks;
        private readonly ScoreManagerScript myScoreManager;
        private readonly List<Dictionary<string, object>> MySubBlocks = new();
        private readonly List<Dictionary<string, object>> MyNextBlocks = new();
        private readonly List<object[]> myTypeBlocks = new();
        private int myX;
        private int myY;
        private int myWidth;
        private int myMaxX;
        private int myMaxY;
        private bool myPause;
        private bool myMoving;
        private int _level;
        private int mySlowDown = 65;
        private int myWaiter;
        private int myKeyBoardWaiter;
        private int myKeyBoardTot = 11;
        private int mySlowDownFactorByLevel = 10;
        private bool myFinished;
        private int myBlockType;
        private int myNextBlockType = 1;
        private int MyNextBlockHor = 16;
        private int MyNextBlockVer = 13;
        private bool myDownPressed;
        private bool myStopKeyAction;
        private int myLastKey;

        public PlayerBlockScript(ILingoMovieEnvironment env, GlobalVars global, GfxScript gfx, BlocksScript blocks, ScoreManagerScript score, int width, int height) : base(env)
        {
            this.env = env;
            _global = global;
            myGfx = gfx;
            myBlocks = blocks;
            myScoreManager = score;
            myMaxX = width;
            myMaxY = height;
           
            AddTypeBlock(new int[,] { { 0,0 }, { -1,0 }, { 1,0 }, { 0,-1 } }, true);    // 'T' Block, true-false = rotate(else its flip axis)
            AddTypeBlock(new int[,] { { 0,0 }, { -1,0 }, { -2,0 }, { 1,0 } }, true);    // '-' Block
            AddTypeBlock(new int[,] { { -1,-1 }, { 0,-1 }, { 0,0 }, { 1,0 } }, false);  // 'z' Block
            AddTypeBlock(new int[,] { { 1,-1 }, { 0,-1 }, { 0,0 }, { -1,0 } }, false);  // 'z' Block invers
            AddTypeBlock(new int[,] { { 0,-1 }, { 1,-1 }, { 0,0 }, { 1,0 } }, false);   // '#' Block Cubus
            AddTypeBlock(new int[,] { { -1,-1 }, { -1,0 }, { 0,0 }, { 1,0 } }, true);   // 'L' Block 
            AddTypeBlock(new int[,] { { 1,-1 }, { -1,0 }, { 0,0 }, { 1,0 } }, true);    // 'L' Block invers
            myX = myMaxX / 2;
            myY = 2;
            myWidth = 1;
            myMoving = false;
            _level = myScoreManager.GetLevel();

            CalculateSpeed();
            if (mySlowDown <= 1) mySlowDown = 1;
            myWaiter = 0;
            myKeyBoardWaiter = 0;
            myKeyBoardTot = 11;
            myNextBlockType = 1;
            MyNextBlockHor = 16;
            MyNextBlockVer = 13;
            mySlowDownFactorByLevel = 10;
            UpdateNextBlock();
            StartMove();
            myStopKeyAction = false;
        }

        private void CalculateSpeed()
        {
            _level = myScoreManager.GetLevel();
            mySlowDownFactorByLevel = 10;
            mySlowDown = 65;
            for (int i = 0; i <= _level; i++)
            {
                mySlowDownFactorByLevel -= 1;
                if (mySlowDownFactorByLevel <= 3) mySlowDownFactorByLevel = 2;
                if (mySlowDown > 1) mySlowDown -= mySlowDownFactorByLevel;
            }
            if (mySlowDown <= 1) mySlowDown = 1;
        }

        public void Keyyed(int val)
        {
            if (myPause) return;
            myKeyBoardWaiter = 0;
            myLastKey = val;
            MoveBlock(val);
        }

        public void PauseGame()
        {
            if (myPause)
            {
                Sprite(35).Blend = 0;
                myPause = false;
                Sprite(35).LocZ = 35;
            }
            else
            {
                Sprite(35).Blend = 100;
                Sprite(35).LocZ = 1010;
                myPause = true;
            }
        }

        private void MoveBlock(int val)
        {
            if (myFinished) return;
            // Right
            if (val == 2) { 
                Rightt(); 
                myDownPressed = false; 
            }
            else if (val == 4) 
            { 
                Leftt(); 
                myDownPressed = false; 
            }
            else if (val == 3)
            {
                if (!myStopKeyAction) 
                    // Down
                    myDownPressed = true; 
                else 
                    myDownPressed = false;
            }
            else if (val == 9) 
            { 
                // nothing
                myDownPressed = false; 
                myStopKeyAction = false; 
            }
            else if (val == 1) 
            { 
                // up
                TurnBlock(); 
                myDownPressed = false; 
            }
        }

        public void LetBlockFall()
        {
            int starting = myY;
            for (int i = starting; i <= myMaxY; i++)
            {
                bool test = DownCheck();
                RefreshBlock();
                if (!test) break;
            }
        }

        public void StepFrame()
        {
            if (myPause) return;
            if (myLastKey <= 4 && myLastKey != 1)
            {
                if (myKeyBoardWaiter > myKeyBoardTot)
                {
                    myKeyBoardWaiter = 0;
                    MoveBlock(myLastKey);
                }
                myKeyBoardWaiter += 1;
            }
            int addon = myDownPressed ? mySlowDown : 0;
            if (myWaiter + addon > mySlowDown)
            {
                myWaiter = 0;
                DownCheck();
                RefreshBlock();
            }
            else
            {
                myWaiter += 1;
            }
        }

        private bool DownCheck()
        {
            bool check = CollitionDetect(myX, myY + 1);
            if (check)
            {
                FreezeBlock();
                ResetBlock();
                return false;
            }
            else
            {
                myY += 1;
                return true;
            }
        }

        private void ResetBlock()
        {
            if (myFinished)
                return;
            
            foreach (var i in MySubBlocks)
            {
                int y = (int)i["yy"] + myY;
                if (myBlocks.FullHorizontal(y))
                    myBlocks.RemoveHorizontal(y);
            }
            myStopKeyAction = true;
            DestroyBlock();
            CreateBlock();
            myY = 2;
            myX = myMaxX / 2;
            myScoreManager.BlockFrozen();// add score when you freeze a block
            myScoreManager.AddDropedBlock(); // add that there's a block dropped

            // check if we go a level up
            if (myScoreManager.GetLevelUp())
                CalculateSpeed();
            if (CollitionDetect(myX, myY))
            {
                myScoreManager.GameFinished();
                myFinished = true;
                myBlocks.FinishedBlocks();
                StopMove();
                _global.GameIsRunning = false;
                //SendSprite<AppliBgBehavior>(1, s => s.GameFinished(myScoreManager.GetScore()));
            }
        }

        private void FreezeBlock()
        {
            if (myFinished)
                return;
            
            foreach (var i in MySubBlocks)
            {
                myBlocks.NewBlock(myX + (int)i["xx"], myY + (int)i["yy"], myBlockType);
            }
        }

        private bool CollitionDetect(int x, int y)
        {
            foreach (var i in MySubBlocks)
            {
                if (myBlocks.IsBlock(x + (int)i["xx"], y + (int)i["yy"]))
                    return true;
            }
            return false;
        }

        private void TurnBlock()
        {
            myWaiter += 1;
            int offsetX = 0;
            var tempBlock = new List<(int x,int y)>();
            bool coll = false;
            foreach (var i in MySubBlocks)
            {
                int oldx = (int)i["xx"];
                int oldy = (int)i["yy"];
                int newy = oldx;
                int newx = -oldy;
                tempBlock.Add((newx, newy));
                if (myX + newx > myMaxX - 1) coll = true;
                if (myBlocks.IsBlock(myX + newx + offsetX, myY + newy)) coll = true;
            }
            if (!coll)
            {
                for (int idx = 0; idx < MySubBlocks.Count; idx++)
                {
                    MySubBlocks[idx]["xx"] = tempBlock[idx].x;
                    MySubBlocks[idx]["yy"] = tempBlock[idx].y;
                }
            }
            myX += offsetX;
            RefreshBlock();
        }
        // ----------------------------------------
        private void Rightt()
        {
            myWaiter += 1;
            if (!CollitionDetect(myX + 1, myY))
            {
                int maxright = 0;
                foreach (var i in MySubBlocks)
                {
                    int tempx = (int)i["xx"];
                    if (tempx > maxright) maxright = tempx;
                }
                if (myX + maxright + 1 < myMaxX)
                {
                    myX += 1;
                    RefreshBlock();
                }
            }
        }

        private void Leftt()
        {
            myWaiter += 1;
            if (!CollitionDetect(myX - 1, myY))
            {
                int maxleft = 0;
                foreach (var i in MySubBlocks)
                {
                    int tempx = (int)i["xx"];
                    if (tempx < maxleft) maxleft = tempx;
                }
                if (myX - 1 + maxleft > 0)
                {
                    myX -= 1;
                    RefreshBlock();
                }
            }
        }
        // ----------------------------------------
        private void StartMove()
        {
            _Movie.ActorList.Add(this);
            myMoving = true;
        }

        private void StopMove()
        {
            _Movie.ActorList.Remove(this);
            myMoving = false;
        }
        // ----------------------------------------
        private void RefreshBlock()
        {
            for (int i = 0; i < MySubBlocks.Count; i++)
            {
                var obj = (BlockScript)MySubBlocks[i]["obj"];
                myGfx.PositionBlock(obj.GetSpriteNum(), myX + (int)MySubBlocks[i]["xx"], myY + (int)MySubBlocks[i]["yy"]);
            }
        }
        // ----------------------------------------
        public void CreateBlock()
        {
            myBlockType = myNextBlockType;
            var chosen = (object[])myTypeBlocks[myBlockType - 1];
            // create subBlocks
            var coords = (List<(int x, int y)>)chosen[0];
            for (int i = 0; i < coords.Count; i++)
            {
                var dict = new Dictionary<string, object>();
                var b = new BlockScript(env, _global, myBlockType);
                b.CreateBlock();
                dict["obj"] = b;
                dict["xx"] = coords[i].x;
                dict["yy"] = coords[i].y;
                MySubBlocks.Add(dict);
            }
            RefreshBlock();
            UpdateNextBlock();
        }

        private void UpdateNextBlock()
        {
            DestroyNextBlock();
            myNextBlockType = Random(myTypeBlocks.Count);
            var chosen = (object[])myTypeBlocks[myNextBlockType - 1];
            var coords = (List<(int x, int y)>)chosen[0];
            foreach (var p in coords)
            {
                var dict = new Dictionary<string, object>();
                var b = new BlockScript(env, _global, myNextBlockType);
                b.CreateBlock();
                dict["obj"] = b;
                dict["xx"] = p.x;
                dict["yy"] = p.y;
                MyNextBlocks.Add(dict);
                myGfx.PositionBlock(b.GetSpriteNum(), MyNextBlockHor + p.x, MyNextBlockVer + p.y);
            }
        }

        private void DestroyNextBlock()
        {
            foreach (var d in MyNextBlocks)
            {
                ((BlockScript)d["obj"]).Destroy();
            }
            MyNextBlocks.Clear();
        }

        public bool GetPause() => myPause;

        private void DestroyBlock()
        {
            foreach (var d in MySubBlocks)
            {
                ((BlockScript)d["obj"]).Destroy();
            }
            MySubBlocks.Clear();
        }

        private void AddTypeBlock(int[,] coords, bool rotate)
        {
            var list = new List<(int x, int y)>();
            for (int i = 0; i < coords.GetLength(0); i++)
                list.Add((coords[i, 0], coords[i, 1]));
            myTypeBlocks.Add(new object[] { list, rotate });
        }

        public void Destroy()
        {
            DestroyNextBlock();
            DestroyBlock();
            StopMove();
        }
    }
}
