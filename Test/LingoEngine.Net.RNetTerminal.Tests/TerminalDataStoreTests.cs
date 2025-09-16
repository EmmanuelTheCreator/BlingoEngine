using System;
using System.Linq;
using FluentAssertions;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetTerminal;

namespace LingoEngine.Net.RNetTerminal.Tests;

public class TerminalDataStoreTests
{
    [Fact]
    public void ApplySpriteDelta_RemoteMove_RepositionsSpriteAndSelection()
    {
        var store = TerminalDataStore.Instance;
        store.LoadTestData();

        var sprite = store.GetSprites().First(s => s.Member is not null);
        var originalBegin = sprite.BeginFrame;
        var originalEnd = sprite.EndFrame;
        var originalApplyLocalChanges = store.ApplyLocalChanges;
        store.SelectSprite(new SpriteRef(sprite.SpriteNum, originalBegin));

        var deltaFrames = Math.Min(5, Math.Max(1, store.FrameCount - originalBegin - 1));
        if (deltaFrames == 0)
        {
            deltaFrames = 1;
        }

        SpriteRef? requestedSprite = null;
        int? requestedBegin = null;
        int? requestedEnd = null;

        void Handler(SpriteRef s, int begin, int end)
        {
            requestedSprite = s;
            requestedBegin = begin;
            requestedEnd = end;
        }

        store.ApplyLocalChanges = false;
        store.SpriteMoveRequested += Handler;

        try
        {
            store.MoveSprite(new SpriteRef(sprite.SpriteNum, originalBegin), deltaFrames);
        }
        finally
        {
            store.SpriteMoveRequested -= Handler;
        }

        sprite.BeginFrame.Should().Be(originalBegin);
        sprite.EndFrame.Should().Be(originalEnd);

        requestedSprite.Should().NotBeNull();
        requestedSprite!.Value.SpriteNum.Should().Be(sprite.SpriteNum);
        requestedSprite.Value.BeginFrame.Should().Be(originalBegin);
        requestedBegin.Should().NotBeNull();
        requestedEnd.Should().NotBeNull();

        var delta = new SpriteDeltaDto(
            requestedBegin!.Value,
            sprite.SpriteNum,
            requestedBegin.Value,
            sprite.LocZ,
            sprite.Member!.CastLibNum,
            sprite.Member.MemberNum,
            (int)Math.Round(sprite.LocH),
            (int)Math.Round(sprite.LocV),
            (int)Math.Round(sprite.Width),
            (int)Math.Round(sprite.Height),
            (int)Math.Round(sprite.Rotation),
            (int)Math.Round(sprite.Skew),
            (int)Math.Round(sprite.Blend),
            sprite.Ink);

        store.ApplySpriteDelta(delta);

        sprite.BeginFrame.Should().Be(requestedBegin.Value);
        sprite.EndFrame.Should().Be(requestedEnd.Value);
        store.FindSprite(new SpriteRef(sprite.SpriteNum, requestedBegin.Value)).Should().BeSameAs(sprite);

        var selection = store.GetSelectedSprite();
        selection.Should().NotBeNull();
        selection!.Value.SpriteNum.Should().Be(sprite.SpriteNum);
        selection.Value.BeginFrame.Should().Be(requestedBegin.Value);

        store.ApplyLocalChanges = originalApplyLocalChanges;
        store.LoadTestData();
    }
}
