using System;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Medias;
using LingoEngine.SDL2.Movies;
using LingoEngine.SDL2.Sounds;
using LingoEngine.SDL2.Texts;
using LingoEngine.SDL2.Shapes;
using LingoEngine.SDL2.Medias;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using AbstUI.Styles;
using LingoEngine.SDL2.Stages;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.Scripts;
using LingoEngine.SDL2.Scripts;
using LingoEngine.FilmLoops;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.SDL2.Styles;
using LingoEngine.SDL2.Bitmaps;
using LingoEngine.SDL2.FilmLoops;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;

namespace LingoEngine.SDL2.Core;
/// <inheritdoc/>
public class LingoSdlFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly ILingoServiceProvider _serviceProvider;
    private readonly LingoSdlRootContext _rootContext;
    private readonly SdlFontManager _fontManager;
    private readonly IAbstStyleManager _styleManager;
    private readonly AbstSdlComponentFactory _gfxFactory;
    internal SdlFontManager FontManager => _fontManager;
    internal LingoSdlRootContext RootContext => _rootContext;

    public AbstSDLComponentContainer ComponentContainer => _rootContext.ComponentContainer;

    public IAbstComponentFactory ComponentFactory => _gfxFactory;
    /// <inheritdoc/>
    public LingoSdlFactory(ILingoServiceProvider serviceProvider, LingoSdlRootContext rootContext)
    {
        _serviceProvider = serviceProvider;
        _rootContext = rootContext;
        _fontManager = (SdlFontManager)_serviceProvider.GetRequiredService<IAbstFontManager>();
        _styleManager = _serviceProvider.GetRequiredService<IAbstStyleManager>();
        _gfxFactory = (AbstSdlComponentFactory)serviceProvider.GetRequiredService<IAbstComponentFactory>();
        _rootContext.Factory = _gfxFactory;
        _rootContext.LingoFactory = this;
    }

    public IAbstComponentFactory GfxFactory => _gfxFactory;
    /// <inheritdoc/>
    public T CreateBehavior<T>(LingoMovie movie) where T : LingoSpriteBehavior
        => movie.GetServiceProvider().GetRequiredService<T>();
    /// <inheritdoc/>
    public T CreateMovieScript<T>(LingoMovie movie) where T : LingoMovieScript
        => movie.GetServiceProvider().GetRequiredService<T>();

    #region Sound
    /// <inheritdoc/>
    public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer)
    {
        var impl = new SdlSound();
        var sound = new LingoSound(impl, castLibsContainer, this);
        impl.Init(sound);
        return sound;
    }
    /// <inheritdoc/>
    public LingoSoundChannel CreateSoundChannel(int number)
    {
        var impl = new SdlSoundChannel(number);
        var channel = new LingoSoundChannel(impl, number);
        impl.Init(channel);
        _disposables.Add(impl);
        return channel;
    }
    #endregion

    #region Members
    /// <inheritdoc/>
    public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember
    {
        return typeof(T) switch
        {
            Type t when t == typeof(LingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberQuickTimeMedia) => (CreateMemberQuickTimeMedia(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberRealMedia) => (CreateMemberRealMedia(cast, numberInCast, name) as T)!,
            _ => throw new NotSupportedException()
        };
    }
    /// <inheritdoc/>
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberSound();
        var member = new LingoMemberSound(impl, (LingoCast)cast, numberInCast, name, fileName ?? "");
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberFilmLoop(_rootContext);
        var member = new LingoFilmLoopMember(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberShape(_rootContext);
        var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberQuickTimeMedia CreateMemberQuickTimeMedia(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var player = _serviceProvider.GetRequiredService<ISDLMediaPlayer>();
        var impl = new SdlMemberMedia(player);
        var member = new LingoMemberQuickTimeMedia(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        if (player is IDisposable disposable) _disposables.Add(disposable);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberRealMedia CreateMemberRealMedia(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var player = _serviceProvider.GetRequiredService<ISDLMediaPlayer>();
        var impl = new SdlMemberMedia(player);
        var member = new LingoMemberRealMedia(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        if (player is IDisposable disposable) _disposables.Add(disposable);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberBitmap(_rootContext);
        var member = new LingoMemberBitmap((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberField(_serviceProvider.GetRequiredService<IAbstFontManager>(), _rootContext);
        var member = new LingoMemberField((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberText(_serviceProvider.GetRequiredService<IAbstFontManager>(), _rootContext);
        var member = new LingoMemberText((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
           APoint regPoint = default)
    {
        var godotInstance = new SdlFrameworkMemberScript();
        var lingoInstance = new LingoMemberScript(godotInstance, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        return lingoInstance;
    }
    /// <inheritdoc/>
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlFrameworkMemberEmpty();
        var member = new LingoMember(impl, LingoMemberType.Empty, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        return member;
    }
    #endregion
    /// <inheritdoc/>
    public LingoStage CreateStage(LingoPlayer player)
    {
        _rootContext.Init(player);
        var impl = new SdlStage(_rootContext, (LingoClock)player.Clock);
        var stage = new LingoStage(impl);
        impl.Init(stage);
        _disposables.Add(impl);
        return stage;
    }
    /// <inheritdoc/>
    public LingoMovie AddMovie(LingoStage stage, LingoMovie movie)
    {
        var sdlStage = stage.Framework<SdlStage>();
        var impl = new SdlMovie(sdlStage, this, movie, m => _disposables.Remove(m));
        movie.Init(impl);
        _disposables.Add(impl);
        return movie;
    }
    /// <inheritdoc/>
    public LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe)
    {
        var movieTyped = (LingoMovie)movie;
        var lingoSprite = new LingoSprite2D(((LingoMovie)movie).GetEnvironment(), movie);
        lingoSprite.SetOnRemoveMe(onRemoveMe);
        movieTyped.Framework<SdlMovie>().CreateSprite(lingoSprite);
        return lingoSprite;
    }
    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }



    public AbstSDLComponentContext CreateContext(IAbstSDLComponent component, AbstSDLComponentContext? parent = null)
        => _gfxFactory.CreateContext(component, parent);

    public AbstSDLRenderContext CreateRenderContext(IAbstSDLComponent? component = null)
        => _gfxFactory.CreateRenderContext(component);
    /// <inheritdoc/>
    public LingoStageMouse CreateMouse(LingoStage stage)
    {
        var mouseImpl = (LingoSdlMouse)_rootContext.Mouse;
        var mouse = new LingoStageMouse(stage, mouseImpl);
        mouseImpl.SetMouse(mouse);
        return mouse;
    }
    public LingoKey CreateKey() => (LingoKey)_rootContext.AbstKey;


    #region Gfx elements
    /// <inheritdoc/>
    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
        => _gfxFactory.CreateGfxCanvas(name, width, height);

    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
        => _gfxFactory.CreateWrapPanel(orientation, name);

    public AbstPanel CreatePanel(string name)
        => _gfxFactory.CreatePanel(name);

    public AbstTabContainer CreateTabContainer(string name)
        => _gfxFactory.CreateTabContainer(name);

    public AbstTabItem CreateTabItem(string name, string title)
        => _gfxFactory.CreateTabItem(name, title);

    public AbstScrollContainer CreateScrollContainer(string name)
        => _gfxFactory.CreateScrollContainer(name);

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

    public AbstInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
        where TValue : System.Numerics.INumber<TValue>
        => _gfxFactory.CreateInputNumber(name, min, max, onChange);

    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        => _gfxFactory.CreateSpinBox(name, min, max, onChange);

    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        => _gfxFactory.CreateInputCheckbox(name, onChange);

    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        => _gfxFactory.CreateInputCombobox(name, onChange);

    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
        => _gfxFactory.CreateItemList(name, onChange);

    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
        => _gfxFactory.CreateColorPicker(name, onChange);

    public AbstLabel CreateLabel(string name, string text = "")
        => _gfxFactory.CreateLabel(name, text);

    public AbstButton CreateButton(string name, string text = "")
        => _gfxFactory.CreateButton(name, text);

    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
        => _gfxFactory.CreateStateButton(name, texture, text, onChange);

    public AbstMenu CreateMenu(string name)
        => _gfxFactory.CreateMenu(name);

    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
        => _gfxFactory.CreateMenuItem(name, shortcut);

    public AbstMenu CreateContextMenu(object window)
        => _gfxFactory.CreateContextMenu(window);

    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
        => _gfxFactory.CreateLayoutWrapper(content, x, y);


    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        => _gfxFactory.CreateHorizontalLineSeparator(name);

    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
        => _gfxFactory.CreateVerticalLineSeparator(name);
    #endregion


}