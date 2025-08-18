using System;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkButton"/>.
/// </summary>
internal class AbstUnityButton : AbstUnityComponent, IAbstFrameworkButton
{
    public string Text { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public IAbstTexture2D? IconTexture { get; set; }

    public event Action? Pressed;

    public void Invoke() => Pressed?.Invoke();
}
