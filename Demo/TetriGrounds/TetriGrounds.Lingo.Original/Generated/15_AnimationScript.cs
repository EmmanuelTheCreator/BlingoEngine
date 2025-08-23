public class AnimationScriptBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList, IHasBeginSpriteEvent, IHasEndSpriteEvent, IHasStepFrameEvent
{
    public int myEndMembernum = 10;
    public int myStartMembernum = 0;
    public int myValue = -1;
    public object myStep;
    public int myDataSpriteNum = 1;
    public string myDataName = "1";
    public string myFunction = "70";
    public int myWaiter;
    public int myWaitbeforeExecute = 0;
    public bool myAnimate;
    public int mySlowDown = 1;
    public int mySlowDownCounter;

    public AnimationScriptBehavior(ILingoMovieEnvironment env) : base(env) { }

    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
    {
        return new BehaviorPropertyDescriptionList()
            .Add(this, x => x.myStartMembernum, "My Start membernum:", 0)
            .Add(this, x => x.myEndMembernum, "My End membernum:", 10)
            .Add(this, x => x.myValue, "My Start Value:", -1)
            .Add(this, x => x.mySlowDown, "mySlowDown:", 1)
            .Add(this, x => x.myDataSpriteNum, "My Sprite that contains info\\n(set value to -1):", 1)
            .Add(this, x => x.myDataName, "Name Info:", "1")
            .Add(this, x => x.myWaitbeforeExecute, "WaitTime before execute:", 0)
            .Add(this, x => x.myFunction, "function to execute:", "70")
        ;
    }
public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
{
    return new BehaviorPropertyDescriptionList()
        .Add(this, x => x.myStartMembernum, "My Start membernum:", 0)
        .Add(this, x => x.myEndMembernum, "My End membernum:", 10)
        .Add(this, x => x.myValue, "My Start Value:", -1)
        .Add(this, x => x.mySlowDown, "mySlowDown:", 1)
        .Add(this, x => x.myDataSpriteNum, "My Sprite that contains info\n(set value to -1):", 1)
        .Add(this, x => x.myDataName, "Name Info:", "1")
        .Add(this, x => x.myWaitbeforeExecute, "WaitTime before execute:", 0)
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
        if ((myValue < myStartMembernum) || (myValue > myEndMembernum))
        {
            myValue = myStartMembernum;
        }
    }
    Updateme();
    myWaiter = myWaitbeforeExecute;
    myAnimate = false;
    StartAnim();
}

// on exitframe me
// if myWaiter<myWaitbeforeExecute then
// if myWaiter=myWaitbeforeExecute-1 then
// sendsprite(myDataSpriteNum ,myFunction,myDataName,myValue)
// end if
// myWaiter = myWaiter +1
// end if
public void Stepframe()
{
    if (myAnimate == true)
    {
        if (mySlowDownCounter == mySlowDown)
        {
            mySlowDownCounter = 0;
            if (myValue <= myEndMembernum)
            {
                myValue = myValue + 1;
                Updateme();
            }
            else
            {
                // finished anim
                _movie.actorList.deleteOne(this);
                myAnimate = false;
            }
        }
        else
        {
            mySlowDownCounter = mySlowDownCounter + 1;
        }
    }
}

public void StartAnim()
{
    for (var i = myStartMembernum; i <= myEndMembernum; i++)
    {
        Member(i).preload();
    }
    myAnimate = true;
    mySlowDownCounter = 0;
    myValue = myStartMembernum;
    _movie.actorList.append(this);
}

public void Updateme()
{
    Sprite(Spritenum).Membernum = // error
    ;
    myValue(// error);
    myWaiter = 0;
}

public void Endsprite()
{
    if (_movie.actorList.getpos(this);
     > 0)
    {
        _movie.actorList.deleteOne(this);
    }
}

}
