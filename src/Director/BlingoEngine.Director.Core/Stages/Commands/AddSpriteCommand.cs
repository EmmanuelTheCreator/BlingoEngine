using BlingoEngine.Members;
using BlingoEngine.Movies;
using AbstUI.Commands;
using System;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Stages.Commands;

public sealed record AddSpriteCommand(
    BlingoMovie Movie,
    IBlingoMember Member,
    // Channel number is 1-based to match BlingoMovie
    int SpriteNumWithChannel,
    int BeginFrame,
    int EndFrame) : IAbstCommand
{
    public Action ToUndo(BlingoSprite sprite, Action refresh)
    {
        return () =>
        {
            sprite.RemoveMe();
            refresh();
        };
    }


    public Action ToRedo(Action refresh)
    {
        var movie = Movie;
        var member = Member;
        int channel = SpriteNumWithChannel;
        int begin = BeginFrame;
        int end = EndFrame;
        return () =>
        {
            movie.AddSpriteByChannelNum(channel, begin, end, member);
            refresh();
        };
    }
}

