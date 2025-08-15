
using AbstUI.Primitives;

namespace LingoEngine.Director.Core.Icons;

public interface IDirectorIconManager
{

    IAbstUITexture2D Get(DirectorIcon icon);
}
