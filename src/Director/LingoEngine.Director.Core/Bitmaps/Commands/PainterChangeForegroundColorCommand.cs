using AbstUI.Primitives;
using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    public sealed record PainterChangeForegroundColorCommand(AColor color) : ILingoCommand;
}
