using AbstUI.Primitives;

namespace AbstUI.Inputs
{
    /// <summary>
    /// Interface for mouse operations provided by the framework.
    /// </summary>
    public interface IAbstFrameworkMouse
    {
        

        /// <summary>Shows or hides the mouse cursor.</summary>
        void HideMouse(bool state);

        /// <summary>
        /// The framework object is beeing replaced , so release the old one.
        /// </summary>
        void Release();
        void ReplaceMouseObj(IAbstMouse lingoMouse);
        void SetCursor(AMouseCursor cursor);
        AMouseCursor GetCursor();
    }
}
