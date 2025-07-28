﻿using Godot;
using LingoEngine.Members;
using LingoEngine.Texts;
using LingoEngine.LGodot.Gfx;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.Core.Windowing.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.Core.UI;
using LingoEngine.Bitmaps;
using LingoEngine.Scripts;
using LingoEngine.Director.Core.Scripts.Commands;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastItem : Control
    {
        private readonly ColorRect _bg;
        private readonly ColorRect _selectionBg;
        private readonly Color _selectedColor;
        private readonly StyleBoxFlat _selectedLabelStyle = new();
        private readonly StyleBoxFlat _normalLabelStyle = new();
        //private readonly CenterContainer _spriteContainer;
        private readonly DirectorMemberThumbnail _thumb;
        private readonly ILingoMember _lingoMember;
        private readonly ColorRect _separator;
        private readonly Action<DirGodotCastItem> _onSelect;
        private readonly ILingoCommandManager _commandManager;
        private readonly Label _caption;
        private Control? _dragHelper;
        private bool _wasClicked;
        private static bool _openingEditor;
        private static object _lock = new object();
        private bool _dragging;
        private Vector2 _dragStart;

        public int LabelHeight { get; set; } = 12;
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;
        public ILingoMember LingoMember => _lingoMember;
        public void SetSelected(bool selected)
        {
            _selectionBg.Visible = selected;
            _bg.Color = selected ? _selectedColor : Colors.DimGray;
            // Labels use the "normal" stylebox for their background, not "panel"
            _caption.AddThemeStyleboxOverride("normal", selected ? _selectedLabelStyle : _normalLabelStyle);
        }
        private readonly ILingoFrameworkFactory _factory;

        public DirGodotCastItem(ILingoMember element, int number, Action<DirGodotCastItem> onSelect, Color selectedColor, ILingoCommandManager commandManager, ILingoFrameworkFactory factory, IDirectorIconManager iconManager)
        {
            Name = "CastItem " + number + ". " + element.Name;
            _lingoMember = element;
            _onSelect = onSelect;
            _commandManager = commandManager;
            _factory = factory;
            _selectedColor = selectedColor;
            CustomMinimumSize = new Vector2(50, 50);
            MouseFilter = MouseFilterEnum.Stop;



            // Selection background - slightly larger than the item itself
            _selectionBg = new ColorRect
            {
                Name = "ItemSelectionBg",
                Color = selectedColor,
                Visible = false,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
                AnchorLeft = 0,
                AnchorTop = 0,
                AnchorRight = 1,
                AnchorBottom = 1,
                OffsetLeft = -1,
                OffsetTop = -1,
                OffsetRight = 1,
                OffsetBottom = 1,
                MouseFilter = MouseFilterEnum.Ignore
            };
            AddChild(_selectionBg);

            // Solid background
            _bg = new ColorRect
            {
                Name = "ItemBackground",
                Color = Colors.White,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
                AnchorLeft = 0,
                AnchorTop = 0,
                AnchorRight = 1,
                AnchorBottom = 1,
                OffsetLeft = 0,
                OffsetTop = 0,
                OffsetRight = 0,
                OffsetBottom = 0,
                MouseFilter = MouseFilterEnum.Ignore
            };
            AddChild(_bg);


            _thumb = new DirectorMemberThumbnail(Width - 1, Height - LabelHeight, _factory, iconManager);
            var thumbCanvas = _thumb.Canvas.Framework<LingoGodotGfxCanvas>();
            thumbCanvas.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(thumbCanvas);

          
            // Bottom label
            _caption = CreateCaption(element, number, selectedColor);

        }

        private Label CreateCaption(ILingoMember element, int number, Color selectedColor)
        {
            var caption = new Label
            {
                Name = "ItemLabel",
                HorizontalAlignment = HorizontalAlignment.Center,
                LabelSettings = new LabelSettings
                {
                    FontSize = 8,
                    FontColor = Colors.Black,
                }
            };
            _selectedLabelStyle.BgColor = selectedColor;
            _normalLabelStyle.BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor();
            AddChild(caption);
            caption.Text = !string.IsNullOrWhiteSpace(element.Name) ? element.NumberInCast + "." + element.Name : number.ToString();
            caption.AddThemeColorOverride("font_color", Colors.Black);
            // Apply background style to the label using the "normal" stylebox
            caption.AddThemeStyleboxOverride("normal", _normalLabelStyle);
            caption.MouseFilter = MouseFilterEnum.Ignore;
            caption.AnchorLeft = 0;
            caption.AnchorRight = 1;
            caption.AnchorTop = 1;
            caption.AnchorBottom = 1;
            caption.OffsetLeft = 0;
            caption.OffsetRight = 0;
            caption.OffsetBottom = 0;
            caption.OffsetTop = -LabelHeight;
            return caption;
        }

        public void SetPosition(int x, int y)
        {
            Position = new Vector2(x, y);
        }
       
        public override void _Input(InputEvent @event)
        {
            if (!IsVisibleInTree()) return;

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2 mousePos = GetGlobalMousePosition();

                Rect2 bounds = new Rect2(GlobalPosition, CustomMinimumSize);

                if (mouseEvent.Pressed && mouseEvent.DoubleClick && !_openingEditor && bounds.HasPoint(mousePos))
                {
                    
                    OpenEditor();
                    _onSelect(this);
                    return;
                }
                else if (_openingEditor && !mouseEvent.Pressed)
                {
                    _openingEditor = false;
                }


                if (!_wasClicked && mouseEvent.Pressed)
                {
                    if (bounds.HasPoint(mousePos))
                    {
                        _onSelect(this);
                        _wasClicked = true;
                    }
                    return;
                }
                else if (_wasClicked && !mouseEvent.Pressed)
                {
                    //_onSelect(this);
                    _wasClicked = false;

                }
            }
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (!IsVisibleInTree()) return;

            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    _dragStart = mb.Position;
                    _dragging = false;
                }
                else
                {
                    _dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion motion)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left) && !_dragging)
                {
                    if (motion.Position.DistanceSquaredTo(_dragStart) > 16)
                    {
                        _dragging = true;
                        DirectorDragDropHolder.StartDrag(_lingoMember, "CastItem");
                        //AcceptEvent(); // Prevent default handling

                        //var preview = new ColorRect
                        //{
                        //    Color = new Color(1f, 1f, 1f, 0.5f),
                        //    Size = CustomMinimumSize
                        //};
                        //// Call start_drag with your data and preview
                        //this.StartDragWorkaround(Variant.From(_lingoMember), preview);
                    }
                }
            }
        }



        private void OpenEditor()
        {
            lock (_lock)
            {
                if (_openingEditor) return;
                _openingEditor = true;
            }
            string? windowCode = _lingoMember switch
            {
                ILingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                LingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                LingoMemberScript script => null,
                _ => null
            };

            if (windowCode != null)
                _commandManager.Handle(new OpenWindowCommand(windowCode));
            else if(_lingoMember is LingoMemberScript script)
                _commandManager.Handle(new OpenScriptCommand(script));
        }

        public void Init()
        {
            _thumb.SetMember(_lingoMember);
        }


        //public override Variant _GetDragData(Vector2 atPosition)
        //{
        //    GD.Print($"CastMemberItem: _GetDragData called at {atPosition} with {_lingoMember.Name}");
        //    var preview = new ColorRect
        //    {
        //        Color = new Color(1f, 1f, 1f, 0.5f),
        //        Size = CustomMinimumSize
        //    };
        //    SetDragPreview(preview);
        //    return Variant.From(_lingoMember);
        //}

        //public override Variant _GetDragData(Vector2 atPosition)
        //{
        //    GD.Print("CastItem: drag triggered at " + atPosition);
        //    var label = new Label { Text = "Dragging " + _lingoMember.Name };
        //    label.CustomMinimumSize = new Vector2(100, 30);
        //    label.Modulate = new Color(1, 1, 0, 0.6f);
        //    SetDragPreview(label);
        //    return Variant.From(_lingoMember);
        //}


    }
}
