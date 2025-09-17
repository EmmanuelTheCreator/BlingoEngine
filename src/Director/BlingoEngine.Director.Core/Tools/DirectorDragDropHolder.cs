using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Tools
{
    public class DirectorDragDropHolder
    {
        public static IBlingoMember? Member { get; private set; }
        public static IBlingoSprite? Sprite { get; private set; }
        public static bool IsDragging { get; private set; }

        public static void StartDrag(IBlingoMember payload, string type)
        {
            Member = payload;
            IsDragging = true;
        }
        public static void StartDrag(IBlingoSprite payload, string type)
        {
            Sprite = payload;
            IsDragging = true;
        }

        public static void CancelDrag()
        {
            Clear();
        }

        public static void EndDrag()
        {
            Clear();
        }

        private static void Clear()
        {
            Member = null;
            Sprite = null;
            IsDragging = false;
        }
    }
}

