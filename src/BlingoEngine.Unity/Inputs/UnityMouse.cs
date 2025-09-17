using AbstUI.LUnity.Inputs;
using BlingoEngine.Bitmaps;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using UnityEngine;

namespace BlingoEngine.Unity.Inputs;

public class BlingoUnityMouse : AbstUnityMouse<BlingoMouse, BlingoMouseEvent>, IBlingoFrameworkMouse
{
    private BlingoMemberBitmap? _cursorImage;

    public BlingoUnityMouse(Lazy<BlingoMouse> mouse) : base(mouse)
    {
    }

    public void SetCursor(BlingoMemberBitmap? image)
    {
        if (image == null) return;
        _cursorImage = image;
        var bmp = image.Framework<Unity.Bitmaps.UnityMemberBitmap>();
        bmp.Preload();
        var tex = bmp.TextureUnity;
        if (tex != null)
            Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
    }
}

