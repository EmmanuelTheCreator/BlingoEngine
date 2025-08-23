public class ScoreManagerParentScript : LingoParentScript
{
    public int myPlayerScore;
    public string myLevel;
    public int myNumberLinesRemoved;
    public int myNumberLinesTot;
    public bool myLevelUp;
    public int myLevelUpNeededScore;
    public int myBlocksDroped;
    public LingoList<object> myOverScreenText = new();

    private readonly GlobalVars _global;

    public ScoreManagerParentScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
        myPlayerScore = 0;
        myNumberLinesTot = 0;
        myLevelUpPlusScore = 0;
        // find stqrt level
        myLevel = System.Convert.ToInt32(GetMember<ILingoMemberTextBase>("T_StartLevel").Text);
        myLevelUpNeededScore = 20 * (myLevel + 1);
        UpdateGfxScore();
        myTexts = [];
        NewText("Go!");
        Refresh();
    }
public void Refresh()
{
    case(myNumberLinesRemoved);
    // error
    // error
    // error
    LineRemoved1();
    // error
    // error
    LineRemoved2();
    // error
    // error
    LineRemoved3();
    // error
    // error
    LineRemoved4();
}

case(myNumberLinesRemoved == 0);
// check fot level up (its the number of blocks droped)
if (myBlocksDroped > myLevelUpNeededScore)
{
    SendSprite<15_AnimationScript>(22, 15_animationscript => 15_animationscript.startAnim());
    myLevelUp = true;
    myLevel = myLevel + 1;
    NewText(("Level " + myLevel) + " !!");
    myLevelUpNeededScore = myLevelUpNeededScore + 20;
    // this is the score needed to go a level up
}
UpdateGfxScore();
GetMember<ILingoMemberTextBase>("T_data").Text = "Level " + myLevel;
// member("T_data").text = "Blocks = "&string(myBlocksDroped)&return&"NeededBlocks = "&myLevelUpNeededScore&return&\
// error
// error
((myNumberLinesTot + "\n") + "myLevel = ") + myLevel// error
// ----------------------------------------
public void LineRemoved1()
{
    myPlayerScore = (myPlayerScore + 5) + myLevel;
}

public void LineRemoved2()
{
    NewText("2 Lines Removed!!");
    myPlayerScore = (myPlayerScore + 12) + myLevel;
}

public void LineRemoved3()
{
    NewText("3 Lines Removed!!");
    myPlayerScore = (myPlayerScore + 20) + myLevel;
}

public void LineRemoved4()
{
    NewText("Wooow, 4 Lines Removed!!");
    myPlayerScore = (myPlayerScore + 30) + myLevel;
}

// ----------------------------------------
public void AddDropedBlock()
{
    myBlocksDroped = myBlocksDroped + 1;
}

public void LineRemoved()
{
    myNumberLinesRemoved = myNumberLinesRemoved + 1;
    myNumberLinesTot = myNumberLinesTot + 1;
}

public void BlockFrozen()
{
    myPlayerScore = myPlayerScore + 1;
    Refresh();
}

public void UpdateGfxScore()
{
    GetMember<ILingoMemberTextBase>("T_Score").Text = string(myPlayerScore);
}

public void GetLevelup()
{
    temp = myLevelUp;
    myLevelUp = false;
    return temp;
}

public void GameFinished()
{
    NewText("You're Terminated....");
}

public void GetLevel()
{
    return myLevel;
}

public void GetScore()
{
    return myPlayerScore;
}

// ---------------------------
public void NewText(object _text)
{
    if (myOverScreenText == null)
    {
        myOverScreenText = [];
    }
    myOverScreenText.Add(new OverScreenTextParentScript(_env, _globalvars, 130, _text, this);
    );
}

public void TextFinished(object obj)
{
    _pos = myOverScreenText.GetPos(obj);
    myOverScreenText[_pos].Destroy();
    myOverScreenText.DeleteOne(obj);
}

public void DestroyoverscreenTxt()
{
    for (var i = 1; i <= myOverScreenText.Count; i++)
    {
        myOverScreenText[i].Destroy();
        myOverScreenText[i] = 0;
    }
    myOverScreenText = [];
}

// ---------------------------
public void Destroy()
{
    DestroyoverscreenTxt();
}

}
