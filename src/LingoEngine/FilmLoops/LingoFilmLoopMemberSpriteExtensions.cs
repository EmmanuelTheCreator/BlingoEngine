using System.Collections.Generic;
using System.Linq;
using LingoEngine.Primitives;

namespace LingoEngine.FilmLoops
{
    public static class LingoFilmLoopMemberSpriteExtensions
    {
        public static LingoRect GetBoundingBoxForFrame(this IEnumerable<LingoFilmLoopMemberSprite> sprites, int frame)
        {
            var boxes = sprites
                .Select(e => e.GetBoundingBoxForFrame(frame))
                .ToList();

            if (boxes.Count == 0)
                return new LingoRect();

            var bounds = boxes[0];
            for (int i = 1; i < boxes.Count; i++)
                bounds = bounds.Union(boxes[i]);

            return bounds;
        }

        public static LingoRect GetBoundingBox(this IEnumerable<LingoFilmLoopMemberSprite> sprites)
        {
            var boxes = sprites
                .Select(e => e.GetBoundingBox())
                .ToList();

            if (boxes.Count == 0)
                return new LingoRect();

            var bounds = boxes[0];
            for (int i = 1; i < boxes.Count; i++)
                bounds = bounds.Union(boxes[i]);

            return bounds;
        }
    }
}
