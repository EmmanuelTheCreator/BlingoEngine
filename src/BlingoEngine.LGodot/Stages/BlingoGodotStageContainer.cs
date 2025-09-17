using Godot;
using BlingoEngine.LGodot.Movies;
using BlingoEngine.Stages;

namespace BlingoEngine.LGodot.Stages
{
    public class BlingoGodotStageContainer : IBlingoFrameworkStageContainer
    {
        private Node? _root;
        private Node2D _stageContainer = new Node2D();
        private BlingoGodotStage? _stage1;

        //private BlingoGodotStage? _stage;
        public Node2D Container => _stageContainer;
        public BlingoGodotStage Stage => _stage1!;
        public float X => _stage1!.X;
        public float Y => _stage1!.Y;

        public BlingoGodotStageContainer(BlingoGodotRootNode blingoGodotRootNode)
        {
            _stageContainer.Name = "StageContainer";
            _stageContainer.ZAsRelative = true;
            _root = blingoGodotRootNode.RootNode;
            if (!blingoGodotRootNode.WithStageInWindow)
                _root.AddChild(_stageContainer);
           
        }
        
        public void SetStage(IBlingoFrameworkStage stage)
        {
            _stage1 = stage as BlingoGodotStage;
            if (_stage1 == null) return;
            _stageContainer.AddChild(_stage1);
            _stage1.ZAsRelative = true;
        }

        public void SetScale(float scale)
        {
            if (_stage1 == null) return;
            _stage1.SetScale(scale);
        }
    }
}

