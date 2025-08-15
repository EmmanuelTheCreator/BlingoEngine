
using AbstUI.Primitives;

namespace LingoEngine.Director.Core.Icons;

public interface IDirectorIconManager
{

    IAbstTexture2D Get(DirectorIcon icon);
}
