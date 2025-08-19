using LingoEngine.Casts;
using LingoEngine.Members;
using AbstUI.Commands;

namespace LingoEngine.Director.Core.Casts.Commands;

/// <summary>
/// Command for deleting a cast member from a cast library.
/// </summary>
public sealed record RemoveMemberCommand(LingoCast Cast, LingoMember Member) : IAbstCommand;
