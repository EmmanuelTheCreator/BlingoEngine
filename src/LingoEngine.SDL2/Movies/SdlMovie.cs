using LingoEngine.Movies;
using LingoEngine.SDL2.Sprites;
using LingoEngine.SDL2.Stages;
using LingoEngine.SDL2.Core;
using LingoEngine.Sprites;
using AbstUI.Primitives;
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

    public int CurrentFrame => _movie.CurrentFrame;

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
        _stage.Render();
    }

    internal void RenderSprites(AbstSDLRenderContext context)
    {
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
