using Godot;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.UI;
using AbstUI.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.FrameworkCommunication;
using AbstUI.LGodot.Components;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotToolsWindow : BaseGodotWindow, IDirFrameworkToolsWindow, IFrameworkFor<DirectorToolsWindow>
{
    private StageToolbar _stageToolbar;

    public event Action<int>? IconPressed;

    public DirGodotToolsWindow(DirectorToolsWindow directorToolsWindow, IServiceProvider serviceProvider, IDirectorIconManager iconManager, IAbstCommandManager commandManager, ILingoFrameworkFactory factory)
        : base( "Tools", serviceProvider)
    {
        Init(directorToolsWindow);

        
        
        _stageToolbar = new StageToolbar(iconManager, commandManager, factory);
        var toolbarPanel = _stageToolbar.Panel.Framework<AbstGodotPanel>();
        toolbarPanel.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(toolbarPanel);

        //AddButton("P", StageTool.Pointer);
        //AddButton("M", StageTool.Move);
        //AddButton("R", StageTool.Rotate);
    }

   

  
}
