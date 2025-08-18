using LingoEngine.ColorPalettes;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Blazor.Core;

namespace LingoEngine.Blazor.ColorPalettes;

/// <summary>
/// Blazor placeholder implementation for color palette members.
/// </summary>
public class LingoBlazorMemberColorPalette : BlazorFrameworkMemberEmpty
{
    private LingoColorPaletteMember _member = null!;

    internal void Init(LingoColorPaletteMember member)
    {
        _member = member;
    }
}
