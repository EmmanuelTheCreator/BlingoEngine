using AbstUI.Commands;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Core;

namespace LingoEngine.Director.Core.Stages
{
    public class DirectorStageWindow : DirectorWindow<IDirFrameworkStageWindow>,
        IAbstCommandHandler<StageToolSelectCommand>, 
        IAbstCommandHandler<MoveSpritesCommand>, 
        IAbstCommandHandler<RotateSpritesCommand>
    {
        private readonly IHistoryManager _historyManager;

        public StageTool SelectedTool { get; private set; }

        public StageIconBar IconBar { get; }

        public DirectorStageWindow(IHistoryManager historyManager, ILingoFrameworkFactory factory, IAbstCommandManager commandManager, ILingoPlayer player, IDirectorEventMediator mediator, IDirStageManager stageManager) : base(factory)
        {
            _historyManager = historyManager;
            IconBar = new StageIconBar(factory, commandManager, player, mediator, stageManager);
        }

        private void UpdateSelectionBox() => Framework.UpdateSelectionBox();
        private void UpdateBoundingBoxes() => Framework.UpdateBoundingBoxes();

        public bool CanExecute(StageToolSelectCommand command) => true;
        public bool Handle(StageToolSelectCommand command)
        {
            SelectedTool = command.Tool;
            return true;
        }

        public bool CanExecute(MoveSpritesCommand command) => true;
        public bool Handle(MoveSpritesCommand command)
        {
            foreach (var kv in command.EndPositions)
            {
                kv.Key.LocH = kv.Value.X;
                kv.Key.LocV = kv.Value.Y;
            }
            _historyManager.Push(command.ToUndo(UpdateSelectionBox), command.ToRedo(UpdateSelectionBox));
            UpdateSelectionBox();
            UpdateBoundingBoxes();
            return true;
        }

        public bool CanExecute(RotateSpritesCommand command) => true;
        public bool Handle(RotateSpritesCommand command)
        {
            foreach (var kv in command.EndRotations)
                kv.Key.Rotation = kv.Value;
            _historyManager.Push(command.ToUndo(UpdateSelectionBox), command.ToRedo(UpdateSelectionBox));
            UpdateSelectionBox();
            UpdateBoundingBoxes();
            return true;
        }
    }
}
