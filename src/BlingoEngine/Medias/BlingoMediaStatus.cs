namespace BlingoEngine.Medias
{
    /// <summary>
    /// Represents the playback state of a media stream.
    /// Mirrors the Lingo <c>mediaStatus</c> property.
    /// </summary>
    public enum BlingoMediaStatus
    {
        Closed,
        Connecting,
        Opened,
        Buffering,
        Playing,
        Seeking,
        Paused,
        Error
    }
}

