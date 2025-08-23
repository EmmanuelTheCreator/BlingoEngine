public class BlockParentScript : LingoParentScript, IHasStepFrameEvent
{
    public object myMember;
    public object myNum;
    public bool myDestroyAnim;
    public int myMemberNumAnim;

    private readonly GlobalVars _global;

    public BlockParentScript(ILingoMovieEnvironment env, GlobalVars global, object _Gfx, int ChosenType) : base(env)
    {
        _global = global;
        if (ChosenType == null)
        {
        ChosenType = 1;
        }
        myMembers = ["Block1", "Block2", "Block3", "Block4", "Block5", "Block6", "Block7"];
        myMember = myMembers[ChosenType];
        myGfx = _Gfx;
        myDestroyAnim = false;
    }
public void Stepframe()
{
    if (myDestroyAnim == true)
    {
        if (myMemberNumAnim > 7)
        {
            myMemberNumAnim = 0;
            _Movie.ActorList.Deleteone(this);
            Destroy();
        }
        myMemberNumAnim = myMemberNumAnim + 1;
        Sprite(myNum).Member = Member("Destroy" + myMemberNumAnim);
    }
}

public void DestroyAnim()
{
    myDestroyAnim = true;
    myMemberNumAnim = 0;
    if (_Movie.ActorList.Getpos(this);
     == 0)
    {
        _Movie.ActorList.Add(this);
    }
}

public void FinishBlock()
{
    myMember = Member("Destroy1");
    Sprite(myNum).Member = myMember;
}

public void CreateBlock()
{
    myNum = gSpritemanager.SAdd()    ;
    Sprite(myNum).Member = myMember;
    Sprite(myNum).Ink = 36;
}

public void GetSpriteNum()
{
    return myNum;
}

public void Destroy()
{
    if (_Movie.ActorList.Getpos(this);
     != 0)
    {
        _Movie.ActorList.Deleteone(this);
    }
    gSpritemanager.SDestroy(myNum);
    myNum = null;
    myGfx = 0;
}

}
