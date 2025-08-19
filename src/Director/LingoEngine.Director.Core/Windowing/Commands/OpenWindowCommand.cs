using AbstUI.Commands;

namespace LingoEngine.Director.Core.Windowing.Commands
{
    public sealed record OpenWindowCommand(string WindowCode) : IAbstCommand;
}
