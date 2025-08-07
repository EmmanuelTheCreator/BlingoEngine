namespace LingoEngine.FilmLoops;

using LingoEngine.Sounds;
using LingoEngine.Sprites;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents the timeline data of a film loop. It describes layered sprites
/// and sounds with their own mini timeline.
/// </summary>
public class LingoFilmLoop
{
    /// <summary>
    /// Information about a sprite used in the film loop timeline.
    /// </summary>
    public record SpriteEntry(int Channel, int BeginFrame, int EndFrame, LingoSprite2DVirtual Sprite);

    /// <summary>
    /// Description of a sound placed on one of the two audio channels.
    /// </summary>
    public record SoundEntry(int Channel, int StartFrame, LingoMemberSound Sound);

    public List<SpriteEntry> SpriteEntries { get; } = new();
    public List<SoundEntry> SoundEntries { get; } = new();

    public int FrameCount { get; private set; }

    public void AddSprite(int channel, int beginFrame, int endFrame, LingoSprite2DVirtual sprite)
    {
        SpriteEntries.Add(new SpriteEntry(channel, beginFrame, endFrame, sprite));
        FrameCount = Math.Max(FrameCount, endFrame);
    }

    public void AddSound(int channel, int startFrame, LingoMemberSound sound)
    {
        SoundEntries.Add(new SoundEntry(channel, startFrame, sound));
    }
}
