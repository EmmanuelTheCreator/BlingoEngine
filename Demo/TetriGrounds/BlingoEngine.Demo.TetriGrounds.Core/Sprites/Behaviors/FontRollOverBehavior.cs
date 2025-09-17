// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using System.Numerics;
using AbstUI.Primitives;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 12_B_FontRollOver.ls
    public class FontRollOverBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasMouseDownEvent, IHasMouseWithinEvent, IHasMouseExitEvent, IBlingoPropertyDescriptionList
    {
        public AColor myColor = new AColor(0, 0, 0);
        public AColor myColorOver = new AColor(100, 0, 0);
        public AColor myColorLock = new AColor(150, 150, 150);
        public bool myLock;
        public string myFunction = "";
        public int mySpriteNum = 4;
        public int myVar1;
        public int myVar2;

        public FontRollOverBehavior(IBlingoMovieEnvironment env) : base(env) {}


        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
        {
            return new BehaviorPropertyDescriptionList()
                .Add(this, x => x.myColor, "Color:", new AColor(0, 0, 0))
                .Add(this, x => x.myColorOver, "ColorOver:", new AColor(100, 0, 0))
                .Add(this, x => x.myColorLock, "ColorLock:", new AColor(150, 150, 150))
                .Add(this, x => x.myLock, "Start Locked:", false)
                .Add(this, x => x.myFunction, "Function:", "0")
                .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
                .Add(this, x => x.myVar1, "Var 1:", 0)
                .Add(this, x => x.myVar2, "Var 2:", 0)
            ;
        }
        public string? GetBehaviorDescription() => "Change color on rollover and execute a function on mouse click";

        public string? GetBehaviorTooltip() => "Change color on rollover and execute a function on mouse click";

        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => Sprite(spriteNum).Member is IBlingoMemberTextBase;

        public void BeginSprite()
        {
            var textMember = Sprite(Me.SpriteNum).Member as IBlingoMemberTextBase;
            if (textMember == null) return;
            if (myLock == true)
            {
                textMember.Color = myColorLock;
            }
            else
            {
                textMember.Color = myColor;
            }
        }

        public void MouseDown(BlingoMouseEvent mouse)
        {
            if (!myLock)
            {
                if (string.IsNullOrEmpty(myFunction)) return;
                SendSprite(mySpriteNum, s => ((IHasBlingoMessage)s)?.HandleMessage(myFunction, myVar1, myVar2));
            }
        }

        public void MouseWithin(BlingoMouseEvent mouse)
        {
            var textMember = Sprite(Me.SpriteNum).Member as IBlingoMemberTextBase;
            if (textMember == null) return;
            if (!myLock)
            {
                if (textMember.Color != myColorOver)
                {
                    textMember.Color = myColorOver;
                    Cursor = 280;
                }
            }
        }

        public void MouseExit(BlingoMouseEvent mouse)
        {
            var textMember = Sprite(Me.SpriteNum).Member as IBlingoMemberTextBase;
            if (textMember == null) return;
            if (!myLock)
            {
                textMember.Color = myColor;
                Cursor = -1;
            }
        }

        public void Lock()
        {
            myLock = true;
            var textMember = Sprite(Me.SpriteNum).Member as IBlingoMemberTextBase;
            if (textMember == null) return;
            textMember.Color = myColorLock;
        }

        public void Unlock()
        {
            myLock = false;
            var textMember = Sprite(Me.SpriteNum).Member as IBlingoMemberTextBase;
            if (textMember == null) return;
            textMember.Color = myColor;
        }

       
    }
}

