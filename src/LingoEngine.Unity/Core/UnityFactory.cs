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
using LingoEngine.Gfx;
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
using LingoEngine.Styles;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.AbstUI.Primitives;

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
        var fontManager = _serviceProvider.GetRequiredService<ILingoFontManager>();
        var impl = new UnityMemberField(fontManager);
        var member = new LingoMemberField((LingoCast)cast, impl, numberInCast, name, fileName ?? string.Empty, regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
    {
        var fontManager = _serviceProvider.GetRequiredService<ILingoFontManager>();
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
        var mouseImpl = new UnityMouse(new Lazy<LingoMouse>(() => null!));
        var mouse = new LingoStageMouse(stage, mouseImpl);
        mouseImpl.SetLingoMouse(mouse);
        return mouse;
    }

    public LingoKey CreateKey()
    {
        LingoKey? key = null;
        var impl = new UnityKey();
        key = new LingoKey(impl);
        impl.SetLingoKey(key);
        return key;
    }

    #region Gfx
    public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height) => throw new NotImplementedException();
    public LingoGfxWrapPanel CreateWrapPanel(AOrientation orientation, string name) => throw new NotImplementedException();
    public LingoGfxPanel CreatePanel(string name) => throw new NotImplementedException();
    public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y) => throw new NotImplementedException();
    public LingoGfxTabContainer CreateTabContainer(string name) => throw new NotImplementedException();
    public LingoGfxTabItem CreateTabItem(string name, string title) => throw new NotImplementedException();
    public LingoGfxScrollContainer CreateScrollContainer(string name) => throw new NotImplementedException();
    public LingoGfxInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null) where TValue : System.Numerics.INumber<TValue> => throw new NotImplementedException();
    public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null) => throw new NotImplementedException();
    public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null) => throw new NotImplementedException();
    public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null) => throw new NotImplementedException();
    public LingoGfxColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null) => throw new NotImplementedException();
    public LingoGfxLabel CreateLabel(string name, string text = "") => throw new NotImplementedException();
    public LingoGfxButton CreateButton(string name, string text = "") => throw new NotImplementedException();
    public LingoGfxStateButton CreateStateButton(string name, ILingoTexture2D? texture = null, string text = "", Action<bool>? onChange = null) => throw new NotImplementedException();
    public LingoGfxMenu CreateMenu(string name) => throw new NotImplementedException();
    public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null) => throw new NotImplementedException();
    public LingoGfxMenu CreateContextMenu(object window) => throw new NotImplementedException();
    public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name) => throw new NotImplementedException();
    public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name) => throw new NotImplementedException();
    public LingoGfxWindow CreateWindow(string name, string title = "") => throw new NotImplementedException();
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
