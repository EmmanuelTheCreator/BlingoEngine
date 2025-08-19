using AbstUI.Windowing;
using LingoEngine.Director.Core.Windowing;

namespace LingoEngine.Director.Core.Bitmaps
{
    public interface IDirFrameworkBitmapEditWindow : IAbstFrameworkWindow 
    {
        bool SelectTheTool(PainterToolType tool);
        bool DrawThePixel(int x, int y);
    }
}
