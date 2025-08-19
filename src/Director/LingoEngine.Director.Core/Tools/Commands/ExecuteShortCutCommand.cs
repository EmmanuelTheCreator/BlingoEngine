using AbstUI.Commands;

namespace LingoEngine.Director.Core.Tools.Commands
{
    public sealed record ExecuteShortCutCommand(DirectorShortCutMap ShortCut) : IAbstCommand;
}
