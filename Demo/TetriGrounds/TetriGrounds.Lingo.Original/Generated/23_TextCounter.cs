public class TextCounterBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList, IHasBeginSpriteEvent, IHasExitFrameEvent
{
    public int myMax = 10;
    public int myMin = 0;
    public int myValue = -1;
    public int myStep = 1;
    public int myDataSpriteNum = 1;
    public string myDataName = "1";
    public string myFunction = "70";
    public object myWaiter;
    public int myWaitbeforeExecute = 70;

    public TextCounterBehavior(ILingoMovieEnvironment env) : base(env) { }

    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
    {
        return new BehaviorPropertyDescriptionList()
            .Add(this, x => x.myMin, "Min Value:", 0)
            .Add(this, x => x.myMax, "Max Value:", 10)
            .Add(this, x => x.myValue, "My Start Value:", -1)
            .Add(this, x => x.myStep, "My step:", 1)
            .Add(this, x => x.myDataSpriteNum, "My Sprite that contains info\\n(set value to -1):", 1)
            .Add(this, x => x.myDataName, "Name Info:", "1")
            .Add(this, x => x.myWaitbeforeExecute, "WaitTime before execute:", 70)
            .Add(this, x => x.myFunction, "function to execute:", "70")
        ;
    }
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

public void Beginsprite()
{
    if (myValue == -1)
    {
        myValue = SendSprite<GetCounterStartDataBehavior>(myDataSpriteNum, getcounterstartdatabehavior => getcounterstartdatabehavior.GetCounterStartData(myDataName));
        if (myValue == null)
        {
            myValue = 0;
        }
        if ((myValue < myMin) || (myValue > myMax))
        {
            myValue = 0;
        }
    }
    Updateme();
    myWaiter = myWaitbeforeExecute;
}

public void Exitframe()
{
    if (myWaiter < myWaitbeforeExecute)
    {
        if (myWaiter == (myWaitbeforeExecute - 1))
        {
            SendSprite(myDataSpriteNum, sprite => sprite.myFunction(myDataName, myValue));
        }
        myWaiter = myWaiter + 1;
    }
}

public void Addd()
{
    if (myValue < myMax)
    {
        myValue = myValue + myStep;
        Updateme();
    }
}

public void Deletee()
{
    if (myValue > myMin)
    {
        myValue = myValue - myStep;
        Updateme();
    }
}

public void Updateme()
{
    GetMember<ILingoMemberTextBase>(Sprite(Spritenum).Member).Text = string(myValue);
    ;
    myWaiter = 0;
}

}
