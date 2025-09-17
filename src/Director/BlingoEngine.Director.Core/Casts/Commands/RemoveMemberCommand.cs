using BlingoEngine.Casts;
using BlingoEngine.Members;
using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Casts.Commands;

/// <summary>
/// Command for deleting a cast member from a cast library.
/// </summary>
public sealed record RemoveMemberCommand(BlingoCast Cast, BlingoMember Member) : IAbstCommand;

