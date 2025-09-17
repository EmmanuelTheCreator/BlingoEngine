// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    public interface IHasBlingoMessage : IBlingoSpriteBehavior
    {
        void HandleMessage(string myFunction, params object[]? parameters );
    }
    // Converted from 22_B_Execute.ls
    public class ExecuteBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasMouseEnterEvent, IHasMouseLeaveEvent, IHasMouseDownEvent, IBlingoPropertyDescriptionList, IHasBlingoMessage
    {
        public string myFunction = "";
        public int mySpriteNum = 4;
        public int myVar1;
        public int myVar2;
        private bool myLock;
        public bool myEnableMouseClick = true;
        public bool myEnableMouseRollOver = true;
        public int myStartMember;
        public int myRollOverMember;
        public int myRollOverMemberCastLib;
        private readonly GlobalVars _globalVars;

        public ExecuteBehavior(IBlingoMovieEnvironment env, GlobalVars globalVars) : base(env)
        {
            _globalVars = globalVars;
        }

        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
        {
            return new BehaviorPropertyDescriptionList()
                .Add(this, x => x.myFunction, "Function:", "0")
                .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
                .Add(this, x => x.myVar1, "Var 1:", 0)
                .Add(this, x => x.myVar2, "Var 2:", 0)
                .Add(this, x => x.myEnableMouseClick, "Enable Mouseclick:", true)
                .Add(this, x => x.myEnableMouseRollOver, "Enable Mouse rollover:", true)
                .Add(this, x => x.myRollOverMember, "Rollover member:", 0)
                .Add(this, x => x.myRollOverMemberCastLib, "Rollover Castlib ('0' for auto):", 0)
            ;
        }
        public string? GetBehaviorDescription() => "Execute a function on mouse click";

        public string? GetBehaviorTooltip() => "Execute a function on mouse click";

        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;

        public void BeginSprite()
        {
            myLock = false;
            if (myRollOverMember == -1)
            {
                Me.Blend = 0;
            }
        }

        public void MouseEnter(BlingoMouseEvent mouse)
        {
            if (!myLock)
            {
                if (myEnableMouseRollOver && _globalVars.MousePointer != null)
                {
                    _globalVars.MousePointer.Mouse_Over();
                }
                if (myRollOverMember > 0)
                {
                    if (myRollOverMemberCastLib == 0)
                        Me.Member = Member(myRollOverMember);
                    else
                        Me.Member = Member(myRollOverMember, myRollOverMemberCastLib);
                }
                else if (myRollOverMember == -1)
                {
                    Me.Blend = 100;
                }
            }
        }

        public void MouseLeave(BlingoMouseEvent mouse)
        {
            if (!myLock)
            {
                if (myEnableMouseRollOver && _globalVars.MousePointer != null)
                {
                    _globalVars.MousePointer.Mouse_Restore();
                }
                if (myRollOverMember > 0)
                {
                    Me.Member = Member(myStartMember);
                }
                else if (myRollOverMember == -1)
                {
                    Me.Blend = 0;
                }
            }
        }

        public void MouseDown(BlingoMouseEvent mouse)
        {
            if (!myLock && myEnableMouseClick)
            {
                if (string.IsNullOrEmpty(myFunction)) return;
                SendSprite<IHasBlingoMessage>(mySpriteNum, s => s?.HandleMessage(myFunction, myVar1, myVar2));
            }
        }

        public void Lock() => myLock = true;
        public void UnLock() => myLock = false;

        public void HandleMessage(string myFunction, params object[]? parameters)
        {
            
        }
    }
}

