using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Tools;

public interface IDirFrameworkCodeHighlighter : IFrameworkFor<DirCodeHighlichter>
{
    void Init(DirCodeHighlichter highlighter);
}
