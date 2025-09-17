using AbstUI.Primitives;
using AbstUI.Commands;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Stages.Commands
{
    public sealed record MoveSpritesCommand(
        IReadOnlyDictionary<BlingoSprite2D, APoint> StartPositions,
        IReadOnlyDictionary<BlingoSprite2D, APoint> EndPositions) : IAbstCommand
    {
        public Action ToUndo(Action updateSelectionBox)
        {
            var undo = StartPositions.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in undo)
                {
                    kv.Key.LocH = kv.Value.X;
                    kv.Key.LocV = kv.Value.Y;
                }
                updateSelectionBox();
            };
        }

        public Action ToRedo(Action updateSelectionBox)
        {
            var redo = EndPositions.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in redo)
                {
                    kv.Key.LocH = kv.Value.X;
                    kv.Key.LocV = kv.Value.Y;
                }
                updateSelectionBox();
            };
        }
    }
}

