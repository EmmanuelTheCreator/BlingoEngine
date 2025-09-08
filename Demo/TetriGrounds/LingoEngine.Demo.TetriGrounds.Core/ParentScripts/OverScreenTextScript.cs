using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    public interface IOverScreenTextParent
    {
        void TextFinished(OverScreenTextScript text);
    }
    // Converted from 6_OverScreenText.ls
    public class OverScreenTextScript : LingoParentScript, IHasStepFrameEvent
    {
        private readonly GlobalVars _global;
        private int myNum = 48;
        private int myNum2 = 49;
        private int myCounter = 0;

        public int Duration { get; set; } = 130;
        public string Text { get; set; } = string.Empty;
        public int LocV { get; set; } = 100;
        public IOverScreenTextParent? Parent { get; set; }

        public OverScreenTextScript(ILingoMovieEnvironment env, GlobalVars global, int duration, string text, IOverScreenTextParent parent) : base(env)
        {
            Duration = duration;
            Text = text;
            _global = global;
            _Movie.ActorList.Add(this);
            Parent = parent;
            if (Sprite(myNum).Puppet)
            {
                myNum = 46;
                myNum = 47;
                LocV = 130;
            }
            Sprite(myNum).Puppet = true;
            Sprite(myNum2).Puppet = true;
            Sprite(myNum).SetMember("T_OverScreen");
            Sprite(myNum2).SetMember("T_OverScreen2");
            Sprite(myNum).LocZ = 1000;
            Sprite(myNum2).LocZ = 999;
            Sprite(myNum).Ink = 36;
            Sprite(myNum2).Ink = 36;
            Member<LingoMemberText>("T_OverScreen")!.Text = Text;
            Member<LingoMemberText>("T_OverScreen2")!.Text = Text;
        }

        

        public void StepFrame()
        {
            myCounter += 1;
            if (myCounter > Duration)
            {
                _Movie.ActorList.Remove(this);
                Sprite(myNum).Puppet = false;
                Sprite(myNum2).Puppet = false;
                Parent?.TextFinished(this);
                return;
            }
            LocV += 2;
            Sprite(myNum).LocH = 180;
            Sprite(myNum2).LocH = 182;
            Sprite(myNum).LocV = LocV;
            Sprite(myNum2).LocV = LocV + 2;
            float blend = 70f - (float)myCounter / Duration * 100f;
            if (blend < 0) blend = 0;
            Sprite(myNum).Blend = (int)blend;
            Sprite(myNum2).Blend = (int)blend;
           
        }

        public void Destroy()
        {
            if (_Movie.ActorList.GetPos(this) > 0) return;
            _Movie.ActorList.Remove(this);
            Sprite(myNum).Puppet = false;
            Sprite(myNum2).Puppet = false;
        }
    }
}
