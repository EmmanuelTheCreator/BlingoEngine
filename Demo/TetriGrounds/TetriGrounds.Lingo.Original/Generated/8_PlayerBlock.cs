public class PlayerBlockParentScript : LingoParentScript, IHasStepFrameEvent
{
    public int myGfx;
    public int myBlocks;
    public int myScoreManager;
    public LingoList<object> MySubBlocks = new();
    public string myMemberName;
    public int myX;
    public int myY;
    public int myWidth;
    public object myMaxX;
    public object myMaxY;
    public bool myDownPressed;
    public bool myMoving;
    public int mySlowDown;
    public int myWaiter;
    public int mySlowDownFactorByLevel;
    public object myTypeBlocks;
    public bool myFinished;
    public object myLastKey;
    public int myKeyBoardWaiter;
    public int myKeyBoardTot;
    public object myBlockType;
    public LingoList<object> MyNextBlocks = new();
    public int myNextBlockType;
    public int MyNextBlockHor;
    public int MyNextBlockVer;
    public bool myPause;
    public bool myStopKeyAction;

    private readonly GlobalVars _global;

    public PlayerBlockParentScript(ILingoMovieEnvironment env, GlobalVars global, int _Gfx, int _Blocks, int _ScoreManager, object _width, object _height) : base(env)
    {
        _global = global;
        if (_width == null)
        {
        alert("no width given on playerBlock");
        }
        if (_height == null)
        {
        alert("no height given on playerBlock");
        }
        myBlocks = _Blocks;
        myScoreManager = _ScoreManager;
        MySubBlocks = [];
        MyNextBlocks = [];
        myMemberName = "Block1";
        myMaxX = _width;
        myMaxY = _height;
        AddTypeBlock([[0, 0], [-1, 0], [1, 0], [0, -1]], true);
        // 'T' Block, true-false = rotate(else its flip axis)
        AddTypeBlock([[0, 0], [-1, 0], [-2, 0], [1, 0]], true);
        // '-' Block
        AddTypeBlock([[-1, -1], [0, -1], [0, 0], [1, 0]], false);
        // 'z' Block
        AddTypeBlock([[1, -1], [0, -1], [0, 0], [-1, 0]], false);
        // 'z' Block invers
        AddTypeBlock([[0, -1], [1, -1], [0, 0], [1, 0]], false);
        // '#' Block Cubus
        AddTypeBlock([[-1, -1], [-1, 0], [0, 0], [1, 0]], true);
        // 'L' Block
        AddTypeBlock([[1, -1], [-1, 0], [0, 0], [1, 0]], true);
        // 'L' Block  invers
        myGfx = _Gfx;
        myX = myMaxX / 2;
        myY = 2;
        myWidth = 1;
        myMoving = false;
        _level = myScoreManager.GetLevel();
        Calculatespeed();
        if (mySlowDown <= 1)
        {
        mySlowDown = 1;
        }
        myWaiter = 0;
        myKeyBoardWaiter = 0;
        myKeyBoardTot = 11;
        myNextBlockType = 1;
        MyNextBlockHor = 16;
        MyNextBlockVer = 13;
        mySlowDownFactorByLevel = 10;
        UpdateNextBlock();
        StartMove();
        myStopKeyAction = false;
    }
// pointer
// pointer
// pointer
// used for all spritenums that are used by his block
// if the game is finished
// the next block that will come
public void Calculatespeed()
{
    _level = myScoreManager.GetLevel();
    mySlowDownFactorByLevel = 10;
    mySlowDown = 65;
    for (var i = 0; i <= _level; i++)
    {
        mySlowDownFactorByLevel = mySlowDownFactorByLevel - 1;
        if (mySlowDownFactorByLevel <= 3)
        {
            mySlowDownFactorByLevel = 2;
        }
        if (mySlowDown > 1)
        {
            mySlowDown = mySlowDown - mySlowDownFactorByLevel;
        }
    }
    Put(mySlowDown);
    // error
    mySlowDownFactorByLevel();
}

public void Keyyed(object val)
{
    if (myPause == false)
    {
        myKeyBoardWaiter = 0;
        myLastKey = val;
        MoveBlock(val);
    }
}

public void PauseGame()
{
    if (myPause == true)
    {
        Sprite(35).Blend = 0;
        myPause = false;
    }
    else
    {
        Sprite(35).Blend = 100;
        Sprite(35).LocZ = 10010;
        myPause = true;
    }
}

public void MoveBlock(object val)
{
    if (myFinished == true)
    {
        return;
    }
    // right
    if (val == 2)
    {
        Rightt();
        myDownPressed = false;
    }
    else if (val == 4)
    {
        Leftt();
        myDownPressed = false;
    }
    else if (val == 3)
    {
        if (myStopKeyAction == false)
        {
            // down
            myDownPressed = true;
        }
        else
        {
            myDownPressed = false;
        }
    }
    else if (val == 9)
    {
        // nothing
        myDownPressed = false;
        myStopKeyAction = false;
    }
    else if (val == 1)
    {
        // up
        TurnBlock();
        myDownPressed = false;
    }
}

public void LetBlockFall()
{
    starting = myY;
    for (var i = starting; i <= myMaxY; i++)
    {
        test = DownCheck();
        RefreshBlock();
        if (test == false)
        {
            break;
        }
    }
}

public void Stepframe()
{
    if (myPause == false)
    {
        if ((myLastKey <= 4) && (myLastKey != 1))
        {
            if (myKeyBoardWaiter > myKeyBoardTot)
            {
                myKeyBoardWaiter = 0;
                MoveBlock(myLastKey);
            }
            myKeyBoardWaiter = myKeyBoardWaiter + 1;
        }
        Addon = 0;
        if (myDownPressed == true)
        {
            Addon = mySlowDown;
        }
        if ((myWaiter + Addon) > mySlowDown)
        {
            myWaiter = 0;
            DownCheck();
            RefreshBlock();
        }
        else
        {
            myWaiter = myWaiter + 1;
        }
    }
}

public void DownCheck()
{
    check = CollitionDetect(myX, myY + 1);
    if (check == true)
    {
        FreezeBlock();
        ResetBlock();
        return false;
    }
    else
    {
        myY = myY + 1;
        return true;
    }
}

public void ResetBlock()
{
    if (myFinished == false)
    {
        foreach (var i in MySubBlocks)
        {
            _y = i[Symbol("yy")] + myY;
            check = myBlocks.FullHorizontal(_y);
            if (check == true)
            {
                myBlocks.RemoveHorizontal(_y);
            }
        }
        myStopKeyAction = true;
        DestroyBlock();
        CreateBlock();
        myY = 2;
        myX = myMaxX / 2;
        MyScoreManager.BlockFrozen();
        // add score when you freeze a block
        MyScoreManager.AddDropedBlock();
        // add that there's a block dropped
        // check if we go a level up
        levelup = MyScoreManager.GetLevelUp();
        if (levelup == true)
        {
            Calculatespeed();
        }
        check = CollitionDetect(myX, myY);
        if (check == true)
        {
            // game finished
            myScoreManager.GameFinished();
            myFinished = true;
            myBlocks.FinishedBlocks();
            StopMove();
            SendSprite<5_ScoreManager>(1, 5_scoremanager => 5_scoremanager.GameFinished(myScoreManager.GetScore();));
        }
    }
}

public void FreezeBlock()
{
    if (myFinished == false)
    {
        for (var i = 1; i <= MySubBlocks.Count; i++)
        {
            check = myBlocks.NewBlock(myX + MySubBlocks[i][Symbol("xx")], myY + MySubBlocks[i][Symbol("yy")], myBlockType);
        }
    }
}

public void CollitionDetect(object _x, object _y)
{
    for (var i = 1; i <= MySubBlocks.Count; i++)
    {
        if (myBlocks.IsBlock(_x + MySubBlocks[i][Symbol("xx")], _y + MySubBlocks[i][Symbol("yy")]) == true)
        {
            return true;
            break;
            return;
        }
    }
    return false;
}

public void TurnBlock()
{
    myWaiter = myWaiter + 1;
    offsetX = 0;
    tempBlock = [];
    collition = false;
    // prepare rotation
    for (var i = 1; i <= MySubBlocks.Count; i++)
    {
        oldx = MySubBlocks[i][Symbol("xx")];
        oldy = MySubBlocks[i][Symbol("yy")];
        newy = oldx;
        newx = -oldy;
        tempBlock.Add([newx, newy]);
        // check if the block isnt over border, then move it back with 'offset'
        // right
        // if myX+newX > myMaxX-1 then
        // -- check if the new offset will be bigger
        // if offsetX<newX then
        // offsetX=-newX
        // end if
        // else if myX+newX <= 0 then
        // --left
        // -- check if the new offset will be bigger
        // if offsetX>newX then
        // offsetX=-newX
        // end if
        // end if
        if (((myX + newX) > myMaxX) - 1)
        {
            collition = true;
        }
        // colition check
        check = myBlocks.IsBlock((myX + newx) + offsetX, myY + newy);
        if (check == true)
        {
            collition = true;
        }
    }
    if (collition == false)
    {
        // aply new rotation
        for (var i = 1; i <= MySubBlocks.Count; i++)
        {
            MySubBlocks[i][Symbol("xx")] = tempBlock[i][1];
            MySubBlocks[i][Symbol("yy")] = tempBlock[i][2];
        }
    }
    myX = myX + offsetX;
    RefreshBlock();
}

// --------------------------------------
public void Rightt()
{
    myWaiter = myWaiter + 1;
    check = CollitionDetect(myX + 1, myY);
    if (check == false)
    {
        // calculate max right from block
        maxright = 0;
        for (var i = 1; i <= MySubBlocks.Count; i++)
        {
            tempx = MySubBlocks[i][Symbol("xx")];
            if (tempx > maxright)
            {
                maxright = tempx;
            }
        }
        if (((myX + maxright) + 1) < myMaxX)
        {
            myX = myX + 1;
            RefreshBlock();
        }
    }
}

public void Leftt()
{
    myWaiter = myWaiter + 1;
    // left
    // left colition
    check = CollitionDetect(myX - 1, myY);
    if (check == false)
    {
        // calculate max left from block
        maxleft = 0;
        for (var i = 1; i <= MySubBlocks.Count; i++)
        {
            tempx = MySubBlocks[i][Symbol("xx")];
            if (tempx < maxleft)
            {
                maxleft = tempx;
            }
        }
        if (((myX - 1) + maxleft) > 0)
        {
            myX = myX - 1;
            RefreshBlock();
        }
    }
}

// --------------------------------------
public void StartMove()
{
    _movie.ActorList.Add(this);
    myMoving = true;
}

public void StopMove()
{
    _movie.ActorList.DeleteOne(this);
    myMoving = false;
}

// --------------------------------------
public void RefreshBlock()
{
    for (var i = 1; i <= MySubBlocks.Count; i++)
    {
        myGfx.PositionBlock(MySubBlocks[i][Symbol("obj")].GetSpriteNum();, myX + MySubBlocks[i][Symbol("xx")], myY + MySubBlocks[i][Symbol("yy")]);
    }
}

// --------------------------------------------------------
public void CreateBlock()
{
    myBlockType = myNextBlockType;
    chosenBlock = myTypeBlocks[myBlockType];
    // chosenBlock = myTypeBlocks[2]
    // create subBlocks
    for (var i = 1; i <= chosenBlock[1].Count; i++)
    {
        MySubBlocks.Add(new LingoPropertyList<object?>());
        MySubBlocks[i][Symbol("obj")] = new BlockParentScript(_env, _globalvars, myGfx, myBlockType);
        MySubBlocks[i][Symbol("obj")].CreateBlock();
        MySubBlocks[i][Symbol("xx")] = chosenBlock[1][i][1];
        MySubBlocks[i][Symbol("yy")] = chosenBlock[1][i][2];
        MySubBlocks[i][Symbol("rotating")] = chosenBlock[2][1];
    }
    RefreshBlock();
    UpdateNextBlock();
}

public void UpdateNextBlock()
{
    DestroyNextBlock();
    // create new
    myNextBlockType = random(myTypeBlocks.Count);
    chosenBlock = myTypeBlocks[myNextBlockType];
    for (var i = 1; i <= chosenBlock[1].Count; i++)
    {
        MyNextBlocks.Add(new LingoPropertyList<object?>());
        MyNextBlocks[i][Symbol("obj")] = new BlockParentScript(_env, _globalvars, myGfx, myNextBlockType);
        MyNextBlocks[i][Symbol("obj")].CreateBlock();
        MyNextBlocks[i][Symbol("xx")] = chosenBlock[1][i][1];
        MyNextBlocks[i][Symbol("yy")] = chosenBlock[1][i][2];
        MyNextBlocks[i][Symbol("rotating")] = chosenBlock[2][1];
        myGfx.PositionBlock(MyNextBlocks[i][Symbol("obj")].GetSpriteNum();, MyNextBlockHor + MyNextBlocks[i][Symbol("xx")], MyNextBlockVer + MyNextBlocks[i][Symbol("yy")]);
    }
}

public void DestroyNextBlock()
{
    // destroy previous
    foreach (var i in MyNextBlocks)
    {
        i[Symbol("obj")].Destroy();
        i[Symbol("obj")] = 0;
        i[Symbol("xx")] = 0;
        i[Symbol("yy")] = 0;
    }
    MyNextBlocks = [];
}

// --------------------------------------------------------
public void GetPause()
{
    return myPause;
}

public void DestroyBlock()
{
    foreach (var i in MySubBlocks)
    {
        i[Symbol("obj")].Destroy();
        i[Symbol("obj")] = 0;
        i[Symbol("xx")] = 0;
        i[Symbol("yy")] = 0;
    }
    MySubBlocks = [];
}

// -----------------
public void AddTypeBlock(object val, object _rotate)
{
    if (myTypeBlocks == null)
    {
        myTypeBlocks = [];
    }
    myTypeBlocks.Add([val, [_rotate]]);
}

// -----------------
public void Destroy()
{
    DestroyNextBlock();
    DestroyBlock();
    StopMove();
    myGfx = 0;
    myBlocks = 0;
    myScoreManager = 0;
}

}
