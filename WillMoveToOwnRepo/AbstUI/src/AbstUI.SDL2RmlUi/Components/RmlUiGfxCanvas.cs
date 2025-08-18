using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.SDL2;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Bitmaps;
using RmlUiNet;

namespace AbstUI.SDL2RmlUi.Components;

/// <summary>
/// Gfx canvas implementation that renders its SDL texture into a RmlUi image element.
/// </summary>
public class RmlUiGfxCanvas : AbstSdlGfxCanvas
{
    private readonly Element _element;
    private readonly nint _renderer;

    public RmlUiGfxCanvas(
        AbstSdlComponentFactory factory,
        IAbstFontManager fontManager,
        ElementDocument document,
        nint renderer,
        int width,
        int height) : base(factory, fontManager, width, height)
    {
        _renderer = renderer;
        _element = document.AppendChildTag("img");
        _element.SetAttribute("width", width.ToString());
        _element.SetAttribute("height", height.ToString());
        _element.SetProperty("display", "block");
        _element.SetProperty("position", "absolute");
    }

    public new object FrameworkNode => _element;

    public new float X
    {
        get => base.X;
        set
        {
            base.X = value;
            _element.SetProperty("left", $"{value}px");
        }
    }

    public new float Y
    {
        get => base.Y;
        set
        {
            base.Y = value;
            _element.SetProperty("top", $"{value}px");
        }
    }

    public new float Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            _element.SetAttribute("width", ((int)value).ToString());
        }
    }

    public new float Height
    {
        get => base.Height;
        set
        {
            base.Height = value;
            _element.SetAttribute("height", ((int)value).ToString());
        }
    }

    public new bool Visibility
    {
        get => base.Visibility;
        set
        {
            base.Visibility = value;
            _element.SetProperty("display", value ? "block" : "none");
        }
    }

    public new AMargin Margin
    {
        get => base.Margin;
        set
        {
            base.Margin = value;
            _element.SetProperty("margin", $"{value.Top}px {value.Right}px {value.Bottom}px {value.Left}px");
        }
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        var wasDirty = _dirty;
        var texture = base.Render(context);
        nint texHandle = (nint)texture;
        if (wasDirty && texHandle != nint.Zero)
        {
            var tex = new SdlTexture2D(texHandle, (int)Width, (int)Height);
            var base64 = tex.ToPngBase64(context.Renderer);
            _element.SetAttribute("src", $"data:image/png;base64,{base64}");
        }
        return texture;
    }

    public override void Dispose()
    {
        base.Dispose();
        _element.GetParentNode()?.RemoveChild(_element);
    }
}

