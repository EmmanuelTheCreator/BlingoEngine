using AbstUI.LUnity.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Inputs;
using UnityEngine;

namespace LingoEngine.Unity.Inputs;

public class LingoUnityMouse : AbstUnityMouse<LingoMouse, LingoMouseEvent> , ILingoFrameworkMouse
{
    private LingoMemberBitmap? _cursorImage;

    public LingoUnityMouse(Lazy<LingoMouse> mouse) : base(mouse)
    {
    }

    public void SetCursor(LingoMemberBitmap? image)
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
