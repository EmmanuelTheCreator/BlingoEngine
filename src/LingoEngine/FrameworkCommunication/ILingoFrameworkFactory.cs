using System;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Windowing;
using AbstUI;

namespace LingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Factory used by the core engine to create framework-specific
    /// implementations of stages, sprites, members and input handlers.
    /// Each rendering adapter provides its own implementation.
    /// </summary>
    public interface ILingoFrameworkFactory
    {
        IAbstComponentFactory ComponentFactory { get; }

        /// <summary>
        /// Creates the main <see cref="LingoStage"/> for a <see cref="LingoPlayer"/>.
        /// </summary>
        /// <param name="lingoPlayer">Current player instance.</param>
        LingoStage CreateStage(LingoPlayer lingoPlayer);
        /// <summary>
        /// Adds a movie to the specified stage.
        /// </summary>
        LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie);


        #region Members
        /// <summary>Creates a new cast member instance.</summary>
        T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember;
        /// <summary>Creates a picture member.</summary>
        LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            APoint regPoint = default);
        /// <summary>Creates a sound member.</summary>
        LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a film loop member.</summary>
        LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a vector shape member.</summary>
        LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a field member.</summary>
        LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a text member.</summary>
        LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a placeholder member.</summary>
        LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        #endregion

        /// <summary>Creates a sound instance.</summary>
        LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer);
        /// <summary>Creates a sound channel.</summary>
        LingoSoundChannel CreateSoundChannel(int number);
        /// <summary>Creates a mouse handler bound to the stage.</summary>
        LingoStageMouse CreateMouse(LingoStage stage);
        /// <summary>Creates a keyboard handler.</summary>
        LingoKey CreateKey();


        #region Gfx Elements
        /// <summary>
        /// Creates a generic drawing canvas instance.
        /// </summary>
        AbstGfxCanvas CreateGfxCanvas(string name, int width, int height);

        /// <summary>
        /// Creates a wrapping panel container.
        /// </summary>
        AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name);

        /// <summary>
        /// Creates a simple panel container for absolute positioning.
        /// </summary>
        AbstPanel CreatePanel(string name);
        AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y);

        /// <summary>
        /// Creates a tab container for organizing child panels.
        /// </summary>
        AbstTabContainer CreateTabContainer(string name);
        AbstTabItem CreateTabItem(string name, string title);

        /// <summary>Creates a scroll container.</summary>
        AbstScrollContainer CreateScrollContainer(string name);

        /// <summary>Creates a slider input control for floating point values.</summary>
        AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null);
        /// <summary>Creates a slider input control for integer values.</summary>
        AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null);

        /// <summary>Creates a single line text input.</summary>
        AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false);

        /// <summary>Creates a numeric input field.</summary>
        AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        /// <summary>Creates a numeric input field.</summary>
        AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null);

        /// <summary>Creates a spin box input.</summary>
        AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null);

        /// <summary>Creates a checkbox input.</summary>
        AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null);

        /// <summary>Creates a combo box input.</summary>
        AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null);

        /// <summary>Creates a list widget.</summary>
        AbstItemList CreateItemList(string name, Action<string?>? onChange = null);

        /// <summary>Creates a color picker input.</summary>

/* Unmerged change from project 'LingoEngine (net8.0)'
Before:
        AbstGfxColorPicker CreateColorPicker(string name, Action<Primitives.LingoColor>? onChange = null);
After:
        AbstGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null);
*/
        AbstColorPicker CreateColorPicker(string name, Action<AbstUI.Primitives.AColor>? onChange = null);

        /// <summary>Creates a simple text label.</summary>
        AbstLabel CreateLabel(string name, string text = "");

        /// <summary>Creates a clickable button.</summary>
        AbstButton CreateButton(string name, string text = "");

        /// <summary>Creates a toggle state button.</summary>
        AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null);

        /// <summary>Creates a menu container.</summary>
        AbstMenu CreateMenu(string name);

        /// <summary>Creates a menu item.</summary>
        AbstMenuItem CreateMenuItem(string name, string? shortcut = null);

        /// <summary>Creates a context menu bound to the given window.</summary>
        AbstMenu CreateContextMenu(object window);

        /// <summary>Creates a horizontal line separator.</summary>
        AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name);

        /// <summary>Creates a vertical line separator.</summary>
        AbstVerticalLineSeparator CreateVerticalLineSeparator(string name);

        ///// <summary>Creates a window container.</summary>
        //AbstWindow CreateWindow(string name, string title = "");

        #endregion

        /// <summary>Creates a sprite instance.</summary>
        LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe);
        /// <summary>Creates a sprite behaviour.</summary>
        T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior;
        /// <summary>Creates a movie script.</summary>
        T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript;

  
        
    }
}
