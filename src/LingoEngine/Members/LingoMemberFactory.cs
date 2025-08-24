using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Medias;

namespace LingoEngine.Members
{
    /// <summary>
    /// Lingo Member Factory interface.
    /// </summary>
    public interface ILingoMemberFactory
    {
        T Member<T>(int numberInCast = 0, string name = "") where T : LingoMember;
        LingoMemberBitmap Bitmap(int numberInCast = 0, string name = "");
        LingoMemberSound Sound(int numberInCast = 0, string name = "");
        LingoFilmLoopMember FilmLoop(int numberInCast = 0, string name = "");
        LingoMemberShape Shape(int numberInCast = 0, string name = "");
        LingoMemberText Text(int numberInCast = 0, string name = "");
        LingoMemberQuickTimeMedia QuickTimeMedia(int numberInCast = 0, string name = "");
        LingoMemberRealMedia RealMedia(int numberInCast = 0, string name = "");
    }

    internal class LingoMemberFactory : ILingoMemberFactory
    {
        private readonly ILingoFrameworkFactory _frameworkFactory;
        private readonly ILingoMovieEnvironment _environment;

        public LingoMemberFactory(ILingoFrameworkFactory frameworkFactory, ILingoMovieEnvironment environment)
        {
            _frameworkFactory = frameworkFactory;
            _environment = environment;
        }
        public T Member<T>(int numberInCast = 0, string name = "") where T : LingoMember => _frameworkFactory.CreateMember<T>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);

        public LingoMemberBitmap Bitmap(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberBitmap(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberSound Sound(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberSound(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoFilmLoopMember FilmLoop(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberFilmLoop(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberShape Shape(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberShape(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberText Text(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberText(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberQuickTimeMedia QuickTimeMedia(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMember<LingoMemberQuickTimeMedia>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberRealMedia RealMedia(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMember<LingoMemberRealMedia>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
    }
}
