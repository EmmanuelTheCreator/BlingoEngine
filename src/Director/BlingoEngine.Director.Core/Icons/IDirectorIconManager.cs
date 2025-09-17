
using AbstUI.Primitives;

namespace BlingoEngine.Director.Core.Icons;

public interface IDirectorIconManager
{

    IAbstTexture2D Get(DirectorIcon icon);
}

