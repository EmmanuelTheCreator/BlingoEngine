// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;
using LingoEngine.VerboseLanguage;
using System.ComponentModel;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 16_AppliBg.ls
    public class AppliBgBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasEndSpriteEvent, IOverScreenTextParent, IHasCounterStartData
    {
        private int _pos;
        private int lowest;
        private bool myCheckStartData;
        private bool myStop;
        private int myStartLines;
        private int myStartLevel;
        private int WebName;
        private readonly GlobalVars _globalVars;
        private List<OverScreenTextScript>? myOverScreenText;

        public AppliBgBehavior(ILingoMovieEnvironment env, GlobalVars globalVars) : base(env)
        {
            _globalVars = globalVars;
        }

        public void BeginSprite()
        {
            Member<ILingoMemberTextBase>("PlayerName")!.Text = WebName.ToString();
            //myHsDown = new script("score_get").New();
            //myHsDown.SetShowType(1);
            //myHsUp = script("score_save").New();
            //myGetStartData = script("StartData_get").New(this);
            //// myStop = true
            //myCheckStartData = true;
            //mySendStartData = script("StartData_save").New();
            //Cursor = 200;
            // myHsUp.postScore(member("PlayerName").text, 10000)
        }


        public void DataLoaded(string data, object obj)
        {
            this.Put(data).ToLog();
            if (data == "")
            {
                _Movie.GoTo(2);
            }
            else
            {
                myStartLevel = Convert.ToInt32(data.Line(1));
                myStartLines = Convert.ToInt32(data.Line(2));
                myStop = false;
            }
        }


        public void SendData(string _type, int? data)
        {
            if (data == null)
            {
                return;
            }
            if (_type == "StartLevel")
            {
                myStartLevel = data.Value;
            }
            if (_type == "StartLines")
            {
                myStartLines = data.Value;
            }
            //mySendStartData.Post(myStartLevel, myStartLines);
        }



        public void ExitFrame()
        {
            if (myCheckStartData)
            {
                if (myStop)
                    _Movie.GoTo(_Movie.CurrentFrame);
                else
                    _Movie.GoTo("Game");
            }
        }

        public void GameFinished(int _score)
        {
            RefeshHighScores();
            //myHsDown.SetShowType(2);
            // check if the score is higher
            //lowest = myHsDown.GetLowestPersonalScore();
            //if (_score > lowest)
            {
           //     myHsUp.PostScore(GetMember<ILingoMemberTextBase>("PlayerName").Text, _score);
            }
        }


        public void ReturnFromSaveScore(string data)
        {
            if (data.Contains("Highscore"))  // new highscore
            {
                NewText(data);
                RefeshHighScores();
            }
        }
        public void PersonalHighscores()
        {
            //myHsDown.SetShowType(2);
            //myHsDown.OutputScores();
        }
        public void ShowGeneralScores()
        {
            //myHsDown.SetShowType(1);
            //myHsDown.OutputScores();
        }
        public void RefeshHighScores() 
        { 
            //myHsDown.downloadScores();
        }
        public void NewText(string _text)
        {
            if (myOverScreenText == null)
                myOverScreenText = [];
            
            myOverScreenText.Add(new OverScreenTextScript(_env, _globalVars,130, _text, this));
        }



        public void TextFinished(OverScreenTextScript obj)
        {
            if (myOverScreenText == null) return;
            _pos = myOverScreenText.IndexOf(obj);
            myOverScreenText[_pos].Destroy();
            myOverScreenText.Remove(obj);
        }


        public void DestroyoverscreenTxt()
        {
            if (myOverScreenText == null) return;
            for (var i = 1; i <= myOverScreenText.Count; i++)
                myOverScreenText[i].Destroy();
            
            myOverScreenText = [];
        }

        public int GetCounterStartData(string _type)
        {
            if (_type == "StartLevel")
            {
                return myStartLevel;
            }
            if (_type == "StartLines")
            {
                return myStartLines;
            }
            return 0;
        }

        public void EndSprite()
        {
           // myHsDown.Destroy();
            DestroyoverscreenTxt();
        }



    }
}
