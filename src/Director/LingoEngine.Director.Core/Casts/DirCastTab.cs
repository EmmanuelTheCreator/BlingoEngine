using System.Collections.Generic;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
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
using LingoEngine.Casts;
using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Director.Core.Casts
{
    public class DirCastTab : IDisposable
    {
        private int _itemMargin = 2;
        private readonly List<DirCastItem> _items = new();
        private readonly LingoGfxScrollContainer _scroll;
        private readonly LingoGfxTabItem _tabItem;
        private readonly LingoGfxWrapPanel _wrap;
        private readonly ILingoCast _cast;
        private readonly ILingoCommandManager _commandManager;
        private readonly ILingoFrameworkFactory _factory;
        private readonly IDirectorIconManager _iconManager;
        private DirCastItem? _selected;
        private DirCastItem? _hoveredItem;
        private DirCastItem? _dragItem;
        private bool _dragging;
        private float _dragStartX, _dragStartY;
        private int _columns = 1;
        private static bool _openingEditor;
        private static readonly object _lock = new();

        public event Action<ILingoMember, DirCastItem>? MemberSelected;
        internal LingoGfxTabItem TabItem => _tabItem;

        public int Width { get; private set; }

        public DirCastTab(ILingoFrameworkFactory factory, ILingoCast cast, IDirectorIconManager iconManager, ILingoCommandManager commandManager)
        {
            _commandManager = commandManager;
            _factory = factory;
            _iconManager = iconManager;
            var tabName = cast.Name ?? $"Cast{cast.Number}";
            _tabItem = factory.CreateTabItem("Cast_" + tabName, tabName);
            _wrap = factory.CreateWrapPanel(AOrientation.Vertical, tabName + "_Wrap");
            _cast = cast;
            _wrap.ItemMargin = new APoint(_itemMargin, _itemMargin);
            _scroll = factory.CreateScrollContainer(tabName + "_Scroll");
            _scroll.ClipContents = true;
            _scroll.AddItem(_wrap);

            _tabItem.Content = _scroll;
            _cast.MemberAdded += MemberAdded;
            _cast.MemberDeleted += MemberDeleted;
            _cast.MemberNameChanged += MemberNameChanged;
        }
        public void Dispose()
        {
            _cast.MemberAdded -= MemberAdded;
            _cast.MemberDeleted -= MemberDeleted;
            _cast.MemberNameChanged -= MemberNameChanged;
        }

        private void MemberNameChanged(ILingoMember member)
        {
            var castItem = _items.FirstOrDefault(i => i.Member == member);
            if (castItem == null) return;
            castItem.SetMember(member);
        }

        private void MemberDeleted(ILingoMember member)
        {
            var castItem = _items.FirstOrDefault(i => i.Member == member);
            if (castItem == null) return;
            castItem.MakeEmpty();
        }

        private void MemberAdded(ILingoMember member)
        {
            var castItem = _items.FirstOrDefault(i => i.Member == member);
            if (castItem != null) return;
            castItem = _items[member.NumberInCast - 1];
            castItem.SetMember(member);
        }

        internal void LoadAllMembers()
        {
            _items.Clear();
            _wrap.RemoveAll();
            for (int i = 1; i <= 999; i++)
            {
                var member = _cast.Member[i];
                var item = new DirCastItem(_factory, member, i, _iconManager);
                _items.Add(item);
                _wrap.AddItem(item.Canvas);
            }
        }

        public void SetViewportSize(int width, int height)
        {
            //_scroll.Width = width;
            Width = width;
            _wrap.Width = Width;
            var itemSize = DirCastItem.Width + _itemMargin;
            var div = (double)(Width + 2) / itemSize;
            _columns = Math.Max(1, (int)Math.Floor(div));
            //  Console.WriteLine("Coluimns:"+ Width + " div=" + div + " / "+ itemSize + "= "+ _columns);
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
            float y = e.Mouse.MouseV + _scroll.ScrollVertical - _tabItem.TopHeight;
            if (y < 0 || x < 0 || x > Width)
            {
                if (_hoveredItem != null)
                {
                    _hoveredItem.SetHovered(false);
                    _hoveredItem = null;
                }
                return;
            }
            var memberHover = HitTest(x, y, out var hoverItem);
            if (hoverItem != _hoveredItem)
            {
                _hoveredItem?.SetHovered(false);
                _hoveredItem = hoverItem;
                _hoveredItem?.SetHovered(true);
            }
            switch (e.Type)
            {
                case LingoMouseEventType.MouseDown:
                    var member = memberHover;
                    var item = hoverItem;
                    if (item != null)
                    {
                        Select(item);
                        if (member != null)
                        {
                            if (e.Mouse.DoubleClick)
                            {
                                OpenEditor(member);
                                MemberSelected?.Invoke(member, item);
                            }
                            else
                            {
                                MemberSelected?.Invoke(member, item);
                                _dragStartX = x;
                                _dragStartY = y;
                                _dragging = false;
                                _dragItem = item;
                            }
                        }
                    }
                    break;
                case LingoMouseEventType.MouseMove:
                    if (e.Mouse.MouseDown && _selected != null && _selected.Member != null)
                    {
                        float dx = x - _dragStartX;
                        float dy = y - _dragStartY;
                        if (!_dragging && dx * dx + dy * dy > 16)
                        {
                            _dragging = true;
                            DirectorDragDropHolder.StartDrag(_selected.Member!, "CastItem");
                        }
                    }
                    break;
                case LingoMouseEventType.MouseUp:
                    if (_dragging && _dragItem?.Member != null && hoverItem != null && hoverItem != _dragItem)
                    {
                        SwapItems(_dragItem, hoverItem);
                    }
                    _dragItem = null;
                    _dragging = false;
                    _selected = null;
                    lock (_lock) _openingEditor = false;
                    DirectorDragDropHolder.EndDrag();
                    break;
            }
        }
        private void SwapItems(DirCastItem source, DirCastItem target)
        {
            int srcIndex = _items.IndexOf(source);
            int dstIndex = _items.IndexOf(target);
            if (srcIndex < 0 || dstIndex < 0) return;

            _cast.SwapMembers(srcIndex + 1, dstIndex + 1);

            var srcMember = source.Member;
            source.SetMember(target.Member);
            target.SetMember(srcMember);
        }

        public ILingoMember? HitTest(float x, float y, out DirCastItem? item)
        {
            int col = (int)(x / (DirCastItem.Width + _itemMargin));
            int row = (int)(y / (DirCastItem.Height + _itemMargin));
            int idx = row * _columns + col;
            if (idx >= 0 && idx < _items.Count)
            {
                item = _items[idx];
                return item.Member;
            }
            item = null;
            return null;
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

