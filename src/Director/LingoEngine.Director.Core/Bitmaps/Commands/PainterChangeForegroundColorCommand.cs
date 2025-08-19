using AbstUI.Primitives;
using AbstUI.Commands;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    public sealed record PainterChangeForegroundColorCommand(AColor color) : IAbstCommand;
}
