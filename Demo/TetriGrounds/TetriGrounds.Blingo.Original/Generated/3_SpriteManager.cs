using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class SpriteManagerParent : BlingoParentScript
{
    public int pNum;
    public BlingoList<object> pDestroyList = new();
    public BlingoList<object> pSpritenums = new();
    public int pGame;

    private readonly GlobalVars _global;

    public SpriteManagerParent(IBlingoMovieEnvironment env, GlobalVars global, int _beginningsprite) : base(env)
    {
        _global = global;
        
        pNum = _beginningsprite;
        // pNum = Counter
        pDestroyList = [];
        // List for all removed sprites Number
        pSpritenums = [];
        // all used sprites
        pGame = 0;
        // _game
    }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// the spritemanager create sprites only when needed.
// it begins at 100 to have 100 sprite for decoration of the editor
// --------------------------------------------------------------------------------------------
// create new spritemanager
// add a sprite
public void Sadd()
{
    // member("numSprites",1).text="used"&&pNum
    if (pDestroyList == [])
    {
        pNum = pNum + 1;
        // create a new one
        // check if we are at the maximum
        if (pNum > 999)
        {
            Sprite(1000).Puppet = true;
            Sprite(1000).Loc = point(100, 30);
            Sprite(1000).Member = Member("TomuchSprites");
            Sprite(1000).Ink = 36;
            Sprite(1000).Blend = 100;
            Sprite(1000).LocZ = "10000000000000";
            // updatestage
            Sprite(1000).Blend = 0;
            Sprite(1000).Loc = point(1, -40);
            pNum = pNum - 1;
            return 0;
            return;
        }
        Sprite(pNum).Puppet = true;
        if (pSpritenums.GetPos(pNum) != 0)
        {
            pNum = pNum + 100000;
        }
        pSpritenums.Add(pNum);
        Sprite(pNum).Ink = 36;
        return pNum;
    }
    else
    {
        // create a new from the destroyed sprite list
        pNumDestroy = pDestroyList.GetAt(1);
        Sprite(pNumDestroy).Puppet = true;
        pDestroyList.DeleteOne(pNumDestroy);
        pSpritenums.Add(pNumDestroy);
        Sprite(pNum).Ink = 36;
        return pNumDestroy;
    }
}

// destroy a sprite
public void SDestroy(object sprNum)
{
    if (SprNum == null)
    {
        SDestroyError("no spriteNum");
    }
    if (!(integerP)(SprNum))
    {
        SDestroyError("wrong SpriteNum");
    }
    if (pSpritenums.GetPos(sprNum) == 0)
    {
        return;
    }
    pSpritenums.DeleteOne(sprNum);
    pDestroyList.Add(sprNum);
    Sprite(sprNum).Member = Member("empty");
    Sprite(sprNum).LocZ = sprNum;
    Sprite(sprNum).Puppet = false;
    // go check if there are sprites in the idlelist
    // pGame.graphics.destroyedsprite(sprnum)
    // member("numSprites",1).text="used"&&pNum
}

// ----------------------------------------------------------------------------------
public void GetSpritenums()
{
    return pSpritenums;
}

public void Checksprite(object _num)
{
    if (pSpritenums.Getone(_num) > 0)
    {
        return 1;
    }
    else
    {
        return 0;
    }
}

// --------------------------------------------------------------------------------------
public void Destroy()
{
    foreach (var i in pDestroyList)
    {
        SDestroy(i);
    }
    pGame = null;
    pMap = null;
}

public void SDestroyError(string para)
{
    alert("SpriteDistroy received " + para);
    abort();
}

// it begins at 100 to have 100 sprite for decoration of the editor
// --------------------------------------------------------------------------------------------
// create new spritemanager
public void New(object _beginningsprite)
{
    pNum = _beginningsprite;
    // pNum = Counter
    pDestroyList = [];
    // List for all removed sprites Number
    pSpritenums = [];
    // all used sprites
    pGame = 0;
    // _game
}

// add a sprite
public void Sadd()
{
    // member("numSprites",1).text="used"&&pNum
    if (pDestroyList == [])
    {
        pNum = pNum + 1;
        // create a new one
        // check if we are at the maximum
        if (pNum > 999)
        {
            Sprite(1000).Puppet = true;
            Sprite(1000).Loc = point(100, 30);
            Sprite(1000).Member = Member("TomuchSprites");
            Sprite(1000).Ink = 36;
            Sprite(1000).Blend = 100;
            Sprite(1000).LocZ = "10000000000000";
            // updatestage
            Sprite(1000).Blend = 0;
            Sprite(1000).Loc = point(1, -40);
            pNum = pNum - 1;
            return 0;
            return;
        }
        Sprite(pNum).Puppet = true;
        if (pSpritenums.GetPos(pNum) != 0)
        {
            pNum = pNum + 100000;
        }
        pSpritenums.Add(pNum);
        Sprite(pNum).Ink = 36;
        return pNum;
    }
    else
    {
        // create a new from the destroyed sprite list
        pNumDestroy = pDestroyList.GetAt(1);
        Sprite(pNumDestroy).Puppet = true;
        pDestroyList.DeleteOne(pNumDestroy);
        pSpritenums.Add(pNumDestroy);
        Sprite(pNum).Ink = 36;
        return pNumDestroy;
    }
}

// destroy a sprite
public void SDestroy(object sprNum)
{
    if (SprNum == null)
    {
        SDestroyError("no spriteNum");
    }
    if (!(integerP)(SprNum))
    {
        SDestroyError("wrong SpriteNum");
    }
    if (pSpritenums.GetPos(sprNum) == 0)
    {
        return;
    }
    pSpritenums.DeleteOne(sprNum);
    pDestroyList.Add(sprNum);
    Sprite(sprNum).Member = Member("empty");
    Sprite(sprNum).LocZ = sprNum;
    Sprite(sprNum).Puppet = false;
    // go check if there are sprites in the idlelist
    // pGame.graphics.destroyedsprite(sprnum)
    // member("numSprites",1).text="used"&&pNum
}

// ----------------------------------------------------------------------------------
public void GetSpritenums()
{
    return pSpritenums;
}

public void Checksprite(object _num)
{
    if (pSpritenums.Getone(_num) > 0)
    {
        return 1;
    }
    else
    {
        return 0;
    }
}

// --------------------------------------------------------------------------------------
public void Destroy()
{
    foreach (var i in pDestroyList)
    {
        SDestroy(i);
    }
    pGame = null;
    pMap = null;
}

public void SDestroyError(string para)
{
    alert("SpriteDistroy received " + para);
    abort();
}

}
