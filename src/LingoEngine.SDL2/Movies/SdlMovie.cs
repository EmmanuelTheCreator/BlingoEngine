using LingoEngine.Movies;
using LingoEngine.SDL2.Sprites;
using LingoEngine.SDL2.Stages;
using LingoEngine.SDL2.Core;
using LingoEngine.Sprites;
using System.Linq;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Core;

namespace LingoEngine.SDL2.Movies;

public class SdlMovie : ILingoFrameworkMovie, IDisposable
{
    private readonly SdlStage _stage;
    private readonly Action<SdlMovie> _removeMethod;
    private readonly HashSet<SdlSprite> _drawnSprites = new();
    private readonly HashSet<SdlSprite> _allSprites = new();
    private readonly LingoSdlFactory _factory;
    private LingoMovie _movie;

    public SdlMovie(SdlStage stage, LingoSdlFactory factory, LingoMovie movie, Action<SdlMovie> removeMethod)
    {
        _stage = stage;
        _factory = factory;
        _movie = movie;
        _removeMethod = removeMethod;
        stage.ShowMovie(this);
    }

    internal void Show() => _stage.ShowMovie(this);
    internal void Hide() => _stage.HideMovie(this);

    public void UpdateStage()
    {
        var context = _factory.CreateRenderContext();
        Render(context);
    }

    public void Render(AbstSDLRenderContext context)
    {
        SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
        SDL.SDL_RenderClear(context.Renderer);
        _factory.ComponentContainer.Render(context);

        foreach (var s in _drawnSprites.OrderBy(s => s.ZIndex))
        {
            s.ComponentContext.RenderToTexture(context);
        }
    }

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite2D
    {
        var sprite = new SdlSprite(lingoSprite, _factory,
            s => _drawnSprites.Add(s),
            s => _drawnSprites.Remove(s),
            s => { _drawnSprites.Remove(s); _allSprites.Remove(s); });
        _allSprites.Add(sprite);
        sprite.ComponentContext.QueueRedraw(sprite);
    }

    public void RemoveMe()
    {
        _removeMethod(this);
    }

    APoint ILingoFrameworkMovie.GetGlobalMousePosition() => (0, 0);

    public void Dispose()
    {
        Hide();
        RemoveMe();
    }
}
