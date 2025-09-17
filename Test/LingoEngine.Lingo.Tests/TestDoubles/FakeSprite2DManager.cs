using System.Collections.Generic;
using System.Runtime.Serialization;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal sealed class FakeSprite2DManager : LingoSprite2DManager
{
    private List<string> _timeline = null!;
    private bool _shouldBegin;
    private bool _shouldEnd;
    private bool _hasActivated;
    private int _activationFrame;

    private FakeSprite2DManager() : base(null!, null!)
    {
    }

    internal static FakeSprite2DManager Create(LingoMovie movie, LingoStageMouse mouse, List<string> timeline)
    {
        var manager = (FakeSprite2DManager)FormatterServices.GetUninitializedObject(typeof(FakeSprite2DManager));
        manager.Initialize(movie, mouse, timeline);
        return manager;
    }

    private void Initialize(LingoMovie movie, LingoStageMouse mouse, List<string> timeline)
    {
        _timeline = timeline;
        _shouldBegin = false;
        _shouldEnd = false;
        _hasActivated = false;
        _activationFrame = 1;

        PrivateFieldSetter.SetField(this, "_movie", movie);
        PrivateFieldSetter.SetField(this, "_environment", null);
        PrivateFieldSetter.SetField(this, "_lingoMouse", mouse);
        PrivateFieldSetter.SetField(this, "_mutedSprites", new List<int>());
        SpriteNumChannelOffset = LingoSprite2D.SpriteNumOffset;

        PrivateFieldSetter.SetField(this, "_spriteChannels", new Dictionary<int, LingoSpriteChannel>());
        PrivateFieldSetter.SetField(this, "_spritesByName", new Dictionary<string, LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_allTimeSprites", new List<LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_newPuppetSprites", new List<LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_activeSprites", new Dictionary<int, LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_activeSpritesOrdered", new List<LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_enteredSprites", new List<LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_exitedSprites", new List<LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_deletedPuppetSpritesCache", new Dictionary<int, LingoSprite2D>());
        PrivateFieldSetter.SetField(this, "_changedMembers", new List<LingoMember>());
    }

    internal int ActivationFrame
    {
        get => _activationFrame;
        set
        {
            _activationFrame = value;
            _hasActivated = false;
            _shouldBegin = false;
            _shouldEnd = false;
        }
    }

    internal override void UpdateActiveSprites(int currentFrame, int lastFrame)
    {
        if (!_hasActivated && currentFrame >= _activationFrame)
        {
            _shouldBegin = true;
            _shouldEnd = true;
            _hasActivated = true;
        }
    }

    internal override void BeginSprites()
    {
        if (_shouldBegin)
        {
            _timeline.Add("beginSprite");
            _shouldBegin = false;
        }
    }

    internal override void PrepareEndSprites()
    {
    }

    internal override void DispatchEndSprites()
    {
        if (_shouldEnd)
        {
            _timeline.Add("endSprite");
            _shouldEnd = false;
        }
    }
}
