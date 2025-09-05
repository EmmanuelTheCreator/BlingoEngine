using System;
using AbstUI.Primitives;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Framework specific single line text input.
    /// </summary>
    public interface IAbstFrameworkInputText : IAbstFrameworkNodeInput
    {
        string Text { get; set; }
        int MaxLength { get; set; }
        string? Font { get; set; }
        int FontSize { get; set; }
        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        AColor TextColor { get; set; }
        bool IsMultiLine { get; set; }

        bool HasSelection { get; }
        void DeleteSelection();
        void SetCaretPosition(int position);
        int GetCaretPosition();
        void SetSelection(int start, int end);
        void SetSelection(Range range);
        void InsertText(string text);
    }
}
