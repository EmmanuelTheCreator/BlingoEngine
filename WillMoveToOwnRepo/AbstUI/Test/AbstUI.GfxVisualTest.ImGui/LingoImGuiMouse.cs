using System;
using AbstUI.Inputs;
using AbstUI.ImGui.Inputs;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Inputs;

namespace LingoEngine.ImGui.Inputs;

/// <summary>
/// ImGui-based mouse bridge for LingoEngine.
/// </summary>
public class LingoImGuiMouse : ImGuiMouse<LingoMouseEvent>, ILingoFrameworkMouse
{
    public LingoImGuiMouse(Lazy<AbstMouse<LingoMouseEvent>> mouse) : base(mouse)
    {
    }

    public void SetCursor(AMouseCursor cursorType)
    {
        // ImGui handles cursors internally; no explicit handling needed.
    }

    public void SetCursor(LingoMemberBitmap? image)
    {
        // Custom cursor images are not supported in this stub.
    }
}

