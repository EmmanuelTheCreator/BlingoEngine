using BlingoEngine.Members;
using BlingoEngine.Sprites;
using BlingoEngine.Transitions;
using BlingoEngine.Blazor.Core;

namespace BlingoEngine.Blazor.Transitions;

/// <summary>
/// Blazor placeholder implementation for transition members.
/// </summary>
public class BlingoBlazorMemberTransition : BlazorFrameworkMemberEmpty
{
    private BlingoTransitionMember _member = null!;

    internal void Init(BlingoTransitionMember member)
    {
        _member = member;
    }
}

