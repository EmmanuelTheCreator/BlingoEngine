using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using LingoEngine.Shapes;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Unity.Inputs;
using UnityEngine;
using LingoEngine.Unity.Stages;
using LingoEngine.Unity.Bitmaps;
using LingoEngine.Unity.Movies;
using LingoEngine.Unity.Texts;
using LingoEngine.Unity.Sounds;
using LingoEngine.Unity.FilmLoops;
using LingoEngine.Unity.Shapes;
using LingoEngine.Unity.Scripts;
using LingoEngine.Scripts;
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

namespace LingoEngine.Unity.Core;

public class UnityFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly ILingoServiceProvider _serviceProvider;
    private readonly AbstUnityComponentFactory _gfxFactory;

    public UnityFactory(ILingoServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _gfxFactory = new AbstUnityComponentFactory(serviceProvider);
    }

    public IAbstComponentFactory GfxFactory => _gfxFactory;

    public IAbstComponentFactory ComponentFactory => _gfxFactory;

    public LingoStage CreateStage(LingoPlayer lingoPlayer)
    {
        var go = new GameObject("LingoStage");
        var stageImpl = go.AddComponent<UnityStage>();
        stageImpl.Configure((LingoClock)lingoPlayer.Clock);
        var stage = new LingoStage(stageImpl);
        stageImpl.Init(stage);
        _disposables.Add(stageImpl);
        return stage;
    }

    public LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie)
    {
        var unityStage = stage.Framework<UnityStage>();
        var impl = new UnityMovie(unityStage, lingoMovie, m => _disposables.Remove(m));
        lingoMovie.Init(impl);
        _disposables.Add(impl);
        return lingoMovie;
    }

    #region Members
    public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember
    {
        return typeof(T) switch
        {
            Type t when t == typeof(LingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
            _ => throw new NotSupportedException()
        };
    }
    public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityMemberBitmap(_serviceProvider.GetRequiredService<ILogger<UnityMemberBitmap>>());
        var member = new LingoMemberBitmap((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityMemberSound();
        var member = new LingoMemberSound(impl, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty);
        impl.Init(member);
        return member;
    }
    public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityFilmLoopMember();
        var member = new LingoFilmLoopMember(impl, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityMemberShape();
        var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
        var logger = _serviceProvider.GetRequiredService<ILogger<UnityMemberField>>();
        var impl = new UnityMemberField(fontManager, logger);
        var member = new LingoMemberField((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
        var logger = _serviceProvider.GetRequiredService<ILogger<UnityMemberText>>();
        var impl = new UnityMemberText(fontManager, logger);
        var member = new LingoMemberText((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityFrameworkMemberScript();
        var member = new LingoMemberScript(impl, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        return member;
    }
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new UnityFrameworkMemberEmpty();
        var member = new LingoMember(impl, LingoMemberType.Empty, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        return member;
    }
    #endregion

    public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer)
    {
        var impl = new UnitySound();
        var sound = new LingoSound(impl, castLibsContainer, this);
        impl.Init(sound);
        return sound;
    }
    public LingoSoundChannel CreateSoundChannel(int number)
    {
        var impl = new UnitySoundChannel(number);
        var channel = new LingoSoundChannel(impl, number);
        impl.Init(channel);
        return channel;
    }

    public LingoStageMouse CreateMouse(LingoStage stage)
    {
        var mouseImpl = new LingoUnityMouse(new Lazy<LingoMouse>(() => null!));
        var mouse = new LingoStageMouse(stage, mouseImpl);
        mouseImpl.SetMouseObj(mouse);
        return mouse;
    }

    public LingoKey CreateKey()
    {
        LingoKey? key = null;
        var impl = new LingoUnityKey();
        key = new LingoKey(impl);
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

    public LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe)
    {
        var lingoMovie = (LingoMovie)movie;
        var sprite = new LingoSprite2D(lingoMovie.GetEnvironment(), movie);
        sprite.SetOnRemoveMe(onRemoveMe);
        lingoMovie.Framework<UnityMovie>().CreateSprite(sprite);
        return sprite;
    }

    public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior
        => lingoMovie.GetServiceProvider().GetRequiredService<T>();
    public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript
        => lingoMovie.GetServiceProvider().GetRequiredService<T>();

    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }
}
