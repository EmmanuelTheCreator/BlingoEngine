using System;
using BlingoEngine.Lingo.Core;

namespace Demo.TetriGrounds;

public class StarMovieMovieScript : BlingoMovieScript
{
    private readonly GlobalVars _global;

    public StarMovieMovieScript(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
public void StartMovie()
{
    if (gSpriteManager == null)
    {
        gSpriteManager = new SpriteManagerParent(_env, _globalvars, 100);
    }
    if (gMousePointer == null)
    {
        gMousePointer = new MousePointerParent(_env, _globalvars, 999);
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

public void StartMovie()
{
    if (gSpriteManager == null)
    {
        gSpriteManager = new SpriteManagerParent(_env, _globalvars, 100);
    }
    if (gMousePointer == null)
    {
        gMousePointer = new MousePointerParent(_env, _globalvars, 999);
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
