using AbstUI.Commands;

namespace AbstUI.Windowing.Commands;

public sealed record OpenWindowCommand(string WindowCode) : IAbstCommand;
