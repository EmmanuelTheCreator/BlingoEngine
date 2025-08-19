using Godot;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Tools;

public partial class GodotLingoCSharpConverterPopup : Window, IDirFrameworkDialog, IFrameworkFor<LingoCSharpConverterPopup>
{
    public GodotLingoCSharpConverterPopup() { }

    public void Init(ILingoDialog lingoDialog) { }
}
