using System.Numerics;
using ImGuiNET;
using LingoEngine.SDL2.SDLL;
using static LingoEngine.SDL2.SDLL.SDL;




namespace LingoEngine.SDL2
{

    public sealed class ImGuiSdlBackend : IDisposable
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
            _ticksLast = SDL_GetPerformanceCounter();
            _inited = true;
        }

        public void Shutdown()
        {
            if (!_inited) return;
            var io = ImGui.GetIO();
            io.Fonts.SetTexID(nint.Zero);
            if (_fontTex != nint.Zero) { SDL_DestroyTexture(_fontTex); _fontTex = nint.Zero; }
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

        /// <summary>Call this for each SDL event.</summary>
        public void ProcessEvent(ref SDL_Event e)
        {
            var io = ImGui.GetIO();

            switch (e.type)
            {
                case SDL_EventType.SDL_MOUSEMOTION:
                    io.MousePos = new Vector2(e.motion.x, e.motion.y);
                    break;

                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    {
                        bool down = e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN;
                        if (e.button.button == SDL_BUTTON_LEFT) io.AddMouseButtonEvent(0, down);
                        if (e.button.button == SDL_BUTTON_RIGHT) io.AddMouseButtonEvent(1, down);
                        if (e.button.button == SDL_BUTTON_MIDDLE) io.AddMouseButtonEvent(2, down);
                        break;
                    }

                case SDL_EventType.SDL_MOUSEWHEEL:
                    io.AddMouseWheelEvent(e.wheel.preciseX, e.wheel.preciseY);
                    break;

                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    {
                        unsafe
                        {
                            fixed (byte* p = e.text.text) // single fixed is OK
                            {
                                int len = 0; while (len < 32 && p[len] != 0) len++;
                                if (len > 0)
                                {
                                    string s = System.Text.Encoding.UTF8.GetString(p, len);
                                    ImGuiNET.ImGui.GetIO().AddInputCharactersUTF8(s);
                                }
                            }
                        }
                        break;
                    }

                case SDL_EventType.SDL_KEYDOWN:
                case SDL_EventType.SDL_KEYUP:
                    {
                        bool down = e.type == SDL_EventType.SDL_KEYDOWN;
                        var key = (SDL_Scancode)e.key.keysym.scancode;

                        io.AddKeyEvent(ImGuiKey.ModCtrl, SDL_GetModState().HasFlag(SDL_Keymod.KMOD_CTRL));
                        io.AddKeyEvent(ImGuiKey.ModShift, SDL_GetModState().HasFlag(SDL_Keymod.KMOD_SHIFT));
                        io.AddKeyEvent(ImGuiKey.ModAlt, SDL_GetModState().HasFlag(SDL_Keymod.KMOD_ALT));
                        io.AddKeyEvent(ImGuiKey.ModSuper, SDL_GetModState().HasFlag(SDL_Keymod.KMOD_GUI));

                        MapKey(io, ImGuiKey.Tab, SDL_Scancode.SDL_SCANCODE_TAB, key, down);
                        MapKey(io, ImGuiKey.LeftArrow, SDL_Scancode.SDL_SCANCODE_LEFT, key, down);
                        MapKey(io, ImGuiKey.RightArrow, SDL_Scancode.SDL_SCANCODE_RIGHT, key, down);
                        MapKey(io, ImGuiKey.UpArrow, SDL_Scancode.SDL_SCANCODE_UP, key, down);
                        MapKey(io, ImGuiKey.DownArrow, SDL_Scancode.SDL_SCANCODE_DOWN, key, down);
                        MapKey(io, ImGuiKey.Enter, SDL_Scancode.SDL_SCANCODE_RETURN, key, down);
                        MapKey(io, ImGuiKey.Escape, SDL_Scancode.SDL_SCANCODE_ESCAPE, key, down);
                        MapKey(io, ImGuiKey.Backspace, SDL_Scancode.SDL_SCANCODE_BACKSPACE, key, down);
                        MapKey(io, ImGuiKey.Space, SDL_Scancode.SDL_SCANCODE_SPACE, key, down);
                        break;
                    }
            }
        }


        public void NewFrame()
        {
            var io = ImGui.GetIO();

            SDL_GetWindowSize(_window, out int w, out int h);
            SDL_GL_GetDrawableSize(_window, out int dw, out int dh);
            io.DisplaySize = new Vector2(w, h);
            io.DisplayFramebufferScale = new Vector2(w > 0 ? (float)dw / w : 1f, h > 0 ? (float)dh / h : 1f);

            ulong now = SDL_GetPerformanceCounter();
            double freq = SDL_GetPerformanceFrequency();
            io.DeltaTime = (float)((now - _ticksLast) / freq);
            if (io.DeltaTime <= 0) io.DeltaTime = 1f / 60f;
            _ticksLast = now;

            ImGui.NewFrame();
        }

        internal void EndFrame()
        {
            ImGui.End();
            ImGui.PopStyleVar();
            Render();
            SDL_RenderPresent(_renderer);
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

            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);
            uint fmt =
#if HAS_SDL_PIXELFORMAT_RGBA32
        SDL.SDL_PIXELFORMAT_RGBA32;
#else
    SDL.SDL_PIXELFORMAT_ABGR8888;
#endif
            // Fix enum: fully-qualify the type
            _fontTex = SDL.SDL_CreateTexture(
                _renderer,
                fmt,
                (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC,
                width, height);

            SDL.SDL_SetTextureBlendMode(_fontTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL.SDL_UpdateTexture(_fontTex, IntPtr.Zero, pixels, width * 4);

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

            var vp = new SDL.SDL_Rect { x = 0, y = 0, w = fbWidth, h = fbHeight };
            SDL.SDL_RenderSetViewport(_renderer, ref vp);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                // Fix: use CmdLists instead of CmdListsRange
                var cmdList = drawData.CmdLists[n];

                // Build SDL_Vertex array
                var vertices = new SDL.SDL_Vertex[cmdList.VtxBuffer.Size];
                for (int i = 0; i < cmdList.VtxBuffer.Size; i++)
                {
                    var v = cmdList.VtxBuffer[i];
                    vertices[i].position.x = v.pos.X;
                    vertices[i].position.y = v.pos.Y;
                    vertices[i].tex_coord.x = v.uv.X;
                    vertices[i].tex_coord.y = v.uv.Y;
                    vertices[i].color = SDL_ColorFromUint(v.col);
                }

                // Build 32-bit indices (works for both 16/32 bit ImGui indices)
                var indices = new int[cmdList.IdxBuffer.Size];
                for (int i = 0; i < cmdList.IdxBuffer.Size; i++)
                    indices[i] = unchecked((int)cmdList.IdxBuffer[i]);

                int idxOffset = 0;
                for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
                {
                    var pcmd = cmdList.CmdBuffer[cmd_i];

                    var clip = new SDL.SDL_Rect
                    {
                        x = (int)pcmd.ClipRect.X,
                        y = (int)pcmd.ClipRect.Y,
                        w = (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                        h = (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y)
                    };
                    SDL.SDL_RenderSetClipRect(_renderer, ref clip);

                    nint tex = pcmd.TextureId != nint.Zero ? pcmd.TextureId : _fontTex;

                    // Fix: use SDL_RenderGeometry (not *_Raw) to avoid pointer args
                    SDL.SDL_RenderGeometry(
                        _renderer,
                        tex,
                        vertices, vertices.Length,
                        indices, (int)pcmd.ElemCount   // cast ElemCount -> int
                    );

                    idxOffset += (int)pcmd.ElemCount; // ensure int math
                }
            }

            SDL.SDL_RenderSetClipRect(_renderer, IntPtr.Zero);
        }

        private static SDL.SDL_Color SDL_ColorFromUint(uint packed)
        {
            // ImGui uses ABGR packing on little-endian; SDL expects RGBA in SDL_Color
            return new SDL.SDL_Color
            {
                r = (byte)(packed >> 0),
                g = (byte)(packed >> 8),
                b = (byte)(packed >> 16),
                a = (byte)(packed >> 24)
            };
        }

        private static void MapKey(ImGuiIOPtr io, ImGuiKey imguiKey, SDL_Scancode scan, SDL_Scancode ev, bool down)
        {
            if (scan == ev) io.AddKeyEvent(imguiKey, down);
        }





        #region Textures

        private readonly Dictionary<IntPtr, IntPtr> _tex = new();
        private long _next = 1;
        public nint RegisterTexture(nint sdlTexture)
        {
            var imGuiId = Add(sdlTexture);
            // todo : set texture to the Gfx card it seems for ImGui. How to do this with SDL?
            return imGuiId;
        }

        private IntPtr Add(IntPtr sdlTexture)
        {
            var id = new IntPtr(_next++);
            _tex[id] = sdlTexture;
            return id;
        }
        public IntPtr GetTexture(IntPtr id) => _tex[id];


        #endregion


    }
}
