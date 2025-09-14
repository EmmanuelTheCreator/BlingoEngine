using LingoEngine.Casts;
using LingoEngine.Members;
using AbstUI.Primitives;

namespace LingoEngine.Medias
{
    /// <summary>
    /// Base class for video media cast members.
    /// </summary>
    public class LingoMemberMedia : LingoMember
    {
        private readonly ILingoFrameworkMemberMedia _frameworkMedia;

        public int Duration => _frameworkMedia.Duration;
        public int CurrentTime
        {
            get => _frameworkMedia.CurrentTime;
            set
            {
                if (_frameworkMedia.CurrentTime == value)
                    return;
                _frameworkMedia.CurrentTime = value;
                OnPropertyChanged();
            }
        }
        public LingoMediaStatus MediaStatus => _frameworkMedia.MediaStatus;

        public LingoMemberMedia(ILingoFrameworkMemberMedia frameworkMember, LingoMemberType type, LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, type, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkMedia = frameworkMember;
        }

        public T Framework<T>() where T : class, ILingoFrameworkMemberMedia => (T)_frameworkMedia;

        public void Play() => _frameworkMedia.Play();
        public void Pause() => _frameworkMedia.Pause();
        public void Stop() => _frameworkMedia.Stop();
        public void Seek(int milliseconds) => _frameworkMedia.Seek(milliseconds);
    }
}
