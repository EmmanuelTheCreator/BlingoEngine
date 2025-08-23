public class FontRollOverBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList, IHasMouseWithinEvent, IHasMouseLeaveEvent, IHasMouseDownEvent, IHasBeginSpriteEvent
{
    public AColor myColor = AColor.FromCode(0,0,0);
    public AColor myColorOver = AColor.FromCode(100,0,0);
    public AColor myColorLock = AColor.FromCode(150,150,150);
    public bool myLock = false;
    public string myFunction = "0";
    public int mySpriteNum = 4;
    public int myVar1 = 0;
    public int myVar2 = 0;

    public FontRollOverBehavior(ILingoMovieEnvironment env) : base(env) { }

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
        ;
    }
public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
{
    return new BehaviorPropertyDescriptionList()
        .Add(this, x => x.myFunction, "Function:", "0")
        .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
        .Add(this, x => x.myVar1, "Var 1:", 0)
        .Add(this, x => x.myVar2, "Var 2:", 0);
}

public void Beginsprite()
{
    if (myLock == true)
    {
        Sprite(Spritenum).Member.color = myColorLock;
    }
    else
    {
        Sprite(Spritenum).Member.color = myColor;
    }
}

public void Mousedown(LingoMouseEvent mouse)
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

public void MouseWithin(LingoMouseEvent mouse)
{
    if (myLock == false)
    {
        if (Sprite(Spritenum).Member.color != myColorOver)
        {
            Sprite(Spritenum).Member.color = myColorOver;
            Cursor = 280;
        }
    }
}

public void Mouseleave(LingoMouseEvent mouse)
{
    if (myLock == false)
    {
        Sprite(Spritenum).Member.color = myColor;
        Cursor = -1;
    }
}

public void Lock()
{
    myLock = True;
    Sprite(Spritenum).Member.color = myColorLock;
}

public void Unlock()
{
    Sprite(Spritenum).Member.color = myColor;
    myLock = false;
}

}
