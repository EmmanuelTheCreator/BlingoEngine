using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class FontRollOverBehavior : BlingoSpriteBehavior, IBlingoPropertyDescriptionList, IHasMouseWithinEvent, IHasMouseLeaveEvent, IHasMouseDownEvent, IHasBeginSpriteEvent
{
    public AColor myColor = AColor.FromCode(0,0,0);
    public AColor myColorOver = AColor.FromCode(100,0,0);
    public AColor myColorLock = AColor.FromCode(150,150,150);
    public bool myLock = false;
    public string myFunction = "0";
    public int mySpriteNum = 4;
    public int myVar1 = 0;
    public int myVar2 = 0;

    public FontRollOverBehavior(IBlingoMovieEnvironment env) : base(env) { }

    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
    {
        return new BehaviorPropertyDescriptionList()
            .Add(this, x => x.myColor, "Color:", AColor.FromCode(0,0,0))
            .Add(this, x => x.myColorOver, "ColorOver:", AColor.FromCode(100,0,0))
            .Add(this, x => x.myColorLock, "ColorLock:", AColor.FromCode(150,150,150))
            .Add(this, x => x.myLock, "Start Locked:", false)
            .Add(this, x => x.myFunction, "Function:", "0")
            .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
            .Add(this, x => x.myVar1, "Var 1:", 0)
            .Add(this, x => x.myVar2, "Var 2:", 0)
            .Add(this, x => x.myColor, "Color:", AColor.FromCode(0,0,0))
            .Add(this, x => x.myColorOver, "ColorOver:", AColor.FromCode(100,0,0))
            .Add(this, x => x.myColorLock, "ColorLock:", AColor.FromCode(150,150,150))
            .Add(this, x => x.myLock, "Start Locked:", false)
            .Add(this, x => x.myFunction, "Function:", "0")
            .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
            .Add(this, x => x.myVar1, "Var 1:", 0)
            .Add(this, x => x.myVar2, "Var 2:", 0)
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
        .Add(this, x => x.myVar2, "Var 2:", 0);
}

public void BeginSprite()
{
    if (myLock == true)
    {
        Sprite(Spritenum).Member.Color = myColorLock;
    }
    else
    {
        Sprite(Spritenum).Member.Color = myColor;
    }
}

public void MouseDown(BlingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (myFunction == 0)
        {
            return;
        }
        SendSprite(mySpriteNum, sprite => sprite.myFunction(myVar1, myVar2));
    }
}

public void MouseWithin(BlingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (Sprite(Spritenum).Member.Color != myColorOver)
        {
            Sprite(Spritenum).Member.Color = myColorOver;
            Cursor = 280;
        }
    }
}

public void MouseLeave(BlingoMouseEvent mouse)
{
    if (myLock == false)
    {
        Sprite(Spritenum).Member.Color = myColor;
        Cursor = -1;
    }
}

public void Lock()
{
    myLock = True;
    Sprite(Spritenum).Member.Color = myColorLock;
}

public void Unlock()
{
    Sprite(Spritenum).Member.Color = myColor;
    myLock = false;
}

public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
{
    return new BehaviorPropertyDescriptionList()
        .Add(this, x => x.myFunction, "Function:", "0")
        .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
        .Add(this, x => x.myVar1, "Var 1:", 0)
        .Add(this, x => x.myVar2, "Var 2:", 0);
}

public void BeginSprite()
{
    if (myLock == true)
    {
        Sprite(Spritenum).Member.Color = myColorLock;
    }
    else
    {
        Sprite(Spritenum).Member.Color = myColor;
    }
}

public void MouseDown(BlingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (myFunction == 0)
        {
            return;
        }
        SendSprite(mySpriteNum, sprite => sprite.myFunction(myVar1, myVar2));
    }
}

public void MouseWithin(BlingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (Sprite(Spritenum).Member.Color != myColorOver)
        {
            Sprite(Spritenum).Member.Color = myColorOver;
            Cursor = 280;
        }
    }
}

public void MouseLeave(BlingoMouseEvent mouse)
{
    if (myLock == false)
    {
        Sprite(Spritenum).Member.Color = myColor;
        Cursor = -1;
    }
}

public void Lock()
{
    myLock = True;
    Sprite(Spritenum).Member.Color = myColorLock;
}

public void Unlock()
{
    Sprite(Spritenum).Member.Color = myColor;
    myLock = false;
}

}

