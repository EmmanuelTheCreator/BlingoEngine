using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Movies;
using LingoEngine.Texts;
using System;
using System.Collections.Generic;
#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 5_ScoreManager.ls
    public class ScoreManagerScript : LingoParentScript, IOverScreenTextParent
    {
        private readonly LingoMemberText _memberScore;
        private readonly LingoMemberText _memberTData;
        private readonly List<OverScreenTextScript> myOverScreenText = new();
        private readonly GlobalVars _global;
        private int myPlayerScore;
        private int myLevel;
        private int myNumberLinesRemoved;
        private int myNumberLinesTot;
        private bool myLevelUp;
        private int myLevelUpNeededScore;
        private int myBlocksDroped;
        private DateTime myLastLineClear = DateTime.MinValue;
        private readonly TimeSpan myComboDuration = TimeSpan.FromSeconds(2);

        public ScoreManagerScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
            myPlayerScore = 0;
            myNumberLinesTot = 0;
            myLevelUp = false;
            myBlocksDroped = 0;
            var txt = Member<LingoMemberText>("T_StartLevel");
            _memberScore = Member<LingoMemberText>("T_Score")!;
            _memberTData = Member<LingoMemberText>("T_data")!;
            myLevel = txt != null && int.TryParse(txt.Text, out var lvl) ? lvl : 1;
            myLevelUpNeededScore = 200 * (myLevel + 1);
            UpdateGfxScore();
            NewText("Go!");
            Refresh();
        }

        public void Refresh()
        {
            var linesThisTurn = myNumberLinesRemoved;
            switch (linesThisTurn)
            {
                case 1: LineRemoved1(); break;
                case 2: LineRemoved2(); break;
                case 3: LineRemoved3(); break;
                case 4: LineRemoved4(); break;
            }
            if (linesThisTurn > 0)
            {
                var now = DateTime.UtcNow;
                if (now - myLastLineClear <= myComboDuration)
                {
                    myPlayerScore += 20 * myLevel * linesThisTurn;
                }
                myLastLineClear = now;
            }
            else
            {
                myLastLineClear = DateTime.MinValue;
            }
            myNumberLinesRemoved = 0;
            // check for level up (its the number of blocks droped)
            if (myBlocksDroped > myLevelUpNeededScore)
            {
                SendSprite<AnimationScriptBehavior>(22, x => x.StartAnim());
                myLevelUp = true;
                myLevel += 1;
                NewText($"Level {myLevel} !!");
                myLevelUpNeededScore += 2000;
                myPlayerScore += 200 * myLevel;
            }
            UpdateGfxScore();
            _memberTData.Text = $"Level {myLevel}";
        }

        public void LineRemoved1() => myPlayerScore += 80 * myLevel;
        public void LineRemoved2()
        {
            _Player.SoundPlayRowsDeleted(2);
            NewText("2 Lines Removed!!"); myPlayerScore += 120 * myLevel;
        }
        public void LineRemoved3()
        {
            _Player.SoundPlayRowsDeleted(3);
            NewText("3 Lines Removed!!"); myPlayerScore += 180 * myLevel;
        }
        public void LineRemoved4()
        {
            _Player.SoundPlayRowsDeleted(4);
            NewText("Wooow, 4 Lines Removed!!"); myPlayerScore += 320 * myLevel;
        }

        public void AddDropedBlock(bool hardDrop) => myBlocksDroped += hardDrop ? 4 : 0;
        public void LineRemoved()
        {
            myNumberLinesRemoved += 1;
            myNumberLinesTot += 1;
        }
        public void BlockFrozen()
        {
            myPlayerScore += 4;
            Refresh();
        }
        public void UpdateGfxScore() => _memberScore.Text = myPlayerScore.ToString();
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
            var o = new OverScreenTextScript(_env, _global, 130, text, this);
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
