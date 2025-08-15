using LingoEngine.AbstUI.Primitives;
using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    public sealed record PainterChangeBackgroundColorCommand(AColor color) : ILingoCommand;
}
