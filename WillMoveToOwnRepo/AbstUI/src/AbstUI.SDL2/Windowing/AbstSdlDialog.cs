using System.Runtime.InteropServices;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Inputs;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Windowing
{
    public partial class AbstSdlDialog : AbstSdlDialog<AbstDialog>, IFrameworkFor<AbstDialog>, IAbstFrameworkDialog<AbstDialog> 
    {
        public AbstSdlDialog(AbstSdlComponentFactory factory) : base(factory)
        {
        }
    }
    /// <summary>
    /// SDL2 implementation of a generic dialog window.
    /// </summary>
    public class AbstSdlDialog<TAbstDialog> : AbstSdlPanel, IAbstFrameworkDialog<TAbstDialog>, IFrameworkFor<TAbstDialog>, IHandleSdlEvent, IDisposable
        where TAbstDialog : IAbstDialog
    {
        private readonly AbstSdlComponentFactory _factory;
        private IAbstDialog _dialog = null!;
        private string _title = string.Empty;
        private bool _isPopup;
        private bool _borderless;
        private bool _centered;
        private ISdlFontLoadedByUser? _font;
        private SDL.SDL_Rect _closeRect;
        public const int TitleBarHeight = 24;

        #region Properties

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public new AColor BackgroundColor
        {
            get => base.BackgroundColor ?? AColors.White;
            set => base.BackgroundColor = value;
        }

        public bool IsOpen => Visibility;

        public bool IsPopup
        {
            get => _isPopup;
            set => _isPopup = value;
        }

        public bool Borderless
        {
            get => _borderless;
            set
            {
                if (_borderless == value) return;
                _borderless = value;
                Height = Borderless ? Height : Height + TitleBarHeight;
                ComponentContext.TargetHeight = (int)Height;
            }
        }

        public bool IsActiveWindow => Visibility;

        public IAbstMouse Mouse => _dialog.Mouse;

        public IAbstKey Key => _dialog.Key; 
        #endregion

        public event Action<bool>? OnWindowStateChanged;

        public AbstSdlDialog(AbstSdlComponentFactory factory) : base(factory)
        {
            _factory = factory;
            Visibility = false;
            ClipChildren = true;
        }


        public void Init(IAbstDialog instance)
        {
            _dialog = instance;
            instance.Init(this);
        }

        public void Popup()
        {
            ComponentContext.AlwaysOnTop = true;
            _factory.RootContext.ComponentContainer.Activate(ComponentContext);
            Visibility = true;
            OnWindowStateChanged?.Invoke(true);
        }

        public void PopupCentered()
        {
            _centered = true;
            UpdateCenterPosition();
            Popup();
        }

        private void UpdateCenterPosition()
        {
            var size = _factory.RootContext.GetWindowSize();
            X = (size.X - Width) / 2f;
            Y = (size.Y - Height) / 2f;
        }

        public void Hide()
        {
            Visibility = false;
            ComponentContext.AlwaysOnTop = false;
            _factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
            OnWindowStateChanged?.Invoke(false);
            _centered = false;
        }

        public void SetPositionAndSize(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            SetSize(width, height);
        }

        public APoint GetPosition() => new APoint(X, Y);

        public APoint GetSize() => new APoint(Width, Borderless?Height - TitleBarHeight : Height);

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = Borderless? height+ TitleBarHeight: height;
            ComponentContext.TargetHeight = (int)Height;
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return default;
            if (_centered)
                UpdateCenterPosition();
            var previousX = _xOffset;
            var previousY = _yOffset;
            _xOffset = (int)X;
            _yOffset = (int)(Borderless?Y: Y+TitleBarHeight);
            var tex = (nint)base.Render(context);
            _xOffset = previousX;
            _yOffset = previousY;
            int w = (int)Width;

            if (_font == null)
                _font = context.SdlFontManager.GetTyped(this, null, 14);

            if (!Borderless)
            {
                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, tex);
                // title bar
                SDL.SDL_SetRenderDrawColor(context.Renderer, 200, 200, 200, 255);
                var bar = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = TitleBarHeight };
                SDL.SDL_RenderFillRect(context.Renderer, ref bar);

                // draw title
                if (!string.IsNullOrEmpty(_title))
                {
                    SDL.SDL_Color col = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
                    nint surf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, _title, col);
                    if (surf != nint.Zero)
                    {
                        var s = Marshal.PtrToStructure<SDL.SDL_Surface>(surf);
                        nint t = SDL.SDL_CreateTextureFromSurface(context.Renderer, surf);
                        SDL.SDL_FreeSurface(surf);
                        var dst = new SDL.SDL_Rect
                        {
                            x = 4,
                            y = (TitleBarHeight - s.h) / 2,
                            w = s.w,
                            h = s.h
                        };
                        SDL.SDL_RenderCopy(context.Renderer, t, nint.Zero, ref dst);
                        SDL.SDL_DestroyTexture(t);
                    }
                }

                // close button
                int btnSize = TitleBarHeight - 4;
                _closeRect = new SDL.SDL_Rect { x = w - btnSize - 2, y = 2, w = btnSize, h = btnSize };
                SDL.SDL_SetRenderDrawColor(context.Renderer, 180, 0, 0, 255);
                SDL.SDL_RenderFillRect(context.Renderer, ref _closeRect);
                SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
                SDL.SDL_RenderDrawLine(context.Renderer, _closeRect.x + 3, _closeRect.y + 3, _closeRect.x + _closeRect.w - 3, _closeRect.y + _closeRect.h - 3);
                SDL.SDL_RenderDrawLine(context.Renderer, _closeRect.x + _closeRect.w - 3, _closeRect.y + 3, _closeRect.x + 3, _closeRect.y + _closeRect.h - 3);
                SDL.SDL_SetRenderTarget(context.Renderer, prev);
            }
            //base.Render(context);

            return tex;
        }

        public override void HandleEvent(AbstSDLEvent e)
        {
            if (!Visibility)
                return;
            e.OffsetX = -X;
            e.OffsetY = -Y;
            e.CalulateIsInside(Width,Height);
            // Console.WriteLine($"{e.MouseX}x{e.MouseY} {e.OffsetX}x{e.OffsetY} Inside={e.IsInside}\t {e.ComponentLeft}x{e.ComponentTop} \tSize={Width}x{Height}");
            if (!Borderless)
            {
                if (e.Event.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    var lx = e.ComponentLeft;
                    var ly = e.ComponentTop;
                    if (lx >= _closeRect.x && lx <= _closeRect.x + _closeRect.w &&
                        ly >= _closeRect.y && ly <= _closeRect.y + _closeRect.h)
                    {
                        Hide();
                        e.StopPropagation = true;
                        return;
                    }
                }
            }
            base.HandleEvent(e);
        }

        public override void Dispose()
        {
            _font?.Release();
            base.Dispose();
        }
    }
}
