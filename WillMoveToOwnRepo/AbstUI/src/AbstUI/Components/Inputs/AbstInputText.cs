using System;
using AbstUI.Primitives;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Engine level wrapper for a single line text input.
    /// </summary>
    public class AbstInputText : AbstInputBase<IAbstFrameworkInputText>, IHasTextBackgroundBorderColor
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int MaxLength { get => _framework.MaxLength; set => _framework.MaxLength = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
        public AColor TextColor { get => ((IHasTextBackgroundBorderColor)_framework).TextColor; set => ((IHasTextBackgroundBorderColor)_framework).TextColor = value; }
        public AColor BackgroundColor { get => ((IHasTextBackgroundBorderColor)_framework).BackgroundColor; set => ((IHasTextBackgroundBorderColor)_framework).BackgroundColor = value; }
        public AColor BorderColor { get => ((IHasTextBackgroundBorderColor)_framework).BorderColor; set => ((IHasTextBackgroundBorderColor)_framework).BorderColor = value; }
        public bool IsMultiLine { get => _framework.IsMultiLine; set => _framework.IsMultiLine = value; }

        public bool HasSelection => _framework.HasSelection;
        public void DeleteSelection() => _framework.DeleteSelection();
        public void SetCaretPosition(int line, int column) => _framework.SetCaretPosition(line, column);
        public (int line, int column) GetCaretPosition() => _framework.GetCaretPosition();
        public void SetSelection(int startLine, int startColumn, int endLine, int endColumn) => _framework.SetSelection(startLine, startColumn, endLine, endColumn);
        public void InsertText(string text) => _framework.InsertText(text);
        public event Action<int, int>? OnCaretChanged
        {
            add => _framework.OnCaretChanged += value;
            remove => _framework.OnCaretChanged -= value;
        }
    }
}
