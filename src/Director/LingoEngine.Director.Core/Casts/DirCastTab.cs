using System.Collections.Generic;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Windowing.Commands;
using LingoEngine.Director.Core.Scripts.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Events;
using LingoEngine.Texts;
using LingoEngine.Bitmaps;
using LingoEngine.Scripts;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.Core.Casts
{
    public class DirCastTab
    {
        private readonly List<DirCastItem> _items = new();
        private readonly LingoGfxScrollContainer _scroll;
        private readonly LingoGfxWrapPanel _wrap;
        private readonly ILingoCommandManager _commandManager;
        private DirCastItem? _selected;
        private bool _dragging;
        private float _dragStartX, _dragStartY;
        private int _columns = 1;
        private static bool _openingEditor;
        private static readonly object _lock = new();

        public LingoGfxScrollContainer Scroll => _scroll;
        public event Action<ILingoMember, DirCastItem>? MemberSelected;

        public DirCastTab(ILingoFrameworkFactory factory, string name, IEnumerable<ILingoMember> members, IDirectorIconManager iconManager, ILingoCommandManager commandManager)
        {
            _commandManager = commandManager;
            _scroll = factory.CreateScrollContainer(name + "Scroll");
            _scroll.ClipContents = true;
            _wrap = factory.CreateWrapPanel(LingoOrientation.Horizontal, name + "Wrap");
            _scroll.AddItem(_wrap);
            int idx = 0;
            foreach (var m in members)
            {
                var item = new DirCastItem(factory, m, idx++, iconManager);
                _items.Add(item);
                _wrap.AddItem(item.Canvas);
            }
        }

        public void SetViewportWidth(int width)
        {
            _scroll.Width = width;
            _wrap.Width = width;
            _columns = System.Math.Max(1, width / DirCastItem.Width);
        }

        public ILingoMember? HitTest(float x, float y, out DirCastItem? item)
        {
            int col = (int)(x / DirCastItem.Width);
            int row = (int)(y / DirCastItem.Height);
            int idx = row * _columns + col;
            if (idx >= 0 && idx < _items.Count)
            {
                item = _items[idx];
                return item.Member;
            }
            item = null;
            return null;
        }

        public void Select(DirCastItem? selected)
        {
            _selected = selected;
            foreach (var it in _items)
                it.SetSelected(it == selected);
        }

        public int IndexOfMember(ILingoMember member)
        {
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].Member == member)
                    return i;
            return -1;
        }

        public void ScrollToIndex(int index)
        {
            int row = index / _columns;
            _scroll.ScrollVertical = row * DirCastItem.Height;
        }

        public DirCastItem? GetItem(int index) => index >= 0 && index < _items.Count ? _items[index] : null;

        public void HandleMouseEvent(LingoMouseEvent e)
        {
            float x = e.Mouse.MouseH + _scroll.ScrollHorizontal;
            float y = e.Mouse.MouseV + _scroll.ScrollVertical;
            switch (e.Type)
            {
                case LingoMouseEventType.MouseDown:
                    var member = HitTest(x, y, out var item);
                    if (member != null && item != null)
                    {
                        if (e.Mouse.DoubleClick)
                        {
                            OpenEditor(member);
                            Select(item);
                            MemberSelected?.Invoke(member, item);
                        }
                        else
                        {
                            Select(item);
                            MemberSelected?.Invoke(member, item);
                            _dragStartX = x;
                            _dragStartY = y;
                            _dragging = false;
                        }
                    }
                    break;
                case LingoMouseEventType.MouseMove:
                    if (e.Mouse.MouseDown && _selected != null)
                    {
                        float dx = x - _dragStartX;
                        float dy = y - _dragStartY;
                        if (!_dragging && dx * dx + dy * dy > 16)
                        {
                            _dragging = true;
                            DirectorDragDropHolder.StartDrag(_selected.Member, "CastItem");
                        }
                    }
                    break;
                case LingoMouseEventType.MouseUp:
                    _dragging = false;
                    _selected = null;
                    lock (_lock) _openingEditor = false;
                    break;
            }
        }

        private void OpenEditor(ILingoMember member)
        {
            lock (_lock)
            {
                if (_openingEditor) return;
                _openingEditor = true;
            }
            string? windowCode = member switch
            {
                ILingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                LingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                LingoMemberScript => null,
                _ => null
            };
            if (windowCode != null)
                _commandManager.Handle(new OpenWindowCommand(windowCode));
            else if (member is LingoMemberScript script)
                _commandManager.Handle(new OpenScriptCommand(script));
        }
    }
}

