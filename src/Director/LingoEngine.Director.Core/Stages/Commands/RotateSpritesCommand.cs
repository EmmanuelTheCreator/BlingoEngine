using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Commands;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Stages.Commands
{
    public sealed record RotateSpritesCommand(
        IReadOnlyDictionary<LingoSprite2D, float> StartRotations,
        IReadOnlyDictionary<LingoSprite2D, float> EndRotations) : IAbstCommand
    {
        public Action ToUndo(Action updateSelectionBox)
        {
            var undo = StartRotations.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in undo)
                    kv.Key.Rotation = kv.Value;
                updateSelectionBox();
            };
        }

        public Action ToRedo(Action updateSelectionBox)
        {
            var redo = EndRotations.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in redo)
                    kv.Key.Rotation = kv.Value;
                updateSelectionBox();
            };
        }
    }
}
