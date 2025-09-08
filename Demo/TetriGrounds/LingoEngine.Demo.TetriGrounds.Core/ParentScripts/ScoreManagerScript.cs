using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Movies;
using LingoEngine.Texts;
using System.Collections.Generic;
#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 5_ScoreManager.ls
    public class ScoreManagerScript : LingoParentScript, IOverScreenTextParent
    {
        private int myPlayerScore;
        private int myLevel;
        private int myNumberLinesRemoved;
        private int myNumberLinesTot;
        private bool myLevelUp;
        private int myLevelUpNeededScore;
        private int myBlocksDroped;
        private readonly List<OverScreenTextScript> myOverScreenText = new();
        private readonly GlobalVars _global;

        public ScoreManagerScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
            myPlayerScore = 0;
            myNumberLinesTot = 0;
            myLevelUp = false;
            myBlocksDroped = 0;
            var txt = Member<LingoMemberText>("T_StartLevel");
            myLevel = txt != null && int.TryParse(txt.Text, out var lvl) ? lvl : 0;
            myLevelUpNeededScore = 20 * (myLevel + 1);
            UpdateGfxScore();
            NewText("Go!");
            Refresh();
        }

        public void Refresh()
        {
            switch (myNumberLinesRemoved)
            {
                case 1: LineRemoved1(); break;
                case 2: LineRemoved2(); break;
                case 3: LineRemoved3(); break;
                case 4: LineRemoved4(); break;
            }
            myNumberLinesRemoved = 0;
            // check fot level up (its the number of blocks droped)
            if (myBlocksDroped > myLevelUpNeededScore)
            {
                SendSprite<AnimationScriptBehavior>(22, x => x.StartAnim());
                myLevelUp = true;
                myLevel += 1;
                NewText($"Level {myLevel} !!");
                myLevelUpNeededScore += 20;
            }
            UpdateGfxScore();
            Member<LingoMemberText>("T_data")!.Text =  $"Level {myLevel}";
        }

        public void LineRemoved1() => myPlayerScore += 5 + myLevel;
        public void LineRemoved2() { NewText("2 Lines Removed!!"); myPlayerScore += 12 + myLevel; }
        public void LineRemoved3() { NewText("3 Lines Removed!!"); myPlayerScore += 20 + myLevel; }
        public void LineRemoved4() { NewText("Wooow, 4 Lines Removed!!"); myPlayerScore += 30 + myLevel; }

        public void AddDropedBlock() => myBlocksDroped += 1;
        public void LineRemoved() 
        { 
            myNumberLinesRemoved += 1; 
            myNumberLinesTot += 1; 
        }
        public void BlockFrozen() 
        { 
            myPlayerScore += 1; 
            Refresh(); 
        }
        public void UpdateGfxScore() => Member<LingoMemberText>("T_Score")!.Text = myPlayerScore.ToString();
        public bool GetLevelUp() 
        { 
            var t = myLevelUp; 
            myLevelUp = false; 
            return t; 
        }
        public void GameFinished() => NewText("You're Terminated....");
        public int GetLevel() => myLevel;
        public int GetScore() => myPlayerScore;
        // -----------------------------
        public void NewText(string text)
        {
            var o = new OverScreenTextScript(_env, _global,130, text, this);
            myOverScreenText.Add(o);
        }

        public void TextFinished(OverScreenTextScript obj)
        {
            myOverScreenText.Remove(obj);
            obj.Destroy();
        }

        public void DestroyOverScreenTxt()
        {
            foreach (var o in myOverScreenText.ToArray())
            {
                o.Destroy();
            }
            myOverScreenText.Clear();
        }
        // -----------------------------
        public void Destroy() => DestroyOverScreenTxt();
    }
}
