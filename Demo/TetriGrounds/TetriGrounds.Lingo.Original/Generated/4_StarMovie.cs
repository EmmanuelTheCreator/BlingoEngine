public class StarMovieMovieScript : LingoMovieScript
{
    private readonly GlobalVars _global;

    public StarMovieMovieScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
public void Startmovie()
{
    if (gSpriteManager == null)
    {
        gSpriteManager = new SpriteManagerParentScript(_env, _globalvars, 100);
    }
    if (gMousePointer == null)
    {
        gMousePointer = new MousePointerParentScript(_env, _globalvars, 999);
    }
}

public void StopMovie()
{
    gSpriteManager.destroy();
    gMousePointer.destroy();
    gMousePointer = null;
    // error
    actorlist = [];
}

public void ReplaceSpaces(object str, object leng)
{
    // ------------------------------------
    // replace spaces with underscore
    thisField = str;
    for (var i = 1; i <= thisField.length; i++)
    {
        if (thisField.char[i] == " ")
        {
            thisField = (thisField.char[1..i - 1] + "_") + thisField.char[i + 1..thisField.length];
        }
    }
    // thisField = thisField.char[1..thisField.length-1] -- remove last underscore
    if (thisField.length > leng)
    {
        thisField = thisField.char[1..leng];
    }
    // ------------------------------------
    return thisField;
}

}
