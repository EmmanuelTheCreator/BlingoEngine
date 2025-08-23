public class MousePointerBehavior : LingoSpriteBehavior, IHasStepFrameEvent
{
    public object myNum;
    public object myOldX;
    public object myOldY;
    public int myAnimateNum;
    public int myStartMember;
    public int myNumberMembers;
    public int myDir;

    public MousePointerBehavior(ILingoMovieEnvironment env, object _num) : base(env)
    {
        myNum = _num;
        myStartMember = 80;
        myNumberMembers = 5;
        myAnimateNum = 0;
        myDir = // error
        ;
        // error
        ShowMouse();
    }
public void Stepframe()
{
    Refresh();
}

public void Refresh()
{
    changed = false;
    if (myOldX != _Mouse.MouseH)
    {
        myOldX = _Mouse.MouseH;
        changed = true;
    }
    if (myOldY != _Mouse.MouseV)
    {
        myOldY = _Mouse.MouseV;
        changed = true;
    }
    if (changed == true)
    {
        Sprite(myNum).LocH = _Mouse.MouseH + 17;
        Sprite(myNum).LocV = _Mouse.MouseV + 10;
        if (myDir == 1)
        {
            if (myAnimateNum < myNumberMembers)
            {
                myAnimateNum = myAnimateNum + 1;
            }
            else
            {
                myDir = -1;
            }
        }
        else if (myAnimateNum > 0)
        {
            myAnimateNum = myAnimateNum - 1;
        }
        else
        {
            myDir = // error
            ;
            // error
        }
        Sprite(myNum).Membernum = myAnimateNum + myStartMember;
    }
}

public void ShowMouse()
{
    Sprite(myNum).Puppet = true;
    Sprite(myNum).LocZ = 1000000;
    Sprite(myNum).Member = Member("mouse0000");
    _movie.actorList.append(this);
}

public void Mouse_Over()
{
    // sprite(myNum).member = member("mouse0000")
}

public void Mouse_Restore()
{
    // sprite(myNum).member = member("mouse0000")
}

public void Destroy()
{
    if (_movie.actorList.getpos(this);
     > 0)
    {
        _movie.actorList.deleteOne(this);
    }
}

}
