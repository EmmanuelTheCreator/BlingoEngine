using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AbstUI.LUnity.Bitmaps;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Tools;
using UnityEngine;

namespace LingoEngine.Unity.FilmLoops;

/// <summary>
/// Unity implementation of a film loop member. It composes sprite layers into
/// a single <see cref="Texture2D"/> using the <see cref="LingoFilmLoopComposer"/>.
/// </summary>
public class UnityFilmLoopMember : ILingoFrameworkMemberFilmLoop, IDisposable
{
    private LingoFilmLoopMember _member = null!;
    private UnityTexture2D? _texture;

    /// <inheritdoc/>
    public bool IsLoaded { get; private set; }

    /// <inheritdoc/>
    public byte[]? Media { get; set; }

    /// <inheritdoc/>
    public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;

    /// <inheritdoc/>
    public bool Loop { get; set; } = true;

    /// <inheritdoc/>
    public APoint Offset { get; private set; }

    /// <inheritdoc/>
    public IAbstTexture2D? TextureLingo => _texture;

    /// <summary>
    /// Initializes this instance with its owning member.
    /// </summary>
    internal void Init(LingoFilmLoopMember member)
    {
        _member = member;
    }

    /// <inheritdoc/>
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    /// <inheritdoc/>
    public void Preload() => IsLoaded = true;

    /// <inheritdoc/>
    public void Unload()
    {
        _texture?.Dispose();
        _texture = null;
        IsLoaded = false;
    }

    /// <inheritdoc/>
    public void Erase()
    {
        Media = null;
        Unload();
    }

    /// <inheritdoc/>
    public void ImportFileInto() { }

    /// <inheritdoc/>
    public void CopyToClipboard()
    {
        if (Media == null) return;
        var base64 = Convert.ToBase64String(Media);
        var guiUtilityType = Type.GetType("UnityEngine.GUIUtility, UnityEngine");
        var prop = guiUtilityType?.GetProperty("systemCopyBuffer", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        prop?.SetValue(null, base64);
    }

    /// <inheritdoc/>
    public void PasteClipboardInto()
    {
        var guiUtilityType = Type.GetType("UnityEngine.GUIUtility, UnityEngine");
        var prop = guiUtilityType?.GetProperty("systemCopyBuffer", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        var data = prop?.GetValue(null) as string;
        if (string.IsNullOrEmpty(data)) return;
        try
        {
            Media = Convert.FromBase64String(data);
        }
        catch
        {
        }
    }

    /// <inheritdoc/>
    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor) => _texture;

    /// <inheritdoc/>
    public IAbstTexture2D ComposeTexture(ILingoSprite2DLight hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers, int frame)
    {
        var prep = LingoFilmLoopComposer.Prepare(_member, Framing, layers);
        Offset = prep.Offset;
        int width = prep.Width;
        int height = prep.Height;

        var dest = new Color32[width * height];

        foreach (var info in prep.Layers)
        {
            Texture2D? srcTex = null;
            if (info.Sprite2D.Member is ILingoMemberWithTexture m)
            {
                if (m.RenderToTexture(info.Ink, info.BackColor) is UnityTexture2D ut)
                    srcTex = ut.Texture;
            }
            else if (info.Sprite2D.Texture is UnityTexture2D t)
            {
                srcTex = t.Texture;
            }
            if (srcTex == null)
                continue;

            var colors = srcTex.GetPixels(info.SrcX, srcTex.height - info.SrcY - info.SrcH, info.SrcW, info.SrcH);
            var srcPixels = Array.ConvertAll(colors, c => (Color32)c);
            if (info.DestW != info.SrcW || info.DestH != info.SrcH)
                srcPixels = srcPixels.ScalePixels(info.SrcW, info.SrcH, info.DestW, info.DestH);

            dest.DrawWithMatrix(width, height, srcPixels, info.DestW, info.DestH, info.Transform.Matrix, info.Alpha);
        }

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.SetPixels32(dest);
        tex.Apply();
        _texture?.Dispose();
        _texture = new UnityTexture2D(tex, $"Filmloop_{frame}_Member_{_member.Name}");
        return _texture;
    }

    /// <inheritdoc/>
    public void Dispose() => Unload();

    public bool IsPixelTransparent(int x, int y) => false;
}

