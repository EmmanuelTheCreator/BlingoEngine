using System;
using System.Collections.Generic;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.FilmLoops;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlingoEngine.Blazor.FilmLoops;

/// <summary>
/// Blazor framework implementation for film loop members. It composes the
/// active layers into an off-screen canvas and exposes the resulting texture
/// to sprites.
/// </summary>
public class BlingoBlazorMemberFilmLoop : IBlingoFrameworkMemberFilmLoop, IDisposable
{
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private BlingoFilmLoopMember _member = null!;
    private AbstBlazorTexture2D? _texture;

    public bool IsLoaded { get; private set; }
    public byte[]? Media { get; set; }
    public BlingoFilmLoopFraming Framing { get; set; } = BlingoFilmLoopFraming.Auto;
    public bool Loop { get; set; } = true;
    public APoint Offset { get; private set; }
    public IAbstTexture2D? TextureBlingo => _texture;

    public BlingoBlazorMemberFilmLoop(IJSRuntime js, AbstUIScriptResolver scripts)
    {
        _js = js;
        _scripts = scripts;
    }

    internal void Init(BlingoFilmLoopMember member)
    {
        _member = member;
    }

    public void Preload()
    {
        if (IsLoaded)
            return;
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }

    public void Unload()
    {
        IsLoaded = false;
        _texture?.Dispose();
        _texture = null;
    }

    public void Erase()
    {
        Media = null;
        Unload();
    }

    public void ImportFileInto() { }
    public void CopyToClipboard() { }
    public void PasteClipboardInto() { }
    public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }

    public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
        => _texture;

    public IAbstTexture2D ComposeTexture(IBlingoSprite2DLight hostSprite, IReadOnlyList<BlingoSprite2DVirtual> layers, int frame)
    {
        var prep = BlingoFilmLoopComposer.Prepare(_member, Framing, layers);
        Offset = prep.Offset;
        int width = prep.Width;
        int height = prep.Height;

        var tex = AbstBlazorTexture2D.CreateAsync(_js, width, height).GetAwaiter().GetResult();
        var ctx = _scripts.CanvasGetContext(tex.Canvas, true).GetAwaiter().GetResult();
        _scripts.CanvasClear(ctx, "rgba(0,0,0,0)", width, height).GetAwaiter().GetResult();

        foreach (var info in prep.Layers)
        {
            AbstBlazorTexture2D? srcTex = null;
            if (info.Sprite2D.Member is IBlingoMemberWithTexture memberWithTexture)
            {
                srcTex = memberWithTexture.RenderToTexture(info.Ink, info.BackColor) as AbstBlazorTexture2D;
            }
            else if (info.Sprite2D.Texture is AbstBlazorTexture2D existing)
            {
                srcTex = existing;
            }
            if (srcTex == null)
                continue;

            var m = info.Transform.Matrix;
            ctx.InvokeVoidAsync("save").GetAwaiter().GetResult();
            ctx.InvokeVoidAsync("setTransform", m.M11, m.M12, m.M21, m.M22, m.M31, m.M32).GetAwaiter().GetResult();
            _scripts.CanvasSetGlobalAlpha(ctx, info.Alpha).GetAwaiter().GetResult();
            ctx.InvokeVoidAsync("drawImage", srcTex.Canvas, info.SrcX, info.SrcY, info.SrcW, info.SrcH, 0, 0, info.DestW, info.DestH).GetAwaiter().GetResult();
            ctx.InvokeVoidAsync("restore").GetAwaiter().GetResult();
        }

        _texture = tex;
        return tex;
    }

    public void Dispose() => _texture?.Dispose();
    public bool IsPixelTransparent(int x, int y) => false;
}

