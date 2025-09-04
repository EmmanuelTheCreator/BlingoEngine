using System;
using AbstUI.Components.Graphics;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.Texts;
using AbstUI.Styles;
using AbstUI.LGodot.Components.Graphics;
using AbstUI.LGodot.Styles;
using Godot;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkGfxCanvas"/> that wraps a <see cref="GodotImagePainter"/>.
    /// </summary>
    public partial class AbstGodotGfxCanvas : Control, IAbstFrameworkGfxCanvas, IDisposable, IFrameworkFor<AbstGfxCanvas>
    {
        private readonly GodotImagePainter _painter;
        private AMargin _margin = AMargin.Zero;



        public bool Pixilated
        {
            get => _painter.Pixilated;
            set => _painter.Pixilated = value;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width
        {
            get => Size.X;
            set
            {
                if (Math.Abs(Size.X - value) < float.Epsilon)
                    return;
                Size = new Vector2(value, Size.Y);
                CustomMinimumSize = Size;
                _painter.Resize((int)value, _painter.Height);
                QueueRedraw();
            }
        }

        public float Height
        {
            get => Size.Y;
            set
            {
                if (Math.Abs(Size.Y - value) < float.Epsilon)
                    return;
                Size = new Vector2(Size.X, value);
                CustomMinimumSize = Size;
                _painter.Resize(_painter.Width, (int)value);
                QueueRedraw();
            }
        }
        public bool Visibility { get => Visible; set => Visible = value; }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        public new string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                _painter.Name = value + "_Painter";
            }
        }
        public object FrameworkNode => this;



        public AbstGodotGfxCanvas(AbstGfxCanvas canvas, IAbstFontManager fontManager, int width, int height)
        {
            _painter = new GodotImagePainter((AbstGodotFontManager)fontManager, width, height);
            canvas.Init(this);
            Size = new Vector2(width, height);
            MouseFilter = MouseFilterEnum.Ignore;
            AddChild(_painter.GodotControl);
        }

        public override void _Draw()
        {
            if (Size.X != _painter.Width || Size.Y != _painter.Height)
            {
                Size = new Vector2(_painter.Width, _painter.Height);
                CustomMinimumSize = Size;
            }
        }

        public void Clear(AColor color)
        {
            _painter.Clear(color);
            QueueRedraw();
        }

        public void SetPixel(APoint point, AColor color)
        {
            _painter.SetPixel(point, color);
            QueueRedraw();
        }

        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
        {
            _painter.DrawLine(start, end, color, width);
            QueueRedraw();
        }

        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        {
            _painter.DrawRect(rect, color, filled, width);
            QueueRedraw();
        }

        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        {
            _painter.DrawCircle(center, radius, color, filled, width);
            QueueRedraw();
        }

        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
        {
            _painter.DrawArc(center, radius, startDeg, endDeg, segments, color, width);
            QueueRedraw();
        }

        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
        {
            _painter.DrawPolygon(points, color, filled, width);
            QueueRedraw();
        }

        public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
            int width = -1, AbstTextAlignment alignment = default)
        {
            _painter.DrawText(position, text, font, color, fontSize, width, alignment);
            QueueRedraw();
        }

        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        {
            _painter.DrawPicture(data, width, height, position, format);
            QueueRedraw();
        }

        public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
        {
            _painter.DrawPicture(texture, width, height, position);
            QueueRedraw();
        }

        public IAbstTexture2D GetTexture(string? name = null) => _painter.GetTexture(name);

        public new void Dispose()
        {
            _painter.Dispose();
            base.Dispose();
        }
    }
}

