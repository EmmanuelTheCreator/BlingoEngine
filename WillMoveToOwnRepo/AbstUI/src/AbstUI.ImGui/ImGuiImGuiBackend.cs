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
        ImGui.CreateContext();
        ImGui.StyleColorsLight();
        _inited = true;
    }

    public void Shutdown()
    {
        if (!_inited) return;
        ImGui.DestroyContext();
        _inited = false;
    }

    public ImGuiViewportPtr BeginFrame()
    {
        ImGui.NewFrame();
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        const ImGuiWindowFlags overlayFlags =
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav |
            ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("##overlay_root", overlayFlags);
        return viewport;
    }

    /// <summary>
    /// Process a platform event. This is a placeholder for future integration.
    /// </summary>
    public void ProcessEvent(object evt)
    {
        // TODO: translate platform events to ImGui IO events.
    }

    public void NewFrame()
    {
        ImGui.NewFrame();
    }

    public void EndFrame()
    {
        ImGui.End();
        ImGui.PopStyleVar();
        Render();
    }

    public void Render()
    {
        ImGui.Render();
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

