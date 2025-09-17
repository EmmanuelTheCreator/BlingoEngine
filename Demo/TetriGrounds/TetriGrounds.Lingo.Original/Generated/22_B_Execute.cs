using System;
using LingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class ExecuteBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList, IHasMouseLeaveEvent, IHasMouseDownEvent, IHasMouseEnterEvent, IHasBeginSpriteEvent
{
    public string myFunction = "0";
    public int mySpriteNum = 4;
    public int myVar1 = 0;
    public int myVar2 = 0;
    public bool myLock;
    public bool myEnableMouseClick = true;
    public bool myEnableMouseRollOver = true;
    public object myStartMember;
    public string myRollOverMember = "0";
    public string myRollOverMemberCastLib = "0";

    public ExecuteBehavior(ILingoMovieEnvironment env) : base(env) { }

    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
    {
        return new BehaviorPropertyDescriptionList()
            .Add(this, x => x.myFunction, "Function:", "0")
            .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
            .Add(this, x => x.myVar1, "Var 1:", 0)
            .Add(this, x => x.myVar2, "Var 2:", 0)
            .Add(this, x => x.myEnableMouseClick, "Enable Mouseclick:", true)
            .Add(this, x => x.myEnableMouseRollOver, "Enable Mouse rollover:", true)
            .Add(this, x => x.myRollOverMember, "Rollover member:", "0")
            .Add(this, x => x.myRollOverMemberCastLib, "Rollover Castlib ('0' for auto):", "0")
            .Add(this, x => x.myFunction, "Function:", "0")
            .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
            .Add(this, x => x.myVar1, "Var 1:", 0)
            .Add(this, x => x.myVar2, "Var 2:", 0)
            .Add(this, x => x.myEnableMouseClick, "Enable Mouseclick:", true)
            .Add(this, x => x.myEnableMouseRollOver, "Enable Mouse rollover:", true)
            .Add(this, x => x.myRollOverMember, "Rollover member:", "0")
            .Add(this, x => x.myRollOverMemberCastLib, "Rollover Castlib ('0' for auto):", "0")
        ;
    }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
{
    return new BehaviorPropertyDescriptionList()
        .Add(this, x => x.myFunction, "Function:", "0")
        .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
        .Add(this, x => x.myVar1, "Var 1:", 0)
        .Add(this, x => x.myVar2, "Var 2:", 0)
        .Add(this, x => x.myRollOverMember, "Rollover member:", "0")
        .Add(this, x => x.myRollOverMemberCastLib, "Rollover Castlib ('0' for auto):", "0");
}

public void BeginSprite()
{
    myLock = false;
    if (myRollOverMember == -1)
    {
        Sprite(Spritenum).Blend = 0;
    }
}

public void MouseEnter(LingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (myEnableMouseRollOver == true)
        {
            gMousePointer.Mouse_Over();
        }
        if (myRollOverMember > 0)
        {
            if (myRollOverMemberCastLib == 0)
            {
                Sprite(Spritenum).Member = Member(myRollOverMember, myRollOverMemberCastLib);
            }
            else
            {
                Sprite(Spritenum).Member = Member(myRollOverMember);
            }
        }
        else if (myRollOverMember == -1)
        {
            Sprite(Spritenum).Blend = 100;
        }
    }
}

public void MouseLeave(LingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (myEnableMouseRollOver == true)
        {
            gMousePointer.Mouse_Restore();
        }
        if (myRollOverMember > 0)
        {
            Sprite(Spritenum).Member = Member(myStartMember);
        }
        else if (myRollOverMember == -1)
        {
            Sprite(Spritenum).Blend = 0;
        }
    }
}

public void MouseDown(LingoMouseEvent mouse)
{
    if (myLock == (false && (myEnableMouseClick == true)))
    {
        if (myFunction == 0)
        {
            return;
        }
        SendSprite(mySpriteNum, sprite => sprite.myFunction(myVar1, myVar2));
    }
}

public void Lock()
{
    myLock = true;
}

public void UnLock()
{
    myLock = false;
}

public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
{
    return new BehaviorPropertyDescriptionList()
        .Add(this, x => x.myFunction, "Function:", "0")
        .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
        .Add(this, x => x.myVar1, "Var 1:", 0)
        .Add(this, x => x.myVar2, "Var 2:", 0)
        .Add(this, x => x.myRollOverMember, "Rollover member:", "0")
        .Add(this, x => x.myRollOverMemberCastLib, "Rollover Castlib ('0' for auto):", "0");
}

public void BeginSprite()
{
    myLock = false;
    if (myRollOverMember == -1)
    {
        Sprite(Spritenum).Blend = 0;
    }
}

public void MouseEnter(LingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (myEnableMouseRollOver == true)
        {
            gMousePointer.Mouse_Over();
        }
        if (myRollOverMember > 0)
        {
            if (myRollOverMemberCastLib == 0)
            {
                Sprite(Spritenum).Member = Member(myRollOverMember, myRollOverMemberCastLib);
            }
            else
            {
                Sprite(Spritenum).Member = Member(myRollOverMember);
            }
        }
        else if (myRollOverMember == -1)
        {
            Sprite(Spritenum).Blend = 100;
        }
    }
}

public void MouseLeave(LingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (myEnableMouseRollOver == true)
        {
            gMousePointer.Mouse_Restore();
        }
        if (myRollOverMember > 0)
        {
            Sprite(Spritenum).Member = Member(myStartMember);
        }
        else if (myRollOverMember == -1)
        {
            Sprite(Spritenum).Blend = 0;
        }
    }
}

public void MouseDown(LingoMouseEvent mouse)
{
    if (myLock == (false && (myEnableMouseClick == true)))
    {
        if (myFunction == 0)
        {
            return;
        }
        SendSprite(mySpriteNum, sprite => sprite.myFunction(myVar1, myVar2));
    }
}

public void Lock()
{
    myLock = true;
}

public void UnLock()
{
    myLock = false;
}

}
