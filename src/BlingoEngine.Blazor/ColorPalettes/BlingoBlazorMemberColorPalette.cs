using BlingoEngine.ColorPalettes;
using BlingoEngine.Members;
using BlingoEngine.Sprites;
using BlingoEngine.Blazor.Core;

namespace BlingoEngine.Blazor.ColorPalettes;

/// <summary>
/// Blazor placeholder implementation for color palette members.
/// </summary>
public class BlingoBlazorMemberColorPalette : BlazorFrameworkMemberEmpty
{
    private BlingoColorPaletteMember _member = null!;

    internal void Init(BlingoColorPaletteMember member)
    {
        _member = member;
    }
}

