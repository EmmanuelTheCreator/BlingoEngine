using System;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Shapes;
using BlingoEngine.Texts;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using BlingoEngine.Bitmaps;
using BlingoEngine.FilmLoops;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;

namespace BlingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Factory used by the core engine to create framework-specific
    /// implementations of stages, sprites, members and input handlers.
    /// Each rendering adapter provides its own implementation.
    /// </summary>
    public interface IBlingoFrameworkFactory
    {
        IAbstComponentFactory ComponentFactory { get; }

        /// <summary>
        /// Creates the main <see cref="BlingoStage"/> for a <see cref="BlingoPlayer"/>.
        /// </summary>
        /// <param name="blingoPlayer">Current player instance.</param>
        BlingoStage CreateStage(BlingoPlayer blingoPlayer);
        /// <summary>
        /// Adds a movie to the specified stage.
        /// </summary>
        BlingoMovie AddMovie(BlingoStage stage, BlingoMovie blingoMovie);


        #region Members
        /// <summary>Creates a new cast member instance.</summary>
        T CreateMember<T>(IBlingoCast cast, int numberInCast, string name = "") where T : BlingoMember;
        /// <summary>Creates a picture member.</summary>
        BlingoMemberBitmap CreateMemberBitmap(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null,
            APoint regPoint = default);
        /// <summary>Creates a sound member.</summary>
        BlingoMemberSound CreateMemberSound(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a film loop member.</summary>
        BlingoFilmLoopMember CreateMemberFilmLoop(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a vector shape member.</summary>
        BlingoMemberShape CreateMemberShape(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a field member.</summary>
        BlingoMemberField CreateMemberField(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a text member.</summary>
        BlingoMemberText CreateMemberText(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        BlingoMember CreateScript(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        /// <summary>Creates a placeholder member.</summary>
        BlingoMember CreateEmpty(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default);
        #endregion

        /// <summary>Creates a sound instance.</summary>
        BlingoSound CreateSound(IBlingoCastLibsContainer castLibsContainer);
        /// <summary>Creates a sound channel.</summary>
        BlingoSoundChannel CreateSoundChannel(int number);
        /// <summary>Creates a mouse handler bound to the stage.</summary>
        BlingoStageMouse CreateMouse(BlingoStage stage);
        /// <summary>Creates a keyboard handler.</summary>
        BlingoKey CreateKey();


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

/* Unmerged change from project 'BlingoEngine (net8.0)'
Before:
        AbstGfxColorPicker CreateColorPicker(string name, Action<Primitives.BlingoColor>? onChange = null);
After:
        AbstGfxColorPicker CreateColorPicker(string name, Action<BlingoColor>? onChange = null);
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
        BlingoSprite2D CreateSprite2D(IBlingoMovie movie, Action<BlingoSprite2D> onRemoveMe);
        
    }
}

