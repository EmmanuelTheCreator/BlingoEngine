using AbstUI.Commands;
using BlingoEngine.Director.Core.Bitmaps;

namespace BlingoEngine.Director.Core.Bitmaps.Commands
{
    /// <summary>
    /// Command to activate a specific painter tool by enum.
    /// </summary>
    public sealed record PainterToolSelectCommand(PainterToolType Tool) : IAbstCommand;
    
}

