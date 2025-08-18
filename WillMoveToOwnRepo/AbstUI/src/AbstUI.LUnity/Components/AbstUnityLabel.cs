using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkLabel"/>.
/// </summary>
internal class AbstUnityLabel : AbstUnityComponent, IAbstFrameworkLabel
{
    public string Text { get; set; } = string.Empty;
    public int FontSize { get; set; } = 12;
    public string? Font { get; set; }
    public AColor FontColor { get; set; } = new AColor(255, 255, 255);
    public int LineHeight { get; set; } = 1;
    public ATextWrapMode WrapMode { get; set; } = ATextWrapMode.Off;
    public AbstTextAlignment TextAlignment { get; set; } = AbstTextAlignment.Left;
}
