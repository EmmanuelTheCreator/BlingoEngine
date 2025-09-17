using AbstUI.Texts;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using AbstUI.Styles;

namespace BlingoEngine.Texts.FrameworkCommunication
{
    /// <summary>
    /// Lingo Framework Member Text Base interface.
    /// </summary>
    public interface IBlingoFrameworkMemberTextBase : IBlingoFrameworkMemberWithTexture
    {
        /// <summary>
        /// The raw text contents of the member. This property can be used to read or write the full contents.
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Enables or disables word wrapping in the field.
        /// </summary>
        bool WordWrap { get; set; }

        /// <summary>
        /// Gets or sets the scroll position of the field.
        /// </summary>
        int ScrollTop { get; set; }
        string FontName { get; set; }
        int FontSize { get; set; }
        BlingoTextStyle FontStyle { get; set; }
        AColor TextColor { get; set; }
        AbstTextAlignment Alignment { get; set; }
        int Margin { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        IAbstFontManager FontManager { get; }


        /// <summary>
        /// Copies the current selection to the clipboard.
        /// </summary>
        void Copy(string text);

        string PasteClipboard();


    }
}

