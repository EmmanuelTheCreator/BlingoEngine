using System;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using BlingoEngine.Director.Core.Inspector;

namespace BlingoEngine.Director.LGodot.Inspector;

public partial class DirGodotPropertyInspector : BaseGodotWindow, IDirFrameworkPropertyInspectorWindow, IFrameworkFor<DirectorPropertyInspectorWindow>
{

    private readonly DirectorPropertyInspectorWindow _inspectorWindow;

    public DirGodotPropertyInspector(DirectorPropertyInspectorWindow inspectorWindow, IServiceProvider serviceProvider)
        : base("Property Inspector", serviceProvider)
    {
        _inspectorWindow = inspectorWindow;
        Init(_inspectorWindow);
    }

}

