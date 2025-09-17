using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Scripts;
using BlingoEngine.Sprites;

namespace BlingoEngine.Tempos;


/// <summary>
/// Lingo Tempo Sprite Manager interface.
/// </summary>
public interface IBlingoTempoSpriteManager : IBlingoSpriteManager<BlingoTempoSprite>
{
    public int Tempo { get; }
    BlingoTempoSprite Add(int frameNumber, Action<BlingoTempoSprite>? configure = null);
    BlingoTempoSprite Add(int frameNumber, BlingoTempoSpriteSettings settings);
    void ChangeTempo(BlingoTempoSprite blingoTempoSprite);

}
internal class BlingoTempoSpriteManager : BlingoSpriteManager<BlingoTempoSprite>, IBlingoTempoSpriteManager
{
    private int _tempo = 30;  // Default frame rate (FPS)

    public int Tempo
    {
        get => _tempo;
        private set
        {
            if (value > 0)
                _tempo = value;
        }
    }
    public BlingoTempoSpriteManager(BlingoMovie movie, BlingoMovieEnvironment environment) : base(BlingoTempoSprite.SpriteNumOffset, movie, environment)
    {
    }

    protected override BlingoTempoSprite OnCreateSprite(BlingoMovie movie, Action<BlingoTempoSprite> onRemove) => new BlingoTempoSprite(_environment, onRemove);

    public BlingoTempoSprite Add(int frameNumber, Action<BlingoTempoSprite>? configure = null)
    {
        var sprite = AddSprite(1, "TempoChange_" + frameNumber);
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        if (configure != null)
            configure(sprite);

        return sprite;
    }
    public BlingoTempoSprite Add(int frameNumber, BlingoTempoSpriteSettings settings)
    {
        var sprite = Add(frameNumber);
        sprite.SetSettings(settings);
        return sprite;
    }
    protected override BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member)
    {
        var sprite = Add(begin);

        return sprite;
    }
    public void ChangeTempo(int value)
    {
        if (value > 0 && value < 60)
            _tempo = value;
        _environment.Clock.FrameRate = _tempo;
    }
    public void ChangeTempo(BlingoTempoSprite blingoTempoSprite) => ChangeTempo(blingoTempoSprite.Tempo);


}

