using AbstUI.Windowing;
using LingoEngine.Director.Core.Windowing;

namespace LingoEngine.Director.Core.Stages
{
    public interface IDirFrameworkStageWindow : IAbstFrameworkWindow
    {
        void UpdateBoundingBoxes();
        void UpdateSelectionBox();
    }
}
