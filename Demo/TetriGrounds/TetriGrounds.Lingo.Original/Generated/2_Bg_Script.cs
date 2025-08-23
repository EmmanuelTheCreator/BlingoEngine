public class Bg_ScriptBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasEndSpriteEvent
{
    public object myPlayerBlock;
    public object myGfx;
    public object myBlocks;
    public object myScoreManager;

    public Bg_ScriptBehavior(ILingoMovieEnvironment env) : base(env) { }
public void Beginsprite()
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
    myPlayerBlock.keyyed(val);
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
    if (objectp(myPlayerBlock)    )
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

public void Endsprite()
{
    TeminateGame();
}

public void StartNewGame()
{
    if (gSpriteManager == null)
    {
        gSpriteManager = new SpriteManagerParentScript(_env, _globalvars, 100);
    }
    myWidth = 11;
    myHeight = 22;
    myGfx = new GfxParentScript(_env, _globalvars);
    myScoreManager = new ScoreManagerParentScript(_env, _globalvars);
    myBlocks = new BlocksParentScript(_env, _globalvars, myGfx, myScoreManager, myWidth, myHeight);
    myPlayerBlock = new PlayerBlockParentScript(_env, _globalvars, myGfx, myBlocks, myScoreManager, myWidth, myHeight);
    myPlayerBlock.createBlock();
}

public void TeminateGame()
{
    if (myPlayerBlock != 0)
    {
        myPlayerBlock.destroy();
    }
    myPlayerBlock = 0;
    if (myBlocks != 0)
    {
        myBlocks.destroy();
    }
    myBlocks = 0;
    if (myGfx != 0)
    {
        myGfx.destroy();
    }
    myGfx = 0;
    if (myScoreManager != 0)
    {
        myScoreManager.destroy();
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
