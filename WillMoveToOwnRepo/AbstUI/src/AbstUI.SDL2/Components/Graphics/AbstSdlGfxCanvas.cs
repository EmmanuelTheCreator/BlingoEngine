using AbstUI.Components.Graphics;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.SDL2.Components.Graphics
{
    /// <summary>
    /// SDL implementation of <see cref="IAbstFrameworkGfxCanvas"/> that delegates all drawing
    /// operations to an <see cref="SDLImagePainter"/>.
    /// The canvas itself merely exposes the painter's functionality and renders its texture.
    /// </summary>
    public class AbstSdlGfxCanvas : AbstSdlComponent, IAbstFrameworkGfxCanvas, IFrameworkFor<AbstGfxCanvas>, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;

        private readonly SDLImagePainter _painter;

        public object FrameworkNode => this;

        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                _painter.Name = value + "_Painter";
            }
        }
        public bool Pixilated
        {
            get => _painter.Pixilated;
            set => _painter.Pixilated = value;
        }

        public AbstSdlGfxCanvas(AbstSdlComponentFactory factory, IAbstFontManager fontManager, int width, int height)
            : base(factory)
        {
            _painter = new SDLImagePainter(fontManager, width, height, ComponentContext.Renderer);
            Width = width;
            Height = height;
        }

        public override float Width
        {
            get => base.Width;
            set
            {
                if (Math.Abs(base.Width - value) < float.Epsilon)
                    return;
                base.Width = value;
                _painter.Resize((int)value, _painter.Height);
            }
        }

        public override float Height
        {
            get => base.Height;
            set
            {
                if (Math.Abs(base.Height - value) < float.Epsilon)
                    return;
                base.Height = value;
                _painter.Resize(_painter.Width, (int)value);
            }
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            ComponentContext.Renderer = context.Renderer;
            _painter.Render();
            return _painter.Texture;
        }

        public void Clear(AColor color) => _painter.Clear(color);

        public void SetPixel(APoint point, AColor color) => _painter.SetPixel(point, color);

        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
            => _painter.DrawLine(start, end, color, width);

        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
            => _painter.DrawRect(rect, color, filled, width);

        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
            => _painter.DrawCircle(center, radius, color, filled, width);

        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
            => _painter.DrawArc(center, radius, startDeg, endDeg, segments, color, width);

        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
            => _painter.DrawPolygon(points, color, filled, width);

        public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
            int width = -1, AbstTextAlignment alignment = default, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0)
            => _painter.DrawText(position, text, font, color, fontSize, width, alignment, style, letterSpacing);
        public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
            int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left,
            AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0)
            => _painter.DrawSingleLine(position, text, font, color, fontSize, width, height, alignment, style, letterSpacing);
        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
            => _painter.DrawPicture(data, width, height, position, format);

        public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
            => _painter.DrawPicture(texture, width, height, position);

        public IAbstTexture2D GetTexture(string? name = null) => _painter.GetTexture(name);

        public override void Dispose()
        {
            _painter.Dispose();
            base.Dispose();
        }
    }
}

