using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.SDL2.Core;
using LingoEngine.SDL2.Movies;
using LingoEngine.Stages;

namespace LingoEngine.SDL2.Stages;

public class SdlStage : ILingoFrameworkStage, IDisposable
{
    private bool _isTransitioning;
    private readonly LingoClock _clock;
    private readonly LingoSdlFactory _factory;
    private readonly LingoSdlRootContext _rootContext;
    private LingoStage _stage = null!;
    private readonly HashSet<SdlMovie> _movies = new();
    private SdlMovie? _activeMovie;
    private IAbstTexture2D? _transitionFrame;
    private ARect _transitionRect;
    /// <summary>
    /// screenshot of the previous frame
    /// </summary>
    private IAbstTexture2D? _startFrame;
    public float Scale { get; set; }

    public SdlStage(LingoSdlRootContext rootContext, LingoClock clock, LingoSdlFactory factory)
    {
        _rootContext = rootContext;
        _clock = clock;
        _factory = factory;
    }

    internal LingoSdlRootContext RootContext => _rootContext;

    public LingoStage LingoStage => _stage;

    internal void Init(LingoStage stage)
    {
        _stage = stage;

    }

    internal void ShowMovie(SdlMovie movie)
    {
        _movies.Add(movie);
    }
    internal void HideMovie(SdlMovie movie)
    {
        _movies.Remove(movie);
    }

    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null) { _activeMovie = null; return; }
        var movie = lingoMovie.Framework<SdlMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    public void Dispose() { _movies.Clear(); }

    public void ApplyPropertyChanges()
    {

    }
    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured, bool excludeTransitionOverlay = true)
    {
        onCaptured(GetScreenshot());
    }
    public IAbstTexture2D GetScreenshot()
    {
        var ctx = _factory.CreateRenderContext();
        int w = _stage.Width, h = _stage.Height;

        nint tex = RenderToTexture(ctx.Renderer, w, h, () =>
        {
            SDL.SDL_SetRenderDrawColor(ctx.Renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(ctx.Renderer);
            _factory.ComponentContainer.Render(ctx);
            _activeMovie!.RenderSprites(ctx);
        });

        var texture = new SdlTexture2D(tex, w, h, "StageShot_"+_activeMovie!.CurrentFrame, ctx.Renderer); // caller owns
        //texture.DebugWriteToDisk(ctx.Renderer);
        return texture;
    }


    public void ShowTransition(IAbstTexture2D startTexture)
    {
        _startFrame?.Dispose();
        _startFrame = startTexture; // already a screenshot from GetScreenshot()
        _isTransitioning = true;
    }


    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
    {
        _transitionFrame = texture;
        _transitionRect = targetRect;
        Render();
    }


    public void HideTransition()
    {
        _isTransitioning = false;
        _transitionFrame?.Dispose();
        _transitionFrame = null;
        _startFrame?.Dispose();
        _startFrame = null;
    }


    internal void Render()
    {
        var context = _factory.CreateRenderContext();
        var w = _stage.Width;
        var h = _stage.Height;

        nint frameTex = RenderToTexture(context.Renderer, w, h, () =>
        {
            SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(context.Renderer);

            if (_isTransitioning && _startFrame is SdlTexture2D s0)
            {
                // draw previous frame (full)
                var full = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderCopy(context.Renderer, s0.Handle, IntPtr.Zero, ref full);

                // overlay current/next transition chunk in its rect (if provided)
                if (_transitionFrame is SdlTexture2D s1)
                {
                    var dst = new SDL.SDL_Rect
                    {
                        x = (int)_transitionRect.Left,
                        y = (int)_transitionRect.Top,
                        w = (int)_transitionRect.Width,
                        h = (int)_transitionRect.Height
                    };
                    SDL.SDL_RenderCopy(context.Renderer, s1.Handle, IntPtr.Zero, ref dst);
                }
            }
            else
            {
                // normal render
                _factory.ComponentContainer.Render(context);
                _activeMovie!.RenderSprites(context);
            }
        });

        SDL.SDL_SetRenderTarget(context.Renderer, nint.Zero);
        SDL.SDL_RenderCopy(context.Renderer, frameTex, IntPtr.Zero, IntPtr.Zero);
        SDL.SDL_RenderPresent(context.Renderer);

        SDL.SDL_DestroyTexture(frameTex);
    }

    /// Renders with 'draw' into a target texture and returns it.
    static nint RenderToTexture(nint renderer, int width, int height, Action draw)
    {
        nint tex = SDL.SDL_CreateTexture(renderer,
            SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            width, height);
        if (tex == nint.Zero) throw new Exception(SDL.SDL_GetError());
        SDL.SDL_SetTextureBlendMode(tex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

        nint prev = SDL.SDL_GetRenderTarget(renderer);
        SDL.SDL_SetRenderTarget(renderer, tex);

        draw(); // your scene rendering

        SDL.SDL_SetRenderTarget(renderer, prev);
        return tex;
    }

   
}
