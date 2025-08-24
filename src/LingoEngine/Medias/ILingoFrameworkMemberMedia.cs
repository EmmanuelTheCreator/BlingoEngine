using LingoEngine.Members;

namespace LingoEngine.Medias
{
    /// <summary>
    /// Framework specific implementation details for a media (video) member.
    /// </summary>
    public interface ILingoFrameworkMemberMedia : ILingoFrameworkMember
    {
        /// <summary>Start playback of the media stream.</summary>
        void Play();
        /// <summary>Pause playback of the media stream.</summary>
        void Pause();
        /// <summary>Stop playback of the media stream.</summary>
        void Stop();
        /// <summary>Seek to a position in milliseconds from the start of the stream.</summary>
        /// <param name="milliseconds">Position in milliseconds.</param>
        void Seek(int milliseconds);
        /// <summary>Total length of the media stream in milliseconds.</summary>
        int Duration { get; }
        /// <summary>Current position of the media stream in milliseconds.</summary>
        int CurrentTime { get; set; }
        /// <summary>Current playback status of the media stream.</summary>
        LingoMediaStatus MediaStatus { get; }
    }
}
