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
    gSpriteManager.Destroy();
    gMousePointer.Destroy();
    gMousePointer = null;
    // error
    actorlist = [];
}

public void ReplaceSpaces(object str, object leng)
{
    // ------------------------------------
    // replace spaces with underscore
    thisField = str;
    for (var i = 1; i <= thisField.Length; i++)
    {
        if (thisField.Char[i] == " ")
        {
            thisField = (thisField.Char[1..i - 1] + "_") + thisField.Char[i + 1..thisField.Length];
        }
    }
    // thisField = thisField.char[1..thisField.length-1] -- remove last underscore
    if (thisField.Length > leng)
    {
        thisField = thisField.Char[1..leng];
    }
    // ------------------------------------
    return thisField;
}

}
