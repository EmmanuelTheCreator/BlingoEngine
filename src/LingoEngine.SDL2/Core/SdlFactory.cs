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

namespace LingoEngine.SDL2.Core;
/// <inheritdoc/>
public class SdlFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly SdlRootContext _rootContext;
    /// <inheritdoc/>
    public SdlFactory(IServiceProvider serviceProvider, SdlRootContext rootContext)
    {
        _serviceProvider = serviceProvider;
        _rootContext = rootContext;
    }
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
        var impl = new SdlMemberFilmLoop();
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
        var impl = new SdlMovie(sdlStage, movie, m => _disposables.Remove(m));
        movie.Init(impl);
        _disposables.Add(impl);
        return movie;
    }
    /// <inheritdoc/>
    public T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe) where T : LingoSprite2D
    {
        var movieTyped = (LingoMovie)movie;
        var sprite = movieTyped.GetServiceProvider().GetRequiredService<T>();
        sprite.SetOnRemoveMe(onRemoveMe);
        movieTyped.Framework<SdlMovie>().CreateSprite(sprite);
        return sprite;
    }
    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }
    /// <inheritdoc/>
    public LingoStageMouse CreateMouse(LingoStage stage)
    {
        var mouseImpl = new SdlMouse(new Lazy<LingoMouse>(() => null!));
        var mouse = new LingoStageMouse(stage, mouseImpl);
        mouseImpl.SetLingoMouse(mouse);
        return mouse;
    }
    /// <inheritdoc/>
    public LingoKey CreateKey()
    {
        var impl = _rootContext.Key;
        var key = new LingoKey(impl);
        impl.SetLingoKey(key);
        return key;
    }


    #region Gfx elements
    /// <inheritdoc/>
    public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
    {
        var canvas = new LingoGfxCanvas();
        var impl = new SdlGfxCanvas(_rootContext.Renderer, _serviceProvider.GetRequiredService<ILingoFontManager>(), width, height);
        canvas.Init(impl);
        canvas.Width = width;
        canvas.Height = height;
        canvas.Name = name;
        return canvas;
    }
    /// <inheritdoc/>
    public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
    {

        var panel = new LingoGfxWrapPanel(this);
        var impl = new SdlGfxWrapPanel(_rootContext.Renderer, orientation);

        panel.Init(impl);
        panel.Name = name;
        // Keep orientation in sync on creation
        panel.Orientation = orientation;
        return panel;
    }
    /// <inheritdoc/>
    public LingoGfxPanel CreatePanel(string name)
    {
        var panel = new LingoGfxPanel(this);
        var impl = new SdlGfxPanel(_rootContext.Renderer);
        panel.Init(impl);
        panel.Name = name;
        return panel;
    }
    /// <inheritdoc/>
    public LingoGfxTabContainer CreateTabContainer(string name)
    {
        var tab = new LingoGfxTabContainer();
        var impl = new SdlGfxTabContainer(_rootContext.Renderer);
        tab.Init(impl);
        tab.Name = name;
        return tab;
    }
    /// <inheritdoc/>
    public LingoGfxTabItem CreateTabItem(string name, string title)
    {
        var tab = new LingoGfxTabItem();
        var impl = new SdlGfxTabItem(_rootContext.Renderer, tab);
        tab.Title = title;
        tab.Name = name;
        return tab;
    }
    /// <inheritdoc/>
    public LingoGfxScrollContainer CreateScrollContainer(string name)
    {
        var scroll = new LingoGfxScrollContainer();
        var impl = new SdlGfxScrollContainer(_rootContext.Renderer);
        scroll.Init(impl);
        scroll.Name = name;
        return scroll;
    }

    /// <inheritdoc/>
    public LingoGfxInputSlider<float> CreateInputSliderFloat(LingoOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
    {
        var slider = new LingoGfxInputSlider<float>();
        var impl = new SdlGfxInputSlider<float>(_rootContext.Renderer);
        slider.Init(impl);
        slider.Name = name;
        if (min.HasValue) slider.MinValue = min.Value;
        if (max.HasValue) slider.MaxValue = max.Value;
        if (step.HasValue) slider.Step = step.Value;
        if (onChange != null)
            slider.ValueChanged += () => onChange(slider.Value);
        return slider;
    }

    public LingoGfxInputSlider<int> CreateInputSliderInt(LingoOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
    {
        var slider = new LingoGfxInputSlider<int>();
        var impl = new SdlGfxInputSlider<int>(_rootContext.Renderer);
        slider.Init(impl);
        slider.Name = name;
        if (min.HasValue) slider.MinValue = min.Value;
        if (max.HasValue) slider.MaxValue = max.Value;
        if (step.HasValue) slider.Step = step.Value;
        if (onChange != null)
            slider.ValueChanged += () => onChange(slider.Value);
        return slider;
    }
    /// <inheritdoc/>
    public LingoGfxInputText CreateInputText(string name, int maxLength = 0)
    {
        var input = new LingoGfxInputText { MaxLength = maxLength };
        var impl = new SdlGfxInputText(_rootContext.Renderer);
        input.Init(impl);
        input.Name = name;
        return input;
    }
    /// <inheritdoc/>
    public LingoGfxInputNumber<float> CreateInputNumber(string name, float? min = null, float? max = null)
    {
        var input = new LingoGfxInputNumber<float>();
        //var impl = new SdlGfxInputNumber<float>(_rootContext.Renderer);
        //input.Init(impl);
        //input.Name = name;
        //if (min.HasValue) input.Min = min.Value;
        //if (max.HasValue) input.Max = max.Value;
        return input;
    }
    /// <inheritdoc/>
    public LingoGfxInputNumber<int> CreateInputNumber(string name, int? min = null, int? max = null)
    {
        var input = new LingoGfxInputNumber<int>();
        //var impl = new SdlGfxInputNumber<int>(_rootContext.Renderer);
        //input.Init(impl);
        //input.Name = name;
        //if (min.HasValue) input.Min = min.Value;
        //if (max.HasValue) input.Max = max.Value;
        return input;
    }
    /// <inheritdoc/>
    public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null)
    {
        var spin = new LingoGfxSpinBox();
        var impl = new SdlGfxSpinBox(_rootContext.Renderer);
        spin.Init(impl);
        spin.Name = name;
        if (min.HasValue) spin.Min = min.Value;
        if (max.HasValue) spin.Max = max.Value;
        return spin;
    }
    /// <inheritdoc/>
    public LingoGfxInputCheckbox CreateInputCheckbox(string name)
    {
        var input = new LingoGfxInputCheckbox();
        var impl = new SdlGfxInputCheckbox(_rootContext.Renderer);
        input.Init(impl);
        input.Name = name;
        return input;
    }
    /// <inheritdoc/>
    public LingoGfxInputCombobox CreateInputCombobox(string name)
    {
        var input = new LingoGfxInputCombobox();
        var impl = new SdlGfxInputCombobox(_rootContext.Renderer);
        input.Init(impl);
        input.Name = name;
        return input;
    }
    /// <inheritdoc/>
    public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
    {
        var list = new LingoGfxItemList();
        var impl = new SdlGfxItemList(_rootContext.Renderer);
        list.Init(impl);
        list.Name = name;
        if (onChange != null)
            list.ValueChanged += () => onChange(list.SelectedKey);
        return list;
    }
    /// <inheritdoc/>
    public LingoGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null)
    {
        var picker = new LingoGfxColorPicker();
        var impl = new SdlGfxColorPicker(_rootContext.Renderer);
        picker.Init(impl);
        picker.Name = name;
        if (onChange != null)
            picker.ValueChanged += () => onChange(picker.Color);
        return picker;
    }
    /// <inheritdoc/>
    public LingoGfxLabel CreateLabel(string name, string text = "")
    {
        var label = new LingoGfxLabel { Text = text };
        var impl = new SdlGfxLabel(_rootContext.Renderer);
        label.Init(impl);
        label.Name = name;
        return label;
    }
    /// <inheritdoc/>
    public LingoGfxButton CreateButton(string name, string text = "")
    {
        var button = new LingoGfxButton { Text = text };
        var impl = new SdlGfxButton(_rootContext.Renderer);
        button.Init(impl);
        button.Name = name;
        return button;
    }
    /// <inheritdoc/>
    public LingoGfxStateButton CreateStateButton(string name, Bitmaps.ILingoImageTexture? texture = null, string text = "", Action<bool>? onChange = null)
    {
        var button = new LingoGfxStateButton { Text = text };
        var impl = new SdlGfxStateButton(_rootContext.Renderer);
        if (onChange != null)
            button.ValueChanged += () => onChange(button.IsOn); // hooking in wrapper since SDL button is dummy
        button.Init(impl);
        button.Name = name;
        if (texture != null)
            button.Texture = texture;
        return button;
    }
    /// <inheritdoc/>
    public LingoGfxMenu CreateMenu(string name)
    {
        var menu = new LingoGfxMenu();
        var impl = new SdlGfxMenu(_rootContext.Renderer, name);
        menu.Init(impl);
        return menu;
    }
    /// <inheritdoc/>
    public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
    {
        var item = new LingoGfxMenuItem();
        var impl = new SdlGfxMenuItem(_rootContext.Renderer, name, shortcut);
        item.Init(impl);
        return item;
    }

    /// <inheritdoc/>
    public LingoGfxMenu CreateContextMenu(object window)
    {
        // SDL UI is not implemented yet, return a basic menu instance
        var menu = CreateMenu("ContextMenu");
        return menu;
    }
    /// <inheritdoc/>
    public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxWindow CreateWindow(string name, string title = "")
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        // Convert nullable float to NullableNum<float> explicitly  
        var minNullableNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
        var maxNullableNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
        return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
    }
    /// <inheritdoc/>
    public LingoGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
    {
        // Convert nullable float to NullableNum<float> explicitly  
        var minNullableNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
        var maxNullableNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
        return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
    }
    /// <inheritdoc/>
    public LingoGfxInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
         where TValue : System.Numerics.INumber<TValue>
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
    {
        throw new NotImplementedException();
    }
    #endregion


}