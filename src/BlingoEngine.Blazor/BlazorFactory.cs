using System;
using System.Collections.Generic;
using AbstUI.Blazor;
using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using BlingoEngine.Bitmaps;
using BlingoEngine.Blazor.Inputs;
using BlingoEngine.Blazor.Movies;
using BlingoEngine.Blazor.Sprites;
using BlingoEngine.Blazor.Stages;
using BlingoEngine.Blazor.FilmLoops;
using BlingoEngine.Blazor.Bitmaps;
using BlingoEngine.Blazor.Shapes;
using BlingoEngine.Blazor.Sounds;
using BlingoEngine.Blazor.Texts;
using BlingoEngine.Blazor.Scripts;
using BlingoEngine.Blazor.Core;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.FilmLoops;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using BlingoEngine.Texts;
using BlingoEngine.Events;
using BlingoEngine.Scripts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Net.Http;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using BlingoEngine.Medias;
using BlingoEngine.Blazor.Medias;

namespace BlingoEngine.Blazor;

/// <summary>
/// Factory responsible for creating Blazor backed framework objects.
/// Only a subset of the factory is implemented, enough for stage, movie and
/// sprite handling which is required by the current tests.
/// </summary>
public class BlazorFactory : IBlingoFrameworkFactory, IDisposable
{
    private readonly IBlingoServiceProvider _services;
    private readonly List<IDisposable> _disposables = new();
    private readonly IAbstComponentFactory _gfxFactory;

    public IAbstComponentFactory ComponentFactory => _gfxFactory;

    public BlazorFactory(IBlingoServiceProvider services)
    {
        _services = services;

        var styleManager = _services.GetRequiredService<IAbstStyleManager>();
        var fontManager = _services.GetRequiredService<IAbstFontManager>();
        _gfxFactory = services.GetRequiredService<IAbstComponentFactory>();
    }

    public BlingoStage CreateStage(BlingoPlayer blingoPlayer)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var root = _services.GetRequiredService<BlingoBlazorRootPanel>();
        var container = _services.GetRequiredService<IBlingoFrameworkStageContainer>();
        var impl = new BlingoBlazorStage(blingoPlayer, js, scripts, root, _gfxFactory);
        container.SetStage(impl);
        var stage = new BlingoStage(impl);
        impl.Init(stage);
        _disposables.Add(impl);
        return stage;
    }

    public BlingoMovie AddMovie(BlingoStage stage, BlingoMovie blingoMovie)
    {
        var blazorStage = stage.Framework<BlingoBlazorStage>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var root = _services.GetRequiredService<BlingoBlazorRootPanel>();
        var impl = new BlingoBlazorMovie(blazorStage, blingoMovie, m => _disposables.Remove(m), scripts, root, _gfxFactory);
        blingoMovie.Init(impl);
        _disposables.Add(impl);
        return blingoMovie;
    }

    public BlingoSprite2D CreateSprite2D(IBlingoMovie movie, Action<BlingoSprite2D> onRemoveMe)
    {
        var blingoMovie = (BlingoMovie)movie;
        var sprite = new BlingoSprite2D(blingoMovie.GetEnvironment(), movie);
        sprite.SetOnRemoveMe(onRemoveMe);
        blingoMovie.Framework<BlingoBlazorMovie>().CreateSprite(sprite);
        return sprite;
    }

    public BlingoStageMouse CreateMouse(BlingoStage stage)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var mouseImpl = new BlingoBlazorMouse(new Lazy<AbstMouse<BlingoMouseEvent>>(() => null!), js, scripts);
        var mouse = new BlingoStageMouse(stage, mouseImpl);
        mouseImpl.SetMouse(mouse);
        return mouse;
    }

    public BlingoKey CreateKey()
    {
        var impl = new BlingoBlazorKey();
        var key = new BlingoKey(impl);
        impl.SetKeyObj(key);
        return key;
    }

    // The remaining factory methods are not yet required for the Blazor
    // backend. They will be implemented as the Blazor integration evolves.
    public T CreateMember<T>(IBlingoCast cast, int numberInCast, string name = "") where T : BlingoMember => typeof(T) switch
    {
        Type t when t == typeof(BlingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
        Type t when t == typeof(BlingoMemberQuickTimeMedia) => (CreateMemberQuickTimeMedia(cast, numberInCast, name) as T)!,
        Type t when t == typeof(BlingoMemberRealMedia) => (CreateMemberRealMedia(cast, numberInCast, name) as T)!,
        _ => throw new NotImplementedException()
    };
    public BlingoMemberBitmap CreateMemberBitmap(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var http = _services.GetRequiredService<HttpClient>();
        var impl = new BlingoBlazorMemberBitmap(js, scripts, http);
        var member = new BlingoMemberBitmap((BlingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMemberSound CreateMemberSound(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var http = _services.GetRequiredService<HttpClient>();
        var impl = new BlingoBlazorMemberSound(http);
        var member = new BlingoMemberSound(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoFilmLoopMember CreateMemberFilmLoop(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new BlingoBlazorMemberFilmLoop(js, scripts);
        var member = new BlingoFilmLoopMember(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMemberShape CreateMemberShape(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new BlingoBlazorMemberShape(js, scripts);
        var member = new BlingoMemberShape((BlingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMemberField CreateMemberField(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var fonts = _services.GetRequiredService<IAbstFontManager>();
        var impl = new BlingoBlazorMemberField(js, scripts, fonts);
        var member = new BlingoMemberField((BlingoCast)cast, impl, numberInCast, ComponentFactory, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMemberText CreateMemberText(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var fonts = _services.GetRequiredService<IAbstFontManager>();
        var impl = new BlingoBlazorMemberText(js, scripts, fonts);
        var member = new BlingoMemberText((BlingoCast)cast, impl, numberInCast, ComponentFactory, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public BlingoMemberQuickTimeMedia CreateMemberQuickTimeMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new BlingoBlazorMemberMedia();
        var member = new BlingoMemberQuickTimeMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMemberRealMedia CreateMemberRealMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new BlingoBlazorMemberMedia();
        var member = new BlingoMemberRealMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public BlingoMember CreateScript(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new BlingoBlazorMemberScript();
        var member = new BlingoMemberScript(impl, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        return member;
    }
    public BlingoMember CreateEmpty(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new BlazorFrameworkMemberEmpty();
        var member = new BlingoMember(impl, BlingoMemberType.Empty, (BlingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        return member;
    }
    public BlingoSound CreateSound(IBlingoCastLibsContainer castLibsContainer)
    {
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new BlingoBlazorSound(scripts);
        var sound = new BlingoSound(impl, castLibsContainer, this);
        impl.Init(sound);
        return sound;
    }
    public BlingoSoundChannel CreateSoundChannel(int number)
    {
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new BlingoBlazorSoundChannel(number, scripts);
        var channel = new BlingoSoundChannel(impl, number);
        impl.Init(channel);
        return channel;
    }
    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height) => _gfxFactory.CreateGfxCanvas(name, width, height);
    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name) => _gfxFactory.CreateWrapPanel(orientation, name);
    public AbstPanel CreatePanel(string name) => _gfxFactory.CreatePanel(name);
    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y) => _gfxFactory.CreateLayoutWrapper(content, x, y);
    public AbstTabContainer CreateTabContainer(string name) => _gfxFactory.CreateTabContainer(name);
    public AbstTabItem CreateTabItem(string name, string title) => _gfxFactory.CreateTabItem(name, title);
    public AbstScrollContainer CreateScrollContainer(string name) => _gfxFactory.CreateScrollContainer(name);
    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null) => _gfxFactory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);
    public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null) => _gfxFactory.CreateInputSliderInt(orientation, name, min, max, step, onChange);
    public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false) => _gfxFactory.CreateInputText(name, maxLength, onChange, multiLine);
    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null) => _gfxFactory.CreateInputNumberFloat(name, min, max, onChange);
    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null) => _gfxFactory.CreateInputNumberInt(name, min, max, onChange);
    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null) => _gfxFactory.CreateSpinBox(name, min, max, onChange);
    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null) => _gfxFactory.CreateInputCheckbox(name, onChange);
    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null) => _gfxFactory.CreateInputCombobox(name, onChange);
    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null) => _gfxFactory.CreateItemList(name, onChange);
    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null) => _gfxFactory.CreateColorPicker(name, onChange);
    public AbstLabel CreateLabel(string name, string text = "") => _gfxFactory.CreateLabel(name, text);
    public AbstButton CreateButton(string name, string text = "") => _gfxFactory.CreateButton(name, text);
    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null) => _gfxFactory.CreateStateButton(name, texture, text, onChange);
    public AbstMenu CreateMenu(string name) => _gfxFactory.CreateMenu(name);
    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null) => _gfxFactory.CreateMenuItem(name, shortcut);
    public AbstMenu CreateContextMenu(object window) => _gfxFactory.CreateContextMenu(window);
    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name) => _gfxFactory.CreateHorizontalLineSeparator(name);
    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name) => _gfxFactory.CreateVerticalLineSeparator(name);
    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }
}

