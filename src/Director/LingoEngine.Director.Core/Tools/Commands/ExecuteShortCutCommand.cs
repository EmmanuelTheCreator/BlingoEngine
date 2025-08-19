using AbstUI.Commands;
using AbstUI.Tools;

namespace LingoEngine.Director.Core.Tools.Commands
{
    public sealed record ExecuteShortCutCommand(AbstShortCutMap ShortCut) : IAbstCommand;
}
