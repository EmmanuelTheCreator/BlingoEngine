using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using BlingoEngine.Shapes;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using BlingoEngine.Bitmaps;
using BlingoEngine.FilmLoops;
using BlingoEngine.Unity.Inputs;
using UnityEngine;
using BlingoEngine.Unity.Stages;
using BlingoEngine.Unity.Bitmaps;
using BlingoEngine.Unity.Movies;
using BlingoEngine.Unity.Texts;
using BlingoEngine.Unity.Sounds;
using BlingoEngine.Unity.FilmLoops;
using BlingoEngine.Unity.Shapes;
using BlingoEngine.Unity.Scripts;
using BlingoEngine.Scripts;
using BlingoEngine.Medias;
using BlingoEngine.Unity.Medias;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.LUnity.Components;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Unity.Core;

public class UnityFactory : IBlingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IBlingoServiceProvider _serviceProvider;
    private readonly AbstUnityComponentFactory _gfxFactory;

    public UnityFactory(IBlingoServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _gfxFactory = new AbstUnityComponentFactory(serviceProvider);
    }

    public IAbstComponentFactory GfxFactory => _gfxFactory;

    public IAbstComponentFactory ComponentFactory => _gfxFactory;

    public BlingoStage CreateStage(BlingoPlayer blingoPlayer)
    {
        var go = new GameObject("BlingoStage");
        var stageImpl = go.AddComponent<UnityStage>();
        stageImpl.Configure((BlingoClock)blingoPlayer.Clock);
        var stage = new BlingoStage(stageImpl);
        stageImpl.Init(stage);
        _disposables.Add(stageImpl);
        return stage;
    }

    public BlingoMovie AddMovie(BlingoStage stage, BlingoMovie blingoMovie)
    {
        var unityStage = stage.Framework<UnityStage>();
        var impl = new UnityMovie(unityStage, blingoMovie, m => _disposables.Remove(m));
        blingoMovie.Init(impl);
        _disposables.Add(impl);
        return blingoMovie;
    }

    #region Members
    public T CreateMember<T>(IBlingoCast cast, int numberInCast, string name = "") where T : BlingoMember
    {
        return typeof(T) switch
        {
            Type t when t == typeof(BlingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberShape) => (CreateMemberShape(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberQuickTimeMedia) => (CreateMemberQuickTimeMedia(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberRealMedia) => (CreateMemberRealMedia(cast, numberInCast, name) as T)!,
            _ => throw new NotSupportedException()
        };
    }
    public BlingoMemberBitmap CreateMemberBitmap(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityMemberBitmap(_serviceProvider.GetRequiredService<ILogger<UnityMemberBitmap>>());
        var member = new BlingoMemberBitmap((BlingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMemberSound CreateMemberSound(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityMemberSound();
        var member = new BlingoMemberSound(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty);
        impl.Init(member);
        return member;
    }
    public BlingoFilmLoopMember CreateMemberFilmLoop(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityFilmLoopMember();
        var member = new BlingoFilmLoopMember(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMemberShape CreateMemberShape(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityMemberShape();
        var member = new BlingoMemberShape((BlingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMemberQuickTimeMedia CreateMemberQuickTimeMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new BlingoUnityMemberMedia();
        var member = new BlingoMemberQuickTimeMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMemberRealMedia CreateMemberRealMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new BlingoUnityMemberMedia();
        var member = new BlingoMemberRealMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMemberField CreateMemberField(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
        var logger = _serviceProvider.GetRequiredService<ILogger<UnityMemberField>>();
        var impl = new UnityMemberField(fontManager, logger);
        var member = new BlingoMemberField((BlingoCast)cast, impl, numberInCast, ComponentFactory, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMemberText CreateMemberText(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
        var logger = _serviceProvider.GetRequiredService<ILogger<UnityMemberText>>();
        var impl = new UnityMemberText(fontManager, logger);
        var member = new BlingoMemberText((BlingoCast)cast, impl, numberInCast, ComponentFactory, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMember CreateScript(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityFrameworkMemberScript();
        var member = new BlingoMemberScript(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        return member;
    }
    public BlingoMember CreateEmpty(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityFrameworkMemberEmpty();
        var member = new BlingoMember(impl, BlingoMemberType.Empty, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        return member;
    }
    #endregion

    public BlingoSound CreateSound(IBlingoCastLibsContainer castLibsContainer)
    {
        var impl = new UnitySound();
        var sound = new BlingoSound(impl, castLibsContainer, this);
        impl.Init(sound);
        return sound;
    }
    public BlingoSoundChannel CreateSoundChannel(int number)
    {
        var impl = new UnitySoundChannel(number);
        var channel = new BlingoSoundChannel(impl, number);
        impl.Init(channel);
        return channel;
    }

    public BlingoStageMouse CreateMouse(BlingoStage stage)
    {
        var mouseImpl = new BlingoUnityMouse(new Lazy<BlingoMouse>(() => null!));
        var mouse = new BlingoStageMouse(stage, mouseImpl);
        mouseImpl.SetMouseObj(mouse);
        return mouse;
    }

    public BlingoKey CreateKey()
    {
        BlingoKey? key = null;
        var impl = new BlingoUnityKey();
        key = new BlingoKey(impl);
        impl.SetKeyObj(key);
        return key;
    }

    #region Gfx
    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height) => _gfxFactory.CreateGfxCanvas(name, width, height);
    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name) => _gfxFactory.CreateWrapPanel(orientation, name);
    public AbstPanel CreatePanel(string name) => _gfxFactory.CreatePanel(name);
    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y) => _gfxFactory.CreateLayoutWrapper(content, x, y);
    public AbstTabContainer CreateTabContainer(string name) => _gfxFactory.CreateTabContainer(name);
    public AbstTabItem CreateTabItem(string name, string title) => _gfxFactory.CreateTabItem(name, title);
    public AbstScrollContainer CreateScrollContainer(string name) => _gfxFactory.CreateScrollContainer(name);
    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        => _gfxFactory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);
    public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        => _gfxFactory.CreateInputSliderInt(orientation, name, min, max, step, onChange);
    public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
        => _gfxFactory.CreateInputText(name, maxLength, onChange, multiLine);
    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        => _gfxFactory.CreateInputNumberFloat(name, min, max, onChange);
    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
        => _gfxFactory.CreateInputNumberInt(name, min, max, onChange);
    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        => _gfxFactory.CreateSpinBox(name, min, max, onChange);
    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null) => _gfxFactory.CreateInputCheckbox(name, onChange);
    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null) => _gfxFactory.CreateInputCombobox(name, onChange);
    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
        => _gfxFactory.CreateItemList(name, onChange);
    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
        => _gfxFactory.CreateColorPicker(name, onChange);
    public AbstLabel CreateLabel(string name, string text = "") => _gfxFactory.CreateLabel(name, text);
    public AbstButton CreateButton(string name, string text = "") => _gfxFactory.CreateButton(name, text);
    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
        => _gfxFactory.CreateStateButton(name, texture, text, onChange);
    public AbstMenu CreateMenu(string name)
        => _gfxFactory.CreateMenu(name);
    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
        => _gfxFactory.CreateMenuItem(name, shortcut);
    public AbstMenu CreateContextMenu(object window)
        => _gfxFactory.CreateContextMenu(window);
    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        => _gfxFactory.CreateHorizontalLineSeparator(name);
    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
        => _gfxFactory.CreateVerticalLineSeparator(name);
    //public AbstWindow CreateWindow(string name, string title = "") => throw new NotImplementedException();
    #endregion

    public BlingoSprite2D CreateSprite2D(IBlingoMovie movie, Action<BlingoSprite2D> onRemoveMe)
    {
        var blingoMovie = (BlingoMovie)movie;
        var sprite = new BlingoSprite2D(blingoMovie.GetEnvironment(), movie);
        sprite.SetOnRemoveMe(onRemoveMe);
        blingoMovie.Framework<UnityMovie>().CreateSprite(sprite);
        return sprite;
    }

  
    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }
}

