using System;
using AbstUI.Components;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputText"/>.
/// </summary>
internal class AbstUnityInputText : AbstUnityComponent, IAbstFrameworkInputText
{
    public bool Enabled { get; set; } = true;

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            ValueChanged?.Invoke();
        }
    }

    public int MaxLength { get; set; }
    public string? Font { get; set; }
    public int FontSize { get; set; }
    public bool IsMultiLine { get; set; }

    public event Action? ValueChanged;
}
