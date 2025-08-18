using System;
using System.Numerics;
using ImGuiNET;

namespace AbstUI.ImGui;

/// <summary>
/// Minimal backend that renders ImGui directly with no external dependencies.
/// Actual rendering will need to be implemented using an appropriate graphics API.
/// </summary>
public sealed class ImGuiImGuiBackend : IDisposable
{
    private bool _inited;

    public void Init(nint window, nint renderer)
    {
        global::ImGuiNET.ImGui.CreateContext();
        global::ImGuiNET.ImGui.StyleColorsLight();
        _inited = true;
    }

    public void Shutdown()
    {
        if (!_inited) return;
        global::ImGuiNET.ImGui.DestroyContext();
        _inited = false;
    }

    public ImGuiViewportPtr BeginFrame()
    {
        global::ImGuiNET.ImGui.NewFrame();
        var viewport = global::ImGuiNET.ImGui.GetMainViewport();
        global::ImGuiNET.ImGui.SetNextWindowPos(viewport.WorkPos);
        global::ImGuiNET.ImGui.SetNextWindowSize(viewport.WorkSize);
        const ImGuiWindowFlags overlayFlags =
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav |
            ImGuiWindowFlags.NoBackground;

        global::ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        global::ImGuiNET.ImGui.Begin("##overlay_root", overlayFlags);
        return viewport;
    }

    /// <summary>
    /// Process a platform event. This is a placeholder for future integration.
    /// </summary>
    public void ProcessEvent(object evt)
    {
        var data = global::ImGuiNET.ImGui.GetDrawData();
        var overlay = global::ImGuiNET.ImGui.GetForegroundDrawList();

        for (var n = 0; n < data.CmdListsCount; n++)
        {
            var src = data.CmdLists[n];
            var vtx = src.VtxBuffer;
            var idx = src.IdxBuffer;

            var idxOffset = 0;
            for (var cmdi = 0; cmdi < src.CmdBuffer.Size; cmdi++)
            {
                var cmd = src.CmdBuffer[cmdi];
                overlay.PushClipRect(
                    new Vector2(cmd.ClipRect.X, cmd.ClipRect.Y),
                    new Vector2(cmd.ClipRect.Z, cmd.ClipRect.W),
                    true);

                for (var i = 0; i < cmd.ElemCount; i += 3)
                {
                    var v0 = vtx[idx[idxOffset + i]];
                    var v1 = vtx[idx[idxOffset + i + 1]];
                    var v2 = vtx[idx[idxOffset + i + 2]];
                    overlay.AddTriangleFilled(v0.pos, v1.pos, v2.pos, v0.col);
                }

                idxOffset += (int)cmd.ElemCount;
                overlay.PopClipRect();
            }
        }
    }
    

    public void NewFrame()
    {
        global::ImGuiNET.ImGui.NewFrame();
    }

    public void EndFrame()
    {
        global::ImGuiNET.ImGui.End();
        global::ImGuiNET.ImGui.PopStyleVar();
        Render();
    }

    public void Render()
    {
        global::ImGuiNET.ImGui.Render();
        // TODO: submit draw data to the renderer
    }

    #region Textures

    private readonly Dictionary<nint, nint> _tex = new();
    private long _next = 1;

    public nint RegisterTexture(nint textureHandle)
    {
        var id = new nint(_next++);
        _tex[id] = textureHandle;
        return id;
    }

    public nint GetTexture(nint id) => _tex[id];

    #endregion

    public void Dispose() => Shutdown();
}

