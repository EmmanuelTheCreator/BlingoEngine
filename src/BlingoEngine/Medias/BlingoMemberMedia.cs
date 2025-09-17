using BlingoEngine.Casts;
using BlingoEngine.Members;
using AbstUI.Primitives;

namespace BlingoEngine.Medias
{
    /// <summary>
    /// Base class for video media cast members.
    /// </summary>
    public class BlingoMemberMedia : BlingoMember
    {
        private readonly IBlingoFrameworkMemberMedia _frameworkMedia;

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
        public BlingoMediaStatus MediaStatus => _frameworkMedia.MediaStatus;

        public BlingoMemberMedia(IBlingoFrameworkMemberMedia frameworkMember, BlingoMemberType type, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, type, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkMedia = frameworkMember;
        }

        public T Framework<T>() where T : class, IBlingoFrameworkMemberMedia => (T)_frameworkMedia;

        public void Play() => _frameworkMedia.Play();
        public void Pause() => _frameworkMedia.Pause();
        public void Stop() => _frameworkMedia.Stop();
        public void Seek(int milliseconds) => _frameworkMedia.Seek(milliseconds);
    }
}

