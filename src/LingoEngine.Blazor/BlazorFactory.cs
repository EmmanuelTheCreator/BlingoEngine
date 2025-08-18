using System;
using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
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

namespace LingoEngine.Blazor;

public class BlazorFactory : ILingoFrameworkFactory
{
    public BlazorFactory(ILingoServiceProvider services) { }
    public LingoStage CreateStage(LingoPlayer lingoPlayer) => throw new NotImplementedException();
    public LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie) => throw new NotImplementedException();
    public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember => throw new NotImplementedException();
    public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
    public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer) => throw new NotImplementedException();
    public LingoSoundChannel CreateSoundChannel(int number) => throw new NotImplementedException();
    public LingoStageMouse CreateMouse(LingoStage stage) => throw new NotImplementedException();
    public LingoKey CreateKey() => throw new NotImplementedException();
    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height) => throw new NotImplementedException();
    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name) => throw new NotImplementedException();
    public AbstPanel CreatePanel(string name) => throw new NotImplementedException();
    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y) => throw new NotImplementedException();
    public AbstTabContainer CreateTabContainer(string name) => throw new NotImplementedException();
    public AbstTabItem CreateTabItem(string name, string title) => throw new NotImplementedException();
    public AbstScrollContainer CreateScrollContainer(string name) => throw new NotImplementedException();
    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null) => throw new NotImplementedException();
    public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false) => throw new NotImplementedException();
    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null) => throw new NotImplementedException();
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
    public LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe) => throw new NotImplementedException();
    public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => throw new NotImplementedException();
    public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => throw new NotImplementedException();
}
