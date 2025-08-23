using System;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Components.Graphics;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Components.Texts;

namespace AbstUI.SDL2.Components.Inputs;

internal class AbstSdlColorPickerPopup : AbstSdlPanel, ISdlFocusable, IHandleSdlEvent, IDisposable
{
    private bool _focused;
    public bool HasFocus => _focused;
    public void SetFocus(bool focus) => _focused = focus;

    private readonly AbstSdlInputNumber<int> _rInput;
    private readonly AbstSdlInputNumber<int> _gInput;
    private readonly AbstSdlInputNumber<int> _bInput;
    private readonly AbstSdlInputNumber<int> _aInput;
    private readonly AbstSdlInputNumber<float> _hInput;
    private readonly AbstSdlInputNumber<float> _sInput;
    private readonly AbstSdlInputNumber<float> _vInput;
    private readonly AbstSdlInputText _hexInput;
    private readonly AbstGfxCanvas _canvas;

    private bool _suppress;

    public AColor Color { get; private set; } = AColors.Black;
    public event Action<AColor>? ColorChanged;

    private const int CanvasWidth = 180;
    private const int CanvasHeight = 80;
    private const int RowHeight = 24;

    public AbstSdlColorPickerPopup(AbstSdlComponentFactory factory) : base(factory)
    {
        Width = 200;
        Height = CanvasHeight + RowHeight * 4 + 16; // canvas + inputs + margins
        BackgroundColor = AColors.White;
        BorderColor = AColors.Black;
        BorderWidth = 1;

        _canvas = factory.CreateGfxCanvas("cp_canvas", CanvasWidth, CanvasHeight);

        _rInput = new AbstSdlInputNumber<int>(factory) { Min = 0, Max = 255, Width = 50, Height = 20 };
        _gInput = new AbstSdlInputNumber<int>(factory) { Min = 0, Max = 255, Width = 50, Height = 20 };
        _bInput = new AbstSdlInputNumber<int>(factory) { Min = 0, Max = 255, Width = 50, Height = 20 };
        _aInput = new AbstSdlInputNumber<int>(factory) { Min = 0, Max = 255, Width = 50, Height = 20 };
        _hInput = new AbstSdlInputNumber<float>(factory) { Min = 0, Max = 360, Width = 50, Height = 20 };
        _sInput = new AbstSdlInputNumber<float>(factory) { Min = 0, Max = 100, Width = 50, Height = 20 };
        _vInput = new AbstSdlInputNumber<float>(factory) { Min = 0, Max = 100, Width = 50, Height = 20 };
        _hexInput = new AbstSdlInputText(factory, false) { Width = 60, Height = 20, MaxLength = 8 };

        AddItem(_canvas.Framework<IAbstFrameworkLayoutNode>());
        AddItem(_rInput);
        AddItem(_gInput);
        AddItem(_bInput);
        AddItem(_aInput);
        AddItem(_hInput);
        AddItem(_sInput);
        AddItem(_vInput);
        AddItem(_hexInput);

        DrawColorRect();

        _rInput.ValueChanged += RgbaChanged;
        _gInput.ValueChanged += RgbaChanged;
        _bInput.ValueChanged += RgbaChanged;
        _aInput.ValueChanged += RgbaChanged;
        _hInput.ValueChanged += HsvChanged;
        _sInput.ValueChanged += HsvChanged;
        _vInput.ValueChanged += HsvChanged;
        _hexInput.ValueChanged += HexChanged;

        SetColor(AColors.Black);
    }

    public void SetColor(AColor c)
    {
        Color = c;
        _suppress = true;
        _rInput.Value = c.R;
        _gInput.Value = c.G;
        _bInput.Value = c.B;
        _aInput.Value = c.A;
        var (h, s, v) = c.ToHsv();
        _hInput.Value = h;
        _sInput.Value = s;
        _vInput.Value = v;
        _hexInput.Text = c.ToHex().TrimStart('#');
        _suppress = false;
    }

    private void RgbaChanged()
    {
        if (_suppress) return;
        _suppress = true;
        byte r = (byte)_rInput.Value;
        byte g = (byte)_gInput.Value;
        byte b = (byte)_bInput.Value;
        byte a = (byte)_aInput.Value;
        Color = new AColor(r, g, b, a);
        var (h, s, v) = Color.ToHsv();
        _hInput.Value = h;
        _sInput.Value = s;
        _vInput.Value = v;
        _hexInput.Text = Color.ToHex().TrimStart('#');
        _suppress = false;
        ColorChanged?.Invoke(Color);
    }

    private void HsvChanged()
    {
        if (_suppress) return;
        _suppress = true;
        var (r, g, b) = AColor.HsvToRgb(_hInput.Value, _sInput.Value, _vInput.Value);
        _rInput.Value = r;
        _gInput.Value = g;
        _bInput.Value = b;
        Color = new AColor(r, g, b, (byte)_aInput.Value);
        _hexInput.Text = Color.ToHex().TrimStart('#');
        _suppress = false;
        ColorChanged?.Invoke(Color);
    }

    private void HexChanged()
    {
        if (_suppress) return;
        try
        {
            var c = AColor.FromHex(_hexInput.Text);
            _suppress = true;
            _rInput.Value = c.R;
            _gInput.Value = c.G;
            _bInput.Value = c.B;
            _aInput.Value = c.A;
            var (h, s, v) = c.ToHsv();
            _hInput.Value = h;
            _sInput.Value = s;
            _vInput.Value = v;
            _suppress = false;
            Color = c;
            ColorChanged?.Invoke(Color);
        }
        catch
        {
            _suppress = false;
        }
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility) return default;

        int leftLabelX = 4; int leftInputX = 24;
        int rightLabelX = 90; int rightInputX = 120;
        int inputsY = CanvasHeight + 8;

        _canvas.X = X+ leftInputX -15; _canvas.Y =Y+ inputsY - _canvas.Height -5;

        _rInput.X = leftInputX; _rInput.Y = inputsY;
        _gInput.X = leftInputX; _gInput.Y = inputsY + RowHeight;
        _bInput.X = leftInputX; _bInput.Y = inputsY + RowHeight * 2;
        _aInput.X = leftInputX; _aInput.Y = inputsY + RowHeight * 3;

        _hInput.X = rightInputX; _hInput.Y = inputsY;
        _sInput.X = rightInputX; _sInput.Y = inputsY + RowHeight;
        _vInput.X = rightInputX; _vInput.Y = inputsY + RowHeight * 2;
        _hexInput.X = rightInputX; _hexInput.Y = inputsY + RowHeight * 3;


        var res = base.Render(context);
        if (res.Texture != nint.Zero)
        {
            var prev = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, res.Texture);
            DrawLabel("R", leftLabelX, inputsY, context);
            DrawLabel("G", leftLabelX, inputsY + RowHeight, context);
            DrawLabel("B", leftLabelX, inputsY + RowHeight * 2, context);
            DrawLabel("A", leftLabelX, inputsY + RowHeight * 3, context);
            DrawLabel("H", rightLabelX, inputsY, context);
            DrawLabel("S", rightLabelX, inputsY + RowHeight, context);
            DrawLabel("V", rightLabelX, inputsY + RowHeight * 2, context);
            DrawLabel("Hex", rightLabelX, inputsY + RowHeight * 3, context);
            SDL.SDL_SetRenderTarget(context.Renderer, prev);
        }

        return res;
    }

    private void DrawColorRect()
    {
        _canvas.Clear(AColor.FromRGB(0, 0, 0));
        for (int x = 0; x < CanvasWidth; x++)
        {
            for (int y = 0; y < CanvasHeight; y++)
            {
                float h = x / (float)CanvasWidth * 360f;
                float v = 100f - y / (float)CanvasHeight * 100f;
                var (r, g, b) = AColor.HsvToRgb(h, 100f, v);
                _canvas.SetPixel(new APoint(x, y), new AColor(r, g, b));
            }
        }
    }

    private ISdlFontLoadedByUser? _font;
    private void DrawLabel(string text, int x, int y, AbstSDLRenderContext ctx)
    {
        _font ??= ctx.SdlFontManager.GetTyped(this, null, 12);
        SDL.SDL_Color col = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
        nint surf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, text, col);
        nint tex = SDL.SDL_CreateTextureFromSurface(ctx.Renderer, surf);
        SDL.SDL_FreeSurface(surf);
        SDL.SDL_QueryTexture(tex, out _, out _, out int w, out int h);
        SDL.SDL_Rect dst = new SDL.SDL_Rect { x = x, y = y + 4, w = w, h = h };
        SDL.SDL_RenderCopy(ctx.Renderer, tex, IntPtr.Zero, ref dst);
        SDL.SDL_DestroyTexture(tex);
    }

    public override void Dispose()
    {
        _rInput.Dispose();
        _gInput.Dispose();
        _bInput.Dispose();
        _aInput.Dispose();
        _hInput.Dispose();
        _sInput.Dispose();
        _vInput.Dispose();
        _hexInput.Dispose();
        _canvas.Dispose();
        _font?.Release();
        base.Dispose();
    }

    public void HandleEvent(AbstSDLEvent e)
    {
        ref var ev = ref e.Event;
        if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
        {
            if (ev.button.x >= _canvas.X && ev.button.x <= _canvas.X + CanvasWidth &&
                ev.button.y >= _canvas.Y && ev.button.y <= _canvas.Y + CanvasHeight)
            {
                int lx = ev.button.x - (int)_canvas.X;
                int ly = ev.button.y - (int)_canvas.Y;
                float h = lx / (float)CanvasWidth * 360f;
                float v = 100f - ly / (float)CanvasHeight * 100f;
                var (r, g, b) = AColor.HsvToRgb(h, 100f, v);
                SetColor(new AColor(r, g, b, (byte)_aInput.Value));
                ColorChanged?.Invoke(Color);
            }
            e.StopPropagation = true;
        }
    }
}
