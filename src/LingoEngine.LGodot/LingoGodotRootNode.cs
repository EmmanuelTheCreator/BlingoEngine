using AbstUI.Inputs;
using Godot;
using LingoEngine.Inputs;

namespace LingoEngine.LGodot
{
    public class LingoGodotRootNode
    {
        public Node RootNode { get; }
        public bool WithStageInWindow { get; }

        public LingoGodotRootNode(Node rootNode, bool withStageInWindow) 
        {
            RootNode = rootNode;
            WithStageInWindow = withStageInWindow;
        }
        public IAbstUIFrameworkMouse GetStageMouseNode(Func<LingoMouse> getMouse)
        {
            var godotInstance = new LingoGodotMouseArea(RootNode, new Lazy<LingoMouse>(() => getMouse()));
            return godotInstance;
        }

    }
}
