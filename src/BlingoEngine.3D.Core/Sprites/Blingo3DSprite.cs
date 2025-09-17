using BlingoEngine.Movies;
using BlingoEngine.L3D.Core.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.L3D.Core.Sprites;

/// <summary>
/// Sprite type that exposes basic 3D functionality.
/// </summary>
public class Blingo3DSprite : BlingoSprite2D
{
    private readonly List<BlingoCamera> _cameras = new();

    public Blingo3DSprite(IBlingoMovieEnvironment environment, IBlingoSpritesPlayer spritesHolder) : base(environment, spritesHolder)
    {
    }

    /// <summary>Active camera for this sprite.</summary>
    public BlingoCamera? Camera { get; set; }

    /// <summary>Adds a camera to the sprite.</summary>
    public void AddCamera(BlingoCamera camera) => _cameras.Add(camera);

    /// <summary>Deletes a camera from the sprite.</summary>
    public void DeleteCamera(BlingoCamera camera) => _cameras.Remove(camera);

    /// <summary>Number of cameras associated with the sprite.</summary>
    public int CameraCount => _cameras.Count;
}

