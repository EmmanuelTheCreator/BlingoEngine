using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class Bg_ScriptBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
{
    public int myPlayerBlock;
    public int myGfx;
    public int myBlocks;
    public int myScoreManager;

    public Bg_ScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void BeginSprite()
{
    if (myPlayerBlock == null)
    {
        Sprite(35).Blend = 0;
    }
    // me.StartNewGame()
}

public void Actionkey(object val)
{
    Put(val);
}

public void Keyaction(object val)
{
    if (myPlayerBlock == null)
    {
        return;
    }
    myPlayerBlock.Keyyed(val);
}

public void PauseGame()
{
    if (myPlayerBlock == null || (myPlayerBlock == 0))
    {
        return;
    }
    myPlayerBlock.PauseGame();
}

public void NewGame()
{
    // check if the game isnt set to pause
    if (myPlayerBlock != null)
    {
        _pause = myPlayerBlock.GetPause();
        if (_pause == false)
        {
            TeminateGame();
            StartNewGame();
        }
    }
    else
    {
        TeminateGame();
        StartNewGame();
    }
}

public void EndSprite()
{
    TeminateGame();
}

public void StartNewGame()
{
    if (gSpriteManager == null)
    {
        gSpriteManager = new SpriteManagerParent(_env, _globalvars, 100);
    }
    myWidth = 11;
    myHeight = 22;
    myGfx = new GfxParent(_env, _globalvars);
    myScoreManager = new ScoreManagerParent(_env, _globalvars);
    myBlocks = new BlocksParent(_env, _globalvars, myGfx, myScoreManager, myWidth, myHeight);
    myPlayerBlock = new PlayerBlockParent(_env, _globalvars, myGfx, myBlocks, myScoreManager, myWidth, myHeight);
    myPlayerBlock.CreateBlock();
}

public void TeminateGame()
{
    if (myPlayerBlock != 0)
    {
        myPlayerBlock.Destroy();
    }
    myPlayerBlock = 0;
    if (myBlocks != 0)
    {
        myBlocks.Destroy();
    }
    myBlocks = 0;
    if (myGfx != 0)
    {
        myGfx.Destroy();
    }
    myGfx = 0;
    if (myScoreManager != 0)
    {
        myScoreManager.Destroy();
    }
    myScoreManager = 0;
}

public void SpaceBar()
{
    if (myPlayerBlock == null || (myPlayerBlock == 0))
    {
        return;
    }
    myPlayerBlock.LetBlockFall();
}

public void BeginSprite()
{
    if (myPlayerBlock == null)
    {
        Sprite(35).Blend = 0;
    }
    // me.StartNewGame()
}

public void Actionkey(object val)
{
    Put(val);
}

public void Keyaction(object val)
{
    if (myPlayerBlock == null)
    {
        return;
    }
    myPlayerBlock.Keyyed(val);
}

public void PauseGame()
{
    if (myPlayerBlock == null || (myPlayerBlock == 0))
    {
        return;
    }
    myPlayerBlock.PauseGame();
}

public void NewGame()
{
    // check if the game isnt set to pause
    if (myPlayerBlock != null)
    {
        _pause = myPlayerBlock.GetPause();
        if (_pause == false)
        {
            TeminateGame();
            StartNewGame();
        }
    }
    else
    {
        TeminateGame();
        StartNewGame();
    }
}

public void EndSprite()
{
    TeminateGame();
}

public void StartNewGame()
{
    if (gSpriteManager == null)
    {
        gSpriteManager = new SpriteManagerParent(_env, _globalvars, 100);
    }
    myWidth = 11;
    myHeight = 22;
    myGfx = new GfxParent(_env, _globalvars);
    myScoreManager = new ScoreManagerParent(_env, _globalvars);
    myBlocks = new BlocksParent(_env, _globalvars, myGfx, myScoreManager, myWidth, myHeight);
    myPlayerBlock = new PlayerBlockParent(_env, _globalvars, myGfx, myBlocks, myScoreManager, myWidth, myHeight);
    myPlayerBlock.CreateBlock();
}

public void TeminateGame()
{
    if (myPlayerBlock != 0)
    {
        myPlayerBlock.Destroy();
    }
    myPlayerBlock = 0;
    if (myBlocks != 0)
    {
        myBlocks.Destroy();
    }
    myBlocks = 0;
    if (myGfx != 0)
    {
        myGfx.Destroy();
    }
    myGfx = 0;
    if (myScoreManager != 0)
    {
        myScoreManager.Destroy();
    }
    myScoreManager = 0;
}

public void SpaceBar()
{
    if (myPlayerBlock == null || (myPlayerBlock == 0))
    {
        return;
    }
    myPlayerBlock.LetBlockFall();
}

}
