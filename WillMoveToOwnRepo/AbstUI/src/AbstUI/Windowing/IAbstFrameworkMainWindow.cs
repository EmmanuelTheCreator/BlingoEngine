using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;

namespace AbstUI.Windowing;

public interface IAbstFrameworkMainWindow : IFrameworkForInitializable<AbstMainWindow>
{
    string Title { get; set; }
    APoint GetTheSize();
    void SetTheSize(APoint size);
}

