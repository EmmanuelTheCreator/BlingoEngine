public class GfxParentScript : LingoParentScript
{
    public int myStartX;
    public int myStartY;

    private readonly GlobalVars _global;

    public GfxParentScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
        myStartX = 250;
        myStartY = 45;
    }
public void PositionBlock(object _sprNum, object _X, object _Y)
{
    if (_sprNum == null)
    {
        return;
    }
    xx = (myStartX + _X) * 17;
    yy = (myStartY + _Y) * 17;
    Sprite(_sprNum).LocH = xx;
    Sprite(_sprNum).LocV = yy;
}

public void Destroy()
{
}

}
