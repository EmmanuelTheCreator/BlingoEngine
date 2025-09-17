using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Bitmaps;
using BlingoEngine.FilmLoops;
using AbstUI.Primitives;
using AbstUI.Core;
using BlingoEngine.Core;
using AbstUI.Components.Graphics;

namespace BlingoEngine.Director.Core.Casts;

/// <summary>
/// Simple cross-platform thumbnail preview for cast members.
/// </summary>
public class DirectorMemberThumbnail : IDisposable
{
    private readonly float _yOffset;
    private readonly float _xOffset;
    private readonly bool _isCustomCanvas;
    private readonly float _rectWidth = 0;
    private readonly float _iconYOffset = 0;
    public int ThumbWidth { get; }
    public int ThumbHeight { get; }

    private IBlingoMember? _cachedMember;
    private IAbstUITextureUserSubscription? _pictureSubscription;
    private IAbstUITextureUserSubscription? _iconSubscription;

    /// <summary>Canvas used for drawing the preview.</summary>
    public AbstGfxCanvas Canvas { get; }
    private readonly IDirectorIconManager? _iconManager;

    public DirectorMemberThumbnail(int width, int height, IBlingoFrameworkFactory factory, IDirectorIconManager? iconManager = null, AbstGfxCanvas? canvas = null, int xOffset = 0, int yOffset = 0)
    {
        _xOffset = xOffset;
        _yOffset = yOffset;
        _rectWidth = width;
        // godot fix 
        if (xOffset > 0 && BlingoEngineGlobal.RunFramework == AbstEngineRunFramework.Godot)
        {
            _xOffset += 0.5f;
            _yOffset += 0.5f;
            _rectWidth--;
            _iconYOffset = 0.5f;
        }
        _isCustomCanvas = canvas != null;
        ThumbWidth = width;
        ThumbHeight = height;

        Canvas = canvas ?? factory.CreateGfxCanvas("MemberThumbnailCanvas", width, height);
        _iconManager = iconManager;

    }

    /// <summary>
    /// Draws the specified member on the canvas.
    /// </summary>
    public void SetMember(IBlingoMember member)
    {
        if (!_isCustomCanvas)
            Canvas.Clear(DirectorColors.BG_WhiteMenus);
        DrawBorder();

        if (!ReferenceEquals(_cachedMember, member))
        {
            _pictureSubscription?.Release();
            _pictureSubscription = null;
            _iconSubscription?.Release();
            _iconSubscription = null;

            if (_iconManager != null)
            {
                var icon = BlingoMemberTypeIcons.GetIconType(member);
                if (icon.HasValue)
                {
                    var data = _iconManager.Get(icon.Value);
                    _iconSubscription = data.AddUser(this);
                }
            }

            _cachedMember = member;
        }

        switch (member)
        {
            case BlingoMemberBitmap pic:
                if (_pictureSubscription == null)
                {
                    pic.Preload();
                    if (pic.TextureBlingo != null)
                        _pictureSubscription = pic.TextureBlingo.AddUser(this);
                }
                var tex = _pictureSubscription?.Texture;
                if (tex != null)
                    Canvas.DrawPicture(tex, ThumbWidth - 2, ThumbHeight - 2, new APoint(_xOffset + 1, _yOffset + 2));
                break;
            case IBlingoMemberTextBase text:
                DrawText(GetPreviewText(text));
                break;
            case BlingoFilmLoopMember filmloop:
                DrawText(filmloop.Name);
                break;
            case BlingoMemberSound sound:
                DrawText(sound.Name);
                break;
        }

        if (_iconSubscription != null)
        {
            var miniIconSize = 16;
            var data = _iconSubscription.Texture;
            var x = ThumbWidth - miniIconSize - 1;
            var y = ThumbHeight - miniIconSize - 1 + _iconYOffset;
            Canvas.DrawRect(ARect.New(_xOffset + x, _yOffset + y, miniIconSize, miniIconSize), AColors.White, true);
            Canvas.DrawPicture(data, miniIconSize - 2, miniIconSize - 2, new APoint(_xOffset + x + 1, _yOffset + y + 1));
        }
    }

    public void SetEmpty()
    {
        _cachedMember = null;
        _pictureSubscription?.Release();
        _pictureSubscription = null;
        _iconSubscription?.Release();
        _iconSubscription = null;
        if (!_isCustomCanvas)
            Canvas.Clear(DirectorColors.BG_WhiteMenus);
        DrawBorder();
    }

    private void DrawBorder()
    {
        Canvas.DrawRect(ARect.New(_xOffset, _yOffset, _rectWidth, ThumbHeight), AColors.White, true);
        Canvas.DrawRect(ARect.New(_xOffset, _yOffset, _rectWidth, ThumbHeight), AColors.Gray, false);
    }

    private void DrawPicture(BlingoMemberBitmap picture)
    {
        picture.Preload();
        if (picture.TextureBlingo == null)
            return;
        var w = picture.TextureBlingo.Width;
        var h = picture.TextureBlingo.Height;
        Canvas.DrawPicture(picture.TextureBlingo, ThumbWidth - 2, ThumbHeight - 2, new APoint(_xOffset + 1, _yOffset + 2));
    }

    private void DrawText(string text)
    {
        const int fontSize = 10;
        const int lineHeight = fontSize + 2;

        int lineCount = text.Split('\n').Length;
        int textHeight = lineCount * lineHeight;
        var startY = _yOffset + (int)Math.Max((ThumbHeight - textHeight) / 2f, 0);

        int maxWidth = ThumbWidth - 4;
        Canvas.DrawText(new APoint(_xOffset + 2, startY), text, null, new AColor(0, 0, 0), fontSize, maxWidth);
    }

    private static string GetPreviewText(IBlingoMemberTextBase text)
    {
        var lines = text.Text.Replace("\r", "").Split('\n');
        var sb = new System.Text.StringBuilder();
        int count = Math.Min(4, lines.Length);
        for (int i = 0; i < count; i++)
        {
            var line = lines[i];
            if (line.Length > 14)
                line = line.Substring(0, 14);
            sb.Append(line);
            if (i < count - 1)
                sb.Append('\n');
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        _pictureSubscription?.Release();
        _iconSubscription?.Release();
        Canvas.Dispose();
    }
}

