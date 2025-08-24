namespace LingoEngine.Medias
{
    /// <summary>
    /// Represents the playback state of a media stream.
    /// Mirrors the Lingo <c>mediaStatus</c> property.
    /// </summary>
    public enum LingoMediaStatus
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
