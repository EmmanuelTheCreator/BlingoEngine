using AbstUI.Inputs;
using AbstUI.LGodot;
using Godot;
using BlingoEngine.Inputs;

namespace BlingoEngine.LGodot
{
    public class BlingoGodotRootNode : IAbstGodotRootNode
    {
        public Node RootNode { get; }
        public bool WithStageInWindow { get; }

        public BlingoGodotRootNode(Node rootNode, bool withStageInWindow) 
        {
            RootNode = rootNode;
            WithStageInWindow = withStageInWindow;
        }
        public IAbstFrameworkMouse GetStageMouseNode(Func<BlingoMouse> getMouse)
        {
            var godotInstance = new BlingoGodotMouseArea(RootNode, new Lazy<IAbstMouseInternal>(() => getMouse()));
            return godotInstance;
        }

    }
}

