using AbstUI.Windowing;
using BlingoEngine.Director.Core.Windowing;

namespace BlingoEngine.Director.Core.Bitmaps
{
    public interface IDirFrameworkBitmapEditWindow : IAbstFrameworkWindow 
    {
        bool SelectTheTool(PainterToolType tool);
        bool DrawThePixel(int x, int y);
    }
}

