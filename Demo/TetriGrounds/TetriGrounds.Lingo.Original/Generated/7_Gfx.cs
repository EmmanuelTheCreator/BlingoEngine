public class GfxBehavior : LingoSpriteBehavior
{
    public int myStartX;
    public int myStartY;

    public GfxBehavior(ILingoMovieEnvironment env) : base(env)
    {
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
