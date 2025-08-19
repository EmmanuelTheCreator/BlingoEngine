using AbstUI.Commands;

namespace LingoEngine.Director.Core.Windowing.Commands
{
    public sealed record CloseWindowCommand(string WindowCode) : IAbstCommand;
}
