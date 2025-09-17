using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;

namespace BlingoEngine.FilmLoops
{
    public static class BlingoFilmLoopMemberSpriteExtensions
    {
        public static ARect GetBoundingBoxForFrame(this IEnumerable<BlingoFilmLoopMemberSprite> sprites, int frame)
        {
            var boxes = sprites
                .Select(e => e.GetBoundingBoxForFrame(frame))
                .ToList();

            if (boxes.Count == 0)
                return new ARect();

            var bounds = boxes[0];
            for (int i = 1; i < boxes.Count; i++)
                bounds = bounds.Union(boxes[i]);

            return bounds;
        }

        public static ARect GetBoundingBox(this IEnumerable<BlingoFilmLoopMemberSprite> sprites)
        {
            var boxes = sprites
                .Select(e => e.GetBoundingBox())
                .ToList();

            if (boxes.Count == 0)
                return new ARect();

            var bounds = boxes[0];
            for (int i = 1; i < boxes.Count; i++)
                bounds = bounds.Union(boxes[i]);

            return bounds;
        }
    }
}

