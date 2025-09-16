using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using LingoEngine.Movies;
using LingoEngine.SDL2.Core;
using LingoEngine.SDL2.Sprites;
using LingoEngine.SDL2.Stages;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Movies;

public class SdlMovie : AbstSdlComponent,ILingoFrameworkMovie, IDisposable
{
    private readonly SdlStage _stage;
    private readonly Action<SdlMovie> _removeMethod;
    private readonly HashSet<SdlSprite> _drawnSprites = new();
    private readonly HashSet<SdlSprite> _allSprites = new();
    private readonly LingoSdlFactory _factory;
    private LingoMovie _movie;

    public int CurrentFrame => _movie.CurrentFrame;

    public AMargin Margin { get; set; } = AMargin.Zero;

    public object FrameworkNode => this;

    public SdlMovie(SdlStage stage, LingoSdlFactory factory, LingoMovie movie, Action<SdlMovie> removeMethod)
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

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite2D
    {
        var sprite = new SdlSprite(lingoSprite, _factory,
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

    APoint ILingoFrameworkMovie.GetGlobalMousePosition() => (0, 0);

    public override void Dispose()
    {
        base.Dispose();
        Hide();
        RemoveMe();
    }

   
}
