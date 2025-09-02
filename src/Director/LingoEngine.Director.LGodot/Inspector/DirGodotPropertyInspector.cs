using System;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using LingoEngine.Director.Core.Inspector;

namespace LingoEngine.Director.LGodot.Inspector;

public partial class DirGodotPropertyInspector : BaseGodotWindow, IDirFrameworkPropertyInspectorWindow, IFrameworkFor<DirectorPropertyInspectorWindow>
{

    private readonly DirectorPropertyInspectorWindow _inspectorWindow;

    public DirGodotPropertyInspector(DirectorPropertyInspectorWindow inspectorWindow, IServiceProvider serviceProvider)
        : base("Property Inspector", serviceProvider)
    {
        _inspectorWindow = inspectorWindow;
        Init(_inspectorWindow);
        _inspectorWindow.Init(TitleBarHeight);
        CustomMinimumSize = Size;

        _inspectorWindow.ResizeFromFW(true, (int)Size.X, (int)Size.Y - TitleBarHeight);
    }

}
