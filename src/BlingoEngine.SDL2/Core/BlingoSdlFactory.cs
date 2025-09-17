using System;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Medias;
using BlingoEngine.SDL2.Movies;
using BlingoEngine.SDL2.Sounds;
using BlingoEngine.SDL2.Texts;
using BlingoEngine.SDL2.Shapes;
using BlingoEngine.SDL2.Medias;
using BlingoEngine.Sounds;
using BlingoEngine.Shapes;
using BlingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using AbstUI.Styles;
using BlingoEngine.SDL2.Stages;
using BlingoEngine.SDL2.Inputs;
using BlingoEngine.Bitmaps;
using BlingoEngine.Scripts;
using BlingoEngine.SDL2.Scripts;
using BlingoEngine.FilmLoops;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.SDL2.Styles;
using BlingoEngine.SDL2.Bitmaps;
using BlingoEngine.SDL2.FilmLoops;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Medias;

namespace BlingoEngine.SDL2.Core;
/// <inheritdoc/>
public class BlingoSdlFactory : IBlingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IBlingoServiceProvider _serviceProvider;
    private readonly BlingoSdlRootContext _rootContext;
    private readonly SdlFontManager _fontManager;
    private readonly IAbstStyleManager _styleManager;
    private readonly AbstSdlComponentFactory _gfxFactory;
    internal SdlFontManager FontManager => _fontManager;
    internal BlingoSdlRootContext RootContext => _rootContext;

    public AbstSDLComponentContainer ComponentContainer => _rootContext.ComponentContainer;

    public IAbstComponentFactory ComponentFactory => _gfxFactory;
    /// <inheritdoc/>
    public BlingoSdlFactory(IBlingoServiceProvider serviceProvider, BlingoSdlRootContext rootContext)
    {
        _serviceProvider = serviceProvider;
        _rootContext = rootContext;
        _fontManager = (SdlFontManager)_serviceProvider.GetRequiredService<IAbstFontManager>();
        _styleManager = _serviceProvider.GetRequiredService<IAbstStyleManager>();
        _gfxFactory = (AbstSdlComponentFactory)serviceProvider.GetRequiredService<IAbstComponentFactory>();
        _rootContext.Factory = _gfxFactory;
        _rootContext.BlingoFactory = this;
    }

    public IAbstComponentFactory GfxFactory => _gfxFactory;
    /// <inheritdoc/>
 

    #region Sound
    /// <inheritdoc/>
    public BlingoSound CreateSound(IBlingoCastLibsContainer castLibsContainer)
    {
        var impl = new SdlSound();
        var sound = new BlingoSound(impl, castLibsContainer, this);
        impl.Init(sound);
        return sound;
    }
    /// <inheritdoc/>
    public BlingoSoundChannel CreateSoundChannel(int number)
    {
        var impl = new SdlSoundChannel(number);
        var channel = new BlingoSoundChannel(impl, number);
        impl.Init(channel);
        _disposables.Add(impl);
        return channel;
    }
    #endregion

    #region Members
    /// <inheritdoc/>
    public T CreateMember<T>(IBlingoCast cast, int numberInCast, string name = "") where T : BlingoMember
    {
        return typeof(T) switch
        {
            Type t when t == typeof(BlingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberQuickTimeMedia) => (CreateMemberQuickTimeMedia(cast, numberInCast, name) as T)!,
            Type t when t == typeof(BlingoMemberRealMedia) => (CreateMemberRealMedia(cast, numberInCast, name) as T)!,
            _ => throw new NotSupportedException()
        };
    }
    /// <inheritdoc/>
    public BlingoMemberSound CreateMemberSound(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberSound();
        var member = new BlingoMemberSound(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "");
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public BlingoFilmLoopMember CreateMemberFilmLoop(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberFilmLoop(_rootContext);
        var member = new BlingoFilmLoopMember(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMemberShape CreateMemberShape(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberShape(_rootContext);
        var member = new BlingoMemberShape((BlingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMemberQuickTimeMedia CreateMemberQuickTimeMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var player = _serviceProvider.GetRequiredService<ISDLMediaPlayer>();
        var impl = new SdlMemberMedia(player);
        var member = new BlingoMemberQuickTimeMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        if (player is IDisposable disposable) _disposables.Add(disposable);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMemberRealMedia CreateMemberRealMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var player = _serviceProvider.GetRequiredService<ISDLMediaPlayer>();
        var impl = new SdlMemberMedia(player);
        var member = new BlingoMemberRealMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        if (player is IDisposable disposable) _disposables.Add(disposable);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMemberBitmap CreateMemberBitmap(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberBitmap(_rootContext);
        var member = new BlingoMemberBitmap((BlingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMemberField CreateMemberField(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberField(_serviceProvider.GetRequiredService<IAbstFontManager>(), _rootContext);
        var member = new BlingoMemberField((BlingoCast)cast, impl, numberInCast, ComponentFactory, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMemberText CreateMemberText(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlMemberText(_serviceProvider.GetRequiredService<IAbstFontManager>(), _rootContext);
        var member = new BlingoMemberText((BlingoCast)cast, impl, numberInCast,ComponentFactory, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public BlingoMember CreateScript(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null,
           APoint regPoint = default)
    {
        var godotInstance = new SdlFrameworkMemberScript();
        var blingoInstance = new BlingoMemberScript(godotInstance, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        return blingoInstance;
    }
    /// <inheritdoc/>
    public BlingoMember CreateEmpty(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new SdlFrameworkMemberEmpty();
        var member = new BlingoMember(impl, BlingoMemberType.Empty, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        return member;
    }
    #endregion
    /// <inheritdoc/>
    public BlingoStage CreateStage(BlingoPlayer player)
    {
        _rootContext.Init(player);
        var impl = new SdlStage(_rootContext, (BlingoClock)player.Clock, this);
        var stage = new BlingoStage(impl);
        impl.Init(stage);
        _disposables.Add(impl);
        return stage;
    }
    /// <inheritdoc/>
    public BlingoMovie AddMovie(BlingoStage stage, BlingoMovie movie)
    {
        var sdlStage = stage.Framework<SdlStage>();
        var impl = new SdlMovie(sdlStage, this, movie, m => _disposables.Remove(m));
        movie.Init(impl);
        _disposables.Add(impl);
        return movie;
    }
    /// <inheritdoc/>
    public BlingoSprite2D CreateSprite2D(IBlingoMovie movie, Action<BlingoSprite2D> onRemoveMe)
    {
        var movieTyped = (BlingoMovie)movie;
        var blingoSprite = new BlingoSprite2D(((BlingoMovie)movie).GetEnvironment(), movie);
        blingoSprite.SetOnRemoveMe(onRemoveMe);
        movieTyped.Framework<SdlMovie>().CreateSprite(blingoSprite);
        return blingoSprite;
    }
    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }



    public AbstSDLComponentContext CreateContext(IAbstSDLComponent component, AbstSDLComponentContext? parent = null)
        => _gfxFactory.CreateContext(component, parent);

    public AbstSDLRenderContext CreateRenderContext(AbstSDLRenderContext? parent, System.Numerics.Vector2 origin)
        => _gfxFactory.CreateRenderContext(parent, origin);
    /// <inheritdoc/>
    public BlingoStageMouse CreateMouse(BlingoStage stage)
    {
        var mouseImpl = (BlingoSdlMouse)_rootContext.Mouse;
        var mouse = new BlingoStageMouse(stage, mouseImpl);
        mouseImpl.SetMouse(mouse);
        return mouse;
    }
    public BlingoKey CreateKey() => (BlingoKey)_rootContext.AbstKey;


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
