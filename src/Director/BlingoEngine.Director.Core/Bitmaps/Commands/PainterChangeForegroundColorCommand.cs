using AbstUI.Primitives;
using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Bitmaps.Commands
{
    public sealed record PainterChangeForegroundColorCommand(AColor color) : IAbstCommand;
}

