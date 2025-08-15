using System;
using System.Collections.Generic;
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
using LingoEngine.Unity.Sprites;
using LingoEngine.Unity.Texts;
using LingoEngine.Unity.Sounds;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.Unity.Core;

public class UnityFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly ILingoServiceProvider _serviceProvider;

    public UnityFactory(ILingoServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
        var impl = new UnityMemberBitmap();
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
    public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
        var impl = new UnityMemberField(fontManager);
        var member = new LingoMemberField((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
        var impl = new UnityMemberText(fontManager);
        var member = new LingoMemberText((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
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
    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height) => throw new NotImplementedException();
    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name) => throw new NotImplementedException();
    public AbstPanel CreatePanel(string name) => throw new NotImplementedException();
    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y) => throw new NotImplementedException();
    public AbstTabContainer CreateTabContainer(string name) => throw new NotImplementedException();
    public AbstTabItem CreateTabItem(string name, string title) => throw new NotImplementedException();
    public AbstScrollContainer CreateScrollContainer(string name) => throw new NotImplementedException();
    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null) => throw new NotImplementedException();
    public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null) => throw new NotImplementedException();
    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null) => throw new NotImplementedException();
    public AbstInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null) where TValue : System.Numerics.INumber<TValue> => throw new NotImplementedException();
    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null) => throw new NotImplementedException();
    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null) => throw new NotImplementedException();
    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null) => throw new NotImplementedException();
    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null) => throw new NotImplementedException();
    public AbstLabel CreateLabel(string name, string text = "") => throw new NotImplementedException();
    public AbstButton CreateButton(string name, string text = "") => throw new NotImplementedException();
    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null) => throw new NotImplementedException();
    public AbstMenu CreateMenu(string name) => throw new NotImplementedException();
    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null) => throw new NotImplementedException();
    public AbstMenu CreateContextMenu(object window) => throw new NotImplementedException();
    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name) => throw new NotImplementedException();
    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name) => throw new NotImplementedException();
    public AbstWindow CreateWindow(string name, string title = "") => throw new NotImplementedException();
    #endregion

    public LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe)
    {
        var lingoMovie = (LingoMovie)movie;
        var sprite = new LingoSprite2D(lingoMovie.GetEnvironment(), movie);
        sprite.SetOnRemoveMe(onRemoveMe);
        lingoMovie.Framework<UnityMovie>().CreateSprite(sprite);
        return sprite;
    }

    public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => throw new NotImplementedException();
    public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => throw new NotImplementedException();

    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }
}
