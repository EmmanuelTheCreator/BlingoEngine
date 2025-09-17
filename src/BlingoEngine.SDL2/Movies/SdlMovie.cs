using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using BlingoEngine.Movies;
using BlingoEngine.SDL2.Core;
using BlingoEngine.SDL2.Sprites;
using BlingoEngine.SDL2.Stages;
using BlingoEngine.Sprites;

namespace BlingoEngine.SDL2.Movies;

public class SdlMovie : AbstSdlComponent,IBlingoFrameworkMovie, IDisposable
{
    private readonly SdlStage _stage;
    private readonly Action<SdlMovie> _removeMethod;
    private readonly HashSet<SdlSprite> _drawnSprites = new();
    private readonly HashSet<SdlSprite> _allSprites = new();
    private readonly BlingoSdlFactory _factory;
    private BlingoMovie _movie;

    public int CurrentFrame => _movie.CurrentFrame;

    public AMargin Margin { get; set; } = AMargin.Zero;

    public object FrameworkNode => this;

    public SdlMovie(SdlStage stage, BlingoSdlFactory factory, BlingoMovie movie, Action<SdlMovie> removeMethod)
        :base((AbstSdlComponentFactory)factory.ComponentFactory)
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
    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility) return nint.Zero;
        RenderSprites(context);
        return AbstSDLRenderResult.RequireRender();
    }

    private void RenderSprites(AbstSDLRenderContext context)
    {
        foreach (var s in _drawnSprites.OrderBy(s => s.ZIndex))
        {
            s.ComponentContext.RenderToTexture(context);
        }
    }

    internal void CreateSprite<T>(T blingoSprite) where T : BlingoSprite2D
    {
        var sprite = new SdlSprite(blingoSprite, _factory,
            s => _drawnSprites.Add(s),
            s => _drawnSprites.Remove(s),
            s => { _drawnSprites.Remove(s); _allSprites.Remove(s); });
        _allSprites.Add(sprite);
        sprite.ComponentContext.QueueRedraw(sprite);
        //((AbstSdlComponent)sprite.FrameworkNode).ComponentContext.SetParents(ComponentContext);
    }

    public void RemoveMe()
    {
        _removeMethod(this);
    }

    APoint IBlingoFrameworkMovie.GetGlobalMousePosition() => (0, 0);

    public override void Dispose()
    {
        base.Dispose();
        Hide();
        RemoveMe();
    }

   
}

