using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Transitions;
using LingoEngine.Blazor.Core;

namespace LingoEngine.Blazor.Transitions;

/// <summary>
/// Blazor placeholder implementation for transition members.
/// </summary>
public class LingoBlazorMemberTransition : BlazorFrameworkMemberEmpty
{
    private LingoTransitionMember _member = null!;

    internal void Init(LingoTransitionMember member)
    {
        _member = member;
    }
}
