using System;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Movies;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.Sounds;
using LingoEngine.SDL2.Texts;
using LingoEngine.SDL2.Shapes;
using LingoEngine.Gfx;
using LingoEngine.SDL2.Gfx;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Styles;
using LingoEngine.SDL2.Stages;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.Scripts;
using LingoEngine.SDL2.Scripts;
using LingoEngine.FilmLoops;
using LingoEngine.SDL2;

namespace LingoEngine.SDL2.Core;
/// <inheritdoc/>
public class SdlFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly ILingoServiceProvider _serviceProvider;
    private readonly SdlRootContext _rootContext;
    private readonly SdlGfxFactory _gfxFactory;
    /// <inheritdoc/>
    public SdlFactory(ILingoServiceProvider serviceProvider, SdlRootContext rootContext)
    {
        _serviceProvider = serviceProvider;
        _rootContext = rootContext;
        _rootContext.Factory = this;
        _gfxFactory = new SdlGfxFactory(_rootContext, _serviceProvider.GetRequiredService<ILingoFontManager>());
    }

    public ILingoGfxFactory GfxFactory => _gfxFactory;
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
            _ => throw new NotSupportedException()
        };
    }
    /// <inheritdoc/>
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberSound();
        var member = new LingoMemberSound(impl, (LingoCast)cast, numberInCast, name, fileName ?? "");
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberFilmLoop(_rootContext);
        var member = new LingoFilmLoopMember(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberShape();
        var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberBitmap();
        var member = new LingoMemberBitmap((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberField(_serviceProvider.GetRequiredService<ILingoFontManager>());
        var member = new LingoMemberField((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberText(_serviceProvider.GetRequiredService<ILingoFontManager>());
        var member = new LingoMemberText((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    /// <inheritdoc/>
    public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
           LingoPoint regPoint = default)
    {
        var godotInstance = new SdlFrameworkMemberScript();
        var lingoInstance = new LingoMemberScript(godotInstance, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        return lingoInstance;
    }
    /// <inheritdoc/>
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
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

    internal SdlRootContext RootContext => _rootContext;

    public LingoSDLComponentContainer ComponentContainer => _rootContext.ComponentContainer;

    public LingoSDLComponentContext CreateContext(ILingoSDLComponent component, LingoSDLComponentContext? parent = null)
        => _gfxFactory.CreateContext(component, parent);

    public LingoSDLRenderContext CreateRenderContext(ILingoSDLComponent? component = null)
        => _gfxFactory.CreateRenderContext(component);
    /// <inheritdoc/>
    public LingoStageMouse CreateMouse(LingoStage stage)
    {
        var mouseImpl = _rootContext.Mouse;
        var mouse = new LingoStageMouse(stage, mouseImpl);
        mouseImpl.SetLingoMouse(mouse);
        return mouse;
    }
    public LingoKey CreateKey() => _rootContext.LingoKey;


    #region Gfx elements
    /// <inheritdoc/>
    public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
        => _gfxFactory.CreateGfxCanvas(name, width, height);

    public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
        => _gfxFactory.CreateWrapPanel(orientation, name);

    public LingoGfxPanel CreatePanel(string name)
        => _gfxFactory.CreatePanel(name);

    public LingoGfxTabContainer CreateTabContainer(string name)
        => _gfxFactory.CreateTabContainer(name);

    public LingoGfxTabItem CreateTabItem(string name, string title)
        => _gfxFactory.CreateTabItem(name, title);

    public LingoGfxScrollContainer CreateScrollContainer(string name)
        => _gfxFactory.CreateScrollContainer(name);

    public LingoGfxInputSlider<float> CreateInputSliderFloat(LingoOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        => _gfxFactory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);

    public LingoGfxInputSlider<int> CreateInputSliderInt(LingoOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        => _gfxFactory.CreateInputSliderInt(orientation, name, min, max, step, onChange);

    public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
        => _gfxFactory.CreateInputText(name, maxLength, onChange);

    public LingoGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        => _gfxFactory.CreateInputNumberFloat(name, min, max, onChange);

    public LingoGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
        => _gfxFactory.CreateInputNumberInt(name, min, max, onChange);

    public LingoGfxInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
        where TValue : System.Numerics.INumber<TValue>
        => _gfxFactory.CreateInputNumber(name, min, max, onChange);

    public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        => _gfxFactory.CreateSpinBox(name, min, max, onChange);

    public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        => _gfxFactory.CreateInputCheckbox(name, onChange);

    public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        => _gfxFactory.CreateInputCombobox(name, onChange);

    public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
        => _gfxFactory.CreateItemList(name, onChange);

    public LingoGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null)
        => _gfxFactory.CreateColorPicker(name, onChange);

    public LingoGfxLabel CreateLabel(string name, string text = "")
        => _gfxFactory.CreateLabel(name, text);

    public LingoGfxButton CreateButton(string name, string text = "")
        => _gfxFactory.CreateButton(name, text);

    public LingoGfxStateButton CreateStateButton(string name, Bitmaps.ILingoImageTexture? texture = null, string text = "", Action<bool>? onChange = null)
        => _gfxFactory.CreateStateButton(name, texture, text, onChange);

    public LingoGfxMenu CreateMenu(string name)
        => _gfxFactory.CreateMenu(name);

    public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
        => _gfxFactory.CreateMenuItem(name, shortcut);

    public LingoGfxMenu CreateContextMenu(object window)
        => _gfxFactory.CreateContextMenu(window);

    public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y)
        => _gfxFactory.CreateLayoutWrapper(content, x, y);

    public LingoGfxWindow CreateWindow(string name, string title = "")
        => _gfxFactory.CreateWindow(name, title);


    public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        => _gfxFactory.CreateHorizontalLineSeparator(name);

    public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
        => _gfxFactory.CreateVerticalLineSeparator(name);
    #endregion


}