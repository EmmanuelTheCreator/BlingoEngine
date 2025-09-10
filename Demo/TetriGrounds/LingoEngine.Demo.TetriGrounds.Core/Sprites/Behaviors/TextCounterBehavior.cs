using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
   

    // Converted from 23_TextCounter.ls
    public class TextCounterBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, ILingoPropertyDescriptionList
    {
        public int myMax = 10;
        public int myMin = 0;
        public int myValue = -1;
        public int myStep = 1;
        public int myDataSpriteNum = 1;
        public string myDataName = "";
        public int myWaitbeforeExecute = 70;
        public string myFunction = "";

        private int myWaiter;
        private LingoMemberText? _textMember;

        public TextCounterBehavior(ILingoMovieEnvironment env) : base(env){}

        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
        {
            return new BehaviorPropertyDescriptionList()
                .Add(this, x => x.myMin, "Min Value:", 0)
                .Add(this, x => x.myMax, "Max Value:", 10)
                .Add(this, x => x.myValue, "My Start Value:", -1)
                .Add(this, x => x.myStep, "My step:", 1)
                .Add(this, x => x.myDataSpriteNum, "My Sprite that contains info\n(set value to -1):", 1)
                .Add(this, x => x.myDataName, "Name Info:", "1")
                .Add(this, x => x.myWaitbeforeExecute, "WaitTime before execute:", 70)
                .Add(this, x => x.myFunction, "function to execute:", "70");
        }

        public string? GetBehaviorDescription() => "A simple counter that can be increased or decreased with limits.";


        public string? GetBehaviorTooltip() => "A simple counter that can be increased or decreased with limits.";

        public bool IsOKToAttach(LingoSymbol spriteType, int spriteNum) => true;


        public void BeginSprite()
        {
            _textMember = Me.Member as LingoMemberText;
            if (myValue == -1)
            {
                myValue = SendSprite<IHasCounterStartData, int>(myDataSpriteNum, c => c.GetCounterStartData(myDataName));
                if (myValue <= 0) myValue = 0;
                if (myValue < myMin || myValue > myMax) myValue = 0;
            }
            UpdateMe();
            myWaiter = myWaitbeforeExecute;
        }

        public void ExitFrame()
        {
            if (myWaiter < myWaitbeforeExecute)
            {
                if (myWaiter == myWaitbeforeExecute - 1)
                    SendSprite(myDataSpriteNum, s => ((IHasLingoMessage)s)?.HandleMessage(myFunction, myDataName, myValue));
                myWaiter++;
            }
        }

        public void Addd()
        {
            if (myValue < myMax)
            {
                myValue += myStep;
                UpdateMe();
            }
        }

        public void Deletee()
        {
            if (myValue > myMin)
            {
                myValue -= myStep;
                UpdateMe();
            }
        }

        private void UpdateMe()
        {
            if (_textMember != null)
                _textMember.Text = myValue.ToString();
            myWaiter = 0;
        }

      
    }
}
