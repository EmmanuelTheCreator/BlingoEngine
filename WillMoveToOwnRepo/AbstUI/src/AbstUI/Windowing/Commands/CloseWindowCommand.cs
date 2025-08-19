using AbstUI.Commands;

namespace AbstUI.Windowing.Commands;

public sealed record CloseWindowCommand(string WindowCode) : IAbstCommand;
