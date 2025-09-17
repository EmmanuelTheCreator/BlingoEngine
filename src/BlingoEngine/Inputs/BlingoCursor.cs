using AbstUI.Primitives;
using BlingoEngine.Bitmaps;

namespace BlingoEngine.Inputs
{
    public class BlingoCursor : IBlingoCursor
    {
        private IBlingoFrameworkMouse _frameworkObj;
        private int _lastCursor;
        private int _cursor;
        private AMouseCursor _cursorType;
        private bool _isCursorVisible = true;
        private BlingoMemberBitmap? image;
        public int this[int index]
        {
            get => _cursor;
            set => Cursor = value;
        }

        public BlingoCursor(IBlingoFrameworkMouse frameworkObj)
        {
            _frameworkObj = frameworkObj;
        }
        public BlingoMemberBitmap? Image
        {
            get => image; set
            {
                image = value;
                CursorType = AMouseCursor.Custom;
            }
        }
        public int Cursor
        {
            get => _cursor;
            set
            {
                _cursor = value;
                _lastCursor = value;
                _cursorType = (AMouseCursor)value;
                if (value == 100)
                    _frameworkObj.SetCursor(Image);
                else
                    _frameworkObj.SetCursor((AMouseCursor)value);
            }
        }
        public AMouseCursor CursorType { get => _cursorType; set => Cursor = (int)value; }

        public bool IsCursorVisible => _isCursorVisible;

        public void HideCursor()
        {
            _isCursorVisible = false;
            _frameworkObj.HideMouse(true);
        }

        public void ResetCursor() => Cursor = _lastCursor;

        public void ShowCursor()
        {
            _isCursorVisible = true;
            _frameworkObj.HideMouse(false);
        }
    }
}

