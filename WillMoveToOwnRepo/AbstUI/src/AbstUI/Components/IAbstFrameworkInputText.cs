namespace AbstUI.Components
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
    }
}
