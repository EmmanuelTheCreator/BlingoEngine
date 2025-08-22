using System;
using System.IO;
using System.Reflection;
using AbstUI.Texts;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using AbstUI.LUnity.Bitmaps;
using UnityEngine;

namespace LingoEngine.Unity.Texts;

/// <summary>
/// Shared implementation for text-based members in the Unity backend.
/// </summary>
public abstract class UnityMemberTextBase<TText> : ILingoFrameworkMemberTextBase, IDisposable where TText : ILingoMemberTextBase
{
    protected TText _lingoMemberText = default!;
    protected string _text = string.Empty;
    protected readonly IAbstFontManager _fontManager;
    private Texture2D? _texture;
    private UnityTexture2D? _textureWrapper;
    private string _fontName = string.Empty;
    private int _fontSize = 12;
    private LingoTextStyle _fontStyle;
    private AColor _textColor = AColor.FromRGB(0, 0, 0);
    private AbstTextAlignment _alignment;
    private int _margin;
    private bool _isLoaded;

    public string Text
    {
        get => _text;
        set => _text = value ?? string.Empty;
    }

    public bool WordWrap { get; set; }
    public int ScrollTop { get; set; }

    public string FontName
    {
        get => _fontName;
        set => _fontName = value ?? string.Empty;
    }

    public int FontSize
    {
        get => _fontSize;
        set { if (value > 0) _fontSize = value; }
    }

    public LingoTextStyle FontStyle
    {
        get => _fontStyle;
        set => _fontStyle = value;
    }

    public AColor TextColor
    {
        get => _textColor;
        set => _textColor = value;
    }

    public AbstTextAlignment Alignment
    {
        get => _alignment;
        set => _alignment = value;
    }

    public int Margin
    {
        get => _margin;
        set => _margin = value;
    }

    public bool IsLoaded => _isLoaded;

    public int Width { get; set; }
    public int Height { get; set; }

    public IAbstTexture2D? TextureLingo => _textureWrapper;

    protected UnityMemberTextBase(IAbstFontManager fontManager)
    {
        _fontManager = fontManager;
    }

    internal void Init(TText member)
    {
        _lingoMemberText = member;
    }

    public void Copy(string text)
    {
        var guiUtilityType = Type.GetType("UnityEngine.GUIUtility, UnityEngine");
        var prop = guiUtilityType?.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
        prop?.SetValue(null, text);
    }

    public string PasteClipboard()
    {
        var guiUtilityType = Type.GetType("UnityEngine.GUIUtility, UnityEngine");
        var prop = guiUtilityType?.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
        return prop?.GetValue(null) as string ?? string.Empty;
    }

    public string ReadText() => File.Exists(_lingoMemberText.FileName) ? File.ReadAllText(_lingoMemberText.FileName) : string.Empty;
    public string ReadTextRtf()
    {
        var rtf = Path.ChangeExtension(_lingoMemberText.FileName, ".rtf");
        return File.Exists(rtf) ? File.ReadAllText(rtf) : string.Empty;
    }

    public void CopyToClipboard() => Copy(Text);
    public void Erase()
    {
        Unload();
        _text = string.Empty;
    }
    public void ImportFileInto() { }
    public void PasteClipboardInto() => _lingoMemberText.Text = PasteClipboard();
    public void Preload() { _isLoaded = true; }
    public void Unload()
    {
        _isLoaded = false;
        _textureWrapper?.Dispose();
        _textureWrapper = null;
        if (_texture != null)
        {
            UnityEngine.Object.Destroy(_texture);
            _texture = null;
        }
    }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public void Dispose() => Unload();

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        var width = Width > 0 ? Width : (int)Math.Ceiling(_fontManager.MeasureTextWidth(Text, FontName, FontSize));
        var info = _fontManager.GetFontInfo(FontName, FontSize);
        var height = Height > 0 ? Height : info.FontHeight;
        if (width <= 0 || height <= 0)
            return null;

        _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var col = new Color(0, 0, 0, 0);
        var colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = col;
        _texture.SetPixels(colors);
        _texture.Apply();

        Width = width;
        Height = height;
        _lingoMemberText.Width = width;
        _lingoMemberText.Height = height;
        _lingoMemberText.RegPoint = new APoint(0, -height / 2);

        _textureWrapper = new UnityTexture2D(_texture);
        return _textureWrapper;
    }
}
