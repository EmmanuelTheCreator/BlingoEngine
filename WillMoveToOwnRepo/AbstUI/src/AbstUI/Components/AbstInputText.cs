namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a single line text input.
    /// </summary>
    public class AbstInputText : AbstInputBase<IAbstFrameworkInputText>
    {

        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int MaxLength { get => _framework.MaxLength; set => _framework.MaxLength = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
        public bool IsMultiLine { get => _framework.IsMultiLine; set => _framework.IsMultiLine = value; }
    }
}
