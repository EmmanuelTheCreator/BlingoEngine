using System.Numerics;
using ImGuiNET;




namespace AbstUI.Blazor
{

    public sealed class ImGuiBlazorBackend : IDisposable
    {
        private nint _window;
        private nint _renderer;
        private nint _fontTex;
        private bool _inited;
        private ulong _ticksLast;

        public void Init(nint window, nint renderer)
        {
            _window = window;
            _renderer = renderer;

            ImGui.CreateContext();
            ImGui.StyleColorsLight();

            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

            CreateFontTexture();
            _ticksLast = Blazor_GetPerformanceCounter();
            _inited = true;
        }

        public void Shutdown()
        {
            if (!_inited) return;
            var io = ImGui.GetIO();
            io.Fonts.SetTexID(nint.Zero);
            if (_fontTex != nint.Zero) { Blazor_DestroyTexture(_fontTex); _fontTex = nint.Zero; }
            ImGui.DestroyContext();
            _inited = false;
        }
        public ImGuiViewportPtr BeginFrame()
        {
            NewFrame();
            var imGuiViewPort = ImGui.GetMainViewport();

            ImGui.SetNextWindowPos(imGuiViewPort.WorkPos);
            ImGui.SetNextWindowSize(imGuiViewPort.WorkSize);
            const ImGuiWindowFlags overlayFlags =
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav |
                ImGuiWindowFlags.NoBackground;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.Begin("##overlay_root", overlayFlags);
            return imGuiViewPort;
        }
        public void Dispose() => Shutdown();

        /// <summary>Call this for each Blazor event.</summary>
        public void ProcessEvent(ref Blazor_Event e)
        {
            var io = ImGui.GetIO();

            switch (e.type)
            {
                case Blazor_EventType.Blazor_MOUSEMOTION:
                    io.MousePos = new Vector2(e.motion.x, e.motion.y);
                    break;

                case Blazor_EventType.Blazor_MOUSEBUTTONDOWN:
                case Blazor_EventType.Blazor_MOUSEBUTTONUP:
                    {
                        bool down = e.type == Blazor_EventType.Blazor_MOUSEBUTTONDOWN;
                        if (e.button.button == Blazor_BUTTON_LEFT) io.AddMouseButtonEvent(0, down);
                        if (e.button.button == Blazor_BUTTON_RIGHT) io.AddMouseButtonEvent(1, down);
                        if (e.button.button == Blazor_BUTTON_MIDDLE) io.AddMouseButtonEvent(2, down);
                        break;
                    }

                case Blazor_EventType.Blazor_MOUSEWHEEL:
                    io.AddMouseWheelEvent(e.wheel.preciseX, e.wheel.preciseY);
                    break;

                case Blazor_EventType.Blazor_TEXTINPUT:
                    {
                        unsafe
                        {
                            fixed (byte* p = e.text.text) // single fixed is OK
                            {
                                int len = 0; while (len < 32 && p[len] != 0) len++;
                                if (len > 0)
                                {
                                    string s = System.Text.Encoding.UTF8.GetString(p, len);
                                    ImGui.GetIO().AddInputCharactersUTF8(s);
                                }
                            }
                        }
                        break;
                    }

                case Blazor_EventType.Blazor_KEYDOWN:
                case Blazor_EventType.Blazor_KEYUP:
                    {
                        bool down = e.type == Blazor_EventType.Blazor_KEYDOWN;
                        var key = e.key.keysym.scancode;

                        io.AddKeyEvent(ImGuiKey.ModCtrl, Blazor_GetModState().HasFlag(Blazor_Keymod.KMOD_CTRL));
                        io.AddKeyEvent(ImGuiKey.ModShift, Blazor_GetModState().HasFlag(Blazor_Keymod.KMOD_SHIFT));
                        io.AddKeyEvent(ImGuiKey.ModAlt, Blazor_GetModState().HasFlag(Blazor_Keymod.KMOD_ALT));
                        io.AddKeyEvent(ImGuiKey.ModSuper, Blazor_GetModState().HasFlag(Blazor_Keymod.KMOD_GUI));

                        MapKey(io, ImGuiKey.Tab, Blazor_Scancode.Blazor_SCANCODE_TAB, key, down);
                        MapKey(io, ImGuiKey.LeftArrow, Blazor_Scancode.Blazor_SCANCODE_LEFT, key, down);
                        MapKey(io, ImGuiKey.RightArrow, Blazor_Scancode.Blazor_SCANCODE_RIGHT, key, down);
                        MapKey(io, ImGuiKey.UpArrow, Blazor_Scancode.Blazor_SCANCODE_UP, key, down);
                        MapKey(io, ImGuiKey.DownArrow, Blazor_Scancode.Blazor_SCANCODE_DOWN, key, down);
                        MapKey(io, ImGuiKey.Enter, Blazor_Scancode.Blazor_SCANCODE_RETURN, key, down);
                        MapKey(io, ImGuiKey.Escape, Blazor_Scancode.Blazor_SCANCODE_ESCAPE, key, down);
                        MapKey(io, ImGuiKey.Backspace, Blazor_Scancode.Blazor_SCANCODE_BACKSPACE, key, down);
                        MapKey(io, ImGuiKey.Space, Blazor_Scancode.Blazor_SCANCODE_SPACE, key, down);
                        break;
                    }
            }
        }


        public void NewFrame()
        {
            var io = ImGui.GetIO();

            Blazor_GetWindowSize(_window, out int w, out int h);
            Blazor_GL_GetDrawableSize(_window, out int dw, out int dh);
            io.DisplaySize = new Vector2(w, h);
            io.DisplayFramebufferScale = new Vector2(w > 0 ? (float)dw / w : 1f, h > 0 ? (float)dh / h : 1f);

            ulong now = Blazor_GetPerformanceCounter();
            double freq = Blazor_GetPerformanceFrequency();
            io.DeltaTime = (float)((now - _ticksLast) / freq);
            if (io.DeltaTime <= 0) io.DeltaTime = 1f / 60f;
            _ticksLast = now;

            ImGui.NewFrame();
        }

        public void EndFrame()
        {
            ImGui.End();
            ImGui.PopStyleVar();
            Render();
            Blazor_RenderPresent(_renderer);
        }

        public void Render()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }


        private void CreateFontTexture()
        {
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.Fonts.GetTexDataAsRGBA32(out nint pixels, out int width, out int height, out _);
            uint fmt =
#if HAS_Blazor_PIXELFORMAT_RGBA32
        Blazor.Blazor_PIXELFORMAT_RGBA32;
#else
    Blazor_PIXELFORMAT_ABGR8888;
#endif
            // Fix enum: fully-qualify the type
            _fontTex = Blazor_CreateTexture(
                _renderer,
                fmt,
                (int)Blazor_TextureAccess.Blazor_TEXTUREACCESS_STATIC,
                width, height);

            Blazor_SetTextureBlendMode(_fontTex, Blazor_BlendMode.Blazor_BLENDMODE_BLEND);
            Blazor_UpdateTexture(_fontTex, nint.Zero, pixels, width * 4);

            io.Fonts.SetTexID(_fontTex);
            io.Fonts.ClearTexData();
        }


        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0) return;

            var fbScale = drawData.FramebufferScale;
            int fbWidth = (int)(drawData.DisplaySize.X * fbScale.X);
            int fbHeight = (int)(drawData.DisplaySize.Y * fbScale.Y);
            if (fbWidth <= 0 || fbHeight <= 0) return;

            var vp = new Blazor_Rect { x = 0, y = 0, w = fbWidth, h = fbHeight };
            Blazor_RenderSetViewport(_renderer, ref vp);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                // Fix: use CmdLists instead of CmdListsRange
                var cmdList = drawData.CmdLists[n];

                // Build Blazor_Vertex array
                var vertices = new Blazor_Vertex[cmdList.VtxBuffer.Size];
                for (int i = 0; i < cmdList.VtxBuffer.Size; i++)
                {
                    var v = cmdList.VtxBuffer[i];
                    vertices[i].position.x = v.pos.X;
                    vertices[i].position.y = v.pos.Y;
                    vertices[i].tex_coord.x = v.uv.X;
                    vertices[i].tex_coord.y = v.uv.Y;
                    vertices[i].color = Blazor_ColorFromUint(v.col);
                }

                // Build 32-bit indices (works for both 16/32 bit ImGui indices)
                var indices = new int[cmdList.IdxBuffer.Size];
                for (int i = 0; i < cmdList.IdxBuffer.Size; i++)
                    indices[i] = unchecked(cmdList.IdxBuffer[i]);

                int idxOffset = 0;
                for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
                {
                    var pcmd = cmdList.CmdBuffer[cmd_i];

                    var clip = new Blazor_Rect
                    {
                        x = (int)pcmd.ClipRect.X,
                        y = (int)pcmd.ClipRect.Y,
                        w = (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                        h = (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y)
                    };
                    Blazor_RenderSetClipRect(_renderer, ref clip);

                    nint tex = pcmd.TextureId != nint.Zero ? pcmd.TextureId : _fontTex;

                    // Fix: use Blazor_RenderGeometry (not *_Raw) to avoid pointer args
                    Blazor_RenderGeometry(
                        _renderer,
                        tex,
                        vertices, vertices.Length,
                        indices, (int)pcmd.ElemCount   // cast ElemCount -> int
                    );

                    idxOffset += (int)pcmd.ElemCount; // ensure int math
                }
            }

            Blazor_RenderSetClipRect(_renderer, nint.Zero);
        }

        private static Blazor_Color Blazor_ColorFromUint(uint packed)
        {
            // ImGui uses ABGR packing on little-endian; Blazor expects RGBA in Blazor_Color
            return new Blazor_Color
            {
                r = (byte)(packed >> 0),
                g = (byte)(packed >> 8),
                b = (byte)(packed >> 16),
                a = (byte)(packed >> 24)
            };
        }

        private static void MapKey(ImGuiIOPtr io, ImGuiKey imguiKey, Blazor_Scancode scan, Blazor_Scancode ev, bool down)
        {
            if (scan == ev) io.AddKeyEvent(imguiKey, down);
        }





        #region Textures

        private readonly Dictionary<nint, nint> _tex = new();
        private long _next = 1;
        public nint RegisterTexture(nint sdlTexture)
        {
            var imGuiId = Add(sdlTexture);
            // todo : set texture to the Gfx card it seems for ImGui. How to do this with Blazor?
            return imGuiId;
        }

        private nint Add(nint sdlTexture)
        {
            var id = new nint(_next++);
            _tex[id] = sdlTexture;
            return id;
        }
        public nint GetTexture(nint id) => _tex[id];


        #endregion


    }
}
