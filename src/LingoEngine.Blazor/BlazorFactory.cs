using System;
using System.Collections.Generic;
using AbstUI.Blazor;
using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using LingoEngine.Bitmaps;
using LingoEngine.Blazor.Inputs;
using LingoEngine.Blazor.Movies;
using LingoEngine.Blazor.Sprites;
using LingoEngine.Blazor.Stages;
using LingoEngine.Blazor.FilmLoops;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.FilmLoops;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Shapes;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Texts;
using LingoEngine.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using LingoEngine.Medias;
using LingoEngine.Blazor.Medias;

namespace LingoEngine.Blazor;

/// <summary>
/// Factory responsible for creating Blazor backed framework objects.
/// Only a subset of the factory is implemented, enough for stage, movie and
/// sprite handling which is required by the current tests.
/// </summary>
public class BlazorFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly ILingoServiceProvider _services;
    private readonly List<IDisposable> _disposables = new();
    private readonly IAbstComponentFactory _gfxFactory;

    public IAbstComponentFactory ComponentFactory => _gfxFactory;

    public BlazorFactory(ILingoServiceProvider services)
    {
        _services = services;

        var styleManager = _services.GetRequiredService<IAbstStyleManager>();
        var fontManager = _services.GetRequiredService<IAbstFontManager>();
        _gfxFactory = services.GetRequiredService<IAbstComponentFactory>();
    }

    public LingoStage CreateStage(LingoPlayer lingoPlayer)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new LingoBlazorStage((LingoClock)lingoPlayer.Clock, js, scripts);
        var stage = new LingoStage(impl);
        impl.Init(stage);
        _disposables.Add(impl);
        return stage;
    }

    public LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie)
    {
        var blazorStage = stage.Framework<LingoBlazorStage>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new LingoBlazorMovie(blazorStage, lingoMovie, m => _disposables.Remove(m), scripts);
        lingoMovie.Init(impl);
        _disposables.Add(impl);
        return lingoMovie;
    }

    public LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe)
    {
        var lingoMovie = (LingoMovie)movie;
        var sprite = new LingoSprite2D(lingoMovie.GetEnvironment(), movie);
        sprite.SetOnRemoveMe(onRemoveMe);
        lingoMovie.Framework<LingoBlazorMovie>().CreateSprite(sprite);
        return sprite;
    }

    public LingoStageMouse CreateMouse(LingoStage stage)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var mouseImpl = new LingoBlazorMouse(new Lazy<AbstMouse<LingoMouseEvent>>(() => null!), js, scripts);
        var mouse = new LingoStageMouse(stage, mouseImpl);
        mouseImpl.SetMouse(mouse);
        return mouse;
    }

    public LingoKey CreateKey()
    {
        var impl = new LingoBlazorKey();
        var key = new LingoKey(impl);
        impl.SetKeyObj(key);
        return key;
    }

    // The remaining factory methods are not yet required for the Blazor
    // backend. They will be implemented as the Blazor integration evolves.
    public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember => typeof(T) switch
    {
        Type t when t == typeof(LingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
        Type t when t == typeof(LingoMemberQuickTimeMedia) => (CreateMemberQuickTimeMedia(cast, numberInCast, name) as T)!,
        Type t when t == typeof(LingoMemberRealMedia) => (CreateMemberRealMedia(cast, numberInCast, name) as T)!,
        _ => throw new NotImplementedException()
    };
    public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var js = _services.GetRequiredService<IJSRuntime>();
        var scripts = _services.GetRequiredService<AbstUIScriptResolver>();
        var impl = new LingoBlazorMemberFilmLoop(js, scripts);
        var member = new LingoFilmLoopMember(impl, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberQuickTimeMedia CreateMemberQuickTimeMedia(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new LingoBlazorMemberMedia();
        var member = new LingoMemberQuickTimeMedia(impl, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public LingoMemberRealMedia CreateMemberRealMedia(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var impl = new LingoBlazorMemberMedia();
        var member = new LingoMemberRealMedia(impl, (LingoCast)cast, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        return member;
    }
    public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer) => throw new NotImplementedException();
    public LingoSoundChannel CreateSoundChannel(int number) => throw new NotImplementedException();
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
    public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => throw new NotImplementedException();
    public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => throw new NotImplementedException();

    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }
}
