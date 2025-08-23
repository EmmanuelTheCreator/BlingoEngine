public class score_getParentScript : LingoParentScript, IHasStepFrameEvent
{
    public string myURL;
    public object myNetID;
    public bool myDone;
    public int myErr;
    public LingoList<object> myScores = new();
    public int myShowType;

    private readonly GlobalVars _global;

    public score_getParentScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
        myURL = "";
        myErr = 0;
        myDone = False;
        scores = [];
        myShowType = 1;
    }
public void SetURL(object scriptURL)
{
    myURL = scriptURL;
}

public void DownloadScores()
{
    myErr = 0;
    myDone = False;
    scores = [];
    myNetID = getNetText(myURL)    ;
    _movie.actorList.add(this);
}

public void StepFrame()
{
    if (netDone(myNetID)    )
    {
        myErr = netError(myNetID)        ;
        if (myErr == "OK")
        {
            data = netTextResult(myNetID)            ;
            myScores = [System.Convert.ToInt32(data.line[1]), System.Convert.ToInt32(data.line[2])];
            OutputScores();
        }
        myDone = True;
        _movie.actorList.deleteOne(this);
    }
}

public void OutputScores()
{
    if (myScores == null)
    {
        return;
    }
    scores = myScores[1];
    if (scores == null)
    {
        return;
    }
    GetMember<ILingoMemberTextBase>("T_InternetScoresNames").Text = "";
    foreach (var i in scores)
    {
        GetMember<ILingoMemberTextBase>("T_InternetScoresNames").Text = (GetMember<ILingoMemberTextBase>("T_InternetScoresNames").Text + i[1]) + "\n";
    }
    GetMember<ILingoMemberTextBase>("T_InternetScores").Text = "";
    foreach (var i in scores)
    {
        GetMember<ILingoMemberTextBase>("T_InternetScores").Text = (GetMember<ILingoMemberTextBase>("T_InternetScores").Text + i[2]) + "\n";
    }
    scores = myScores[2];
    if (scores == null)
    {
        return;
    }
    GetMember<ILingoMemberTextBase>("T_InternetScoresNamesP").Text = "";
    foreach (var i in scores)
    {
        GetMember<ILingoMemberTextBase>("T_InternetScoresNamesP").Text = (GetMember<ILingoMemberTextBase>("T_InternetScoresNamesP").Text + i[1]) + "\n";
    }
    GetMember<ILingoMemberTextBase>("T_InternetScoresP").Text = "";
    foreach (var i in scores)
    {
        GetMember<ILingoMemberTextBase>("T_InternetScoresP").Text = (GetMember<ILingoMemberTextBase>("T_InternetScoresP").Text + i[2]) + "\n";
    }
}

public void GetLowestPersonalScore()
{
    if (myScores == null)
    {
        return 0;
    }
    if (!(listp)(myScores[2])    )
    {
        return 0;
    }
    if (myScores[2].count < 5)
    {
        return 0;
    }
    return myScores[2][myScores[2].count][2];
}

public void SetShowType(object val)
{
    myShowType = val;
}

public void GetHighScoreList()
{
    return myScores;
}

public void GetErr()
{
    return myErr;
}

public void IsDone()
{
    return myDone;
}

public void Destroy()
{
    if (_movie.actorList.getpos(this);
     != 0)
    {
        _movie.actorList.deleteOne(this);
    }
}

}
