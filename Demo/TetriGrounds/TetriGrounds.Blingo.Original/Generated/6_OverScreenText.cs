using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class OverScreenTextParent : BlingoParentScript, IHasStepFrameEvent
{
    public object myStartX;
    public object myStartY;
    public int myNum;
    public int myNum2;
    public int myCounter;
    public object myDuration;
    public object myParent;
    public int myLocV;

    private readonly GlobalVars _global;

    public OverScreenTextParent(IBlingoMovieEnvironment env, GlobalVars global, object _Duration, object _text, object _myParent) : base(env)
    {
        _global = global;
        
        myDuration = _Duration;
        myText = _text;
        myCounter = 0;
        myNum = 48;
        myNum2 = 49;
        myLocV = 100;
        if (Sprite(myNum).Puppet == true)
        {
        myNum = 46;
        myNum = 47;
        myLocV = 130;
        }
        Sprite(myNum).Puppet = true;
        Sprite(myNum2).Puppet = true;
        _movie.ActorList.Add(this);
        Sprite(myNum).Member = Member("T_OverScreen");
        Sprite(myNum2).Member = Member("T_OverScreen2");
        Sprite(myNum).LocZ = 1000;
        Sprite(myNum2).LocZ = 999;
        Sprite(myNum).Ink = 36;
        Sprite(myNum2).Ink = 36;
        GetMember<IBlingoMemberTextBase>("T_OverScreen").Text = myText;
        GetMember<IBlingoMemberTextBase>("T_OverScreen2").Text = myText;
        myParent = _myParent;
    }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void StepFrame()
{
    myCounter = myCounter + 1;
    if (myCounter > myDuration)
    {
        _movie.ActorList.DeleteOne(this);
        Sprite(myNum).Puppet = false;
        Sprite(myNum).Puppet = false;
        myParent.TextFinished(this);
    }
    myLocV = myLocV + 2;
    Sprite(myNum).LocH = 180;
    Sprite(myNum2).LocH = 180 + 2;
    Sprite(myNum).LocV = myLocV;
    Sprite(myNum2).LocV = myLocV + 2;
    blendd = 70 - ((float(myCounter) / myDuration) * 100);
    if (blendd < 0)
    {
        blendd = 0;
    }
    Sprite(myNum).Blend = blendd;
    Sprite(myNum2).Blend = blendd;
}

public void Destroy()
{
    if (_movie.ActorList.GetPos(this) != 0)
    {
        _movie.ActorList.DeleteOne(this);
    }
    Sprite(myNum).Blend = 100;
    Sprite(myNum2).Blend = 100;
    Sprite(myNum).Puppet = false;
    Sprite(myNum2).Puppet = false;
}

public void New(object _Duration, object _text, object _myParent)
{
    myDuration = _Duration;
    myText = _text;
    myCounter = 0;
    myNum = 48;
    myNum2 = 49;
    myLocV = 100;
    if (Sprite(myNum).Puppet == true)
    {
        myNum = 46;
        myNum = 47;
        myLocV = 130;
    }
    Sprite(myNum).Puppet = true;
    Sprite(myNum2).Puppet = true;
    _movie.ActorList.Add(this);
    Sprite(myNum).Member = Member("T_OverScreen");
    Sprite(myNum2).Member = Member("T_OverScreen2");
    Sprite(myNum).LocZ = 1000;
    Sprite(myNum2).LocZ = 999;
    Sprite(myNum).Ink = 36;
    Sprite(myNum2).Ink = 36;
    GetMember<IBlingoMemberTextBase>("T_OverScreen").Text = myText;
    GetMember<IBlingoMemberTextBase>("T_OverScreen2").Text = myText;
    myParent = _myParent;
}

public void StepFrame()
{
    myCounter = myCounter + 1;
    if (myCounter > myDuration)
    {
        _movie.ActorList.DeleteOne(this);
        Sprite(myNum).Puppet = false;
        Sprite(myNum).Puppet = false;
        myParent.TextFinished(this);
    }
    myLocV = myLocV + 2;
    Sprite(myNum).LocH = 180;
    Sprite(myNum2).LocH = 180 + 2;
    Sprite(myNum).LocV = myLocV;
    Sprite(myNum2).LocV = myLocV + 2;
    blendd = 70 - ((float(myCounter) / myDuration) * 100);
    if (blendd < 0)
    {
        blendd = 0;
    }
    Sprite(myNum).Blend = blendd;
    Sprite(myNum2).Blend = blendd;
}

public void Destroy()
{
    if (_movie.ActorList.GetPos(this) != 0)
    {
        _movie.ActorList.DeleteOne(this);
    }
    Sprite(myNum).Blend = 100;
    Sprite(myNum2).Blend = 100;
    Sprite(myNum).Puppet = false;
    Sprite(myNum2).Puppet = false;
}

}
