using AbstUI.Windowing;
using BlingoEngine.Director.Core.Windowing;

namespace BlingoEngine.Director.Core.Stages
{
    public interface IDirFrameworkStageWindow : IAbstFrameworkWindow
    {
        void UpdateBoundingBoxes();
        void UpdateSelectionBox();
    }
}

