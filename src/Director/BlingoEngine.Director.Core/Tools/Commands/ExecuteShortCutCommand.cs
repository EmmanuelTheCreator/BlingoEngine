using AbstUI.Commands;
using AbstUI.Tools;

namespace BlingoEngine.Director.Core.Tools.Commands
{
    public sealed record ExecuteShortCutCommand(AbstShortCutMap ShortCut) : IAbstCommand;
}

