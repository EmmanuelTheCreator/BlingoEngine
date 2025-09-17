using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Shapes;
using BlingoEngine.Texts;
using BlingoEngine.Bitmaps;
using BlingoEngine.FilmLoops;
using BlingoEngine.Medias;

namespace BlingoEngine.Members
{
    /// <summary>
    /// Lingo Member Factory interface.
    /// </summary>
    public interface IBlingoMemberFactory
    {
        T Member<T>(int numberInCast = 0, string name = "") where T : BlingoMember;
        BlingoMemberBitmap Bitmap(int numberInCast = 0, string name = "");
        BlingoMemberSound Sound(int numberInCast = 0, string name = "");
        BlingoFilmLoopMember FilmLoop(int numberInCast = 0, string name = "");
        BlingoMemberShape Shape(int numberInCast = 0, string name = "");
        BlingoMemberText Text(int numberInCast = 0, string name = "");
        BlingoMemberQuickTimeMedia QuickTimeMedia(int numberInCast = 0, string name = "");
        BlingoMemberRealMedia RealMedia(int numberInCast = 0, string name = "");
    }

    internal class BlingoMemberFactory : IBlingoMemberFactory
    {
        private readonly IBlingoFrameworkFactory _frameworkFactory;
        private readonly IBlingoMovieEnvironment _environment;

        public BlingoMemberFactory(IBlingoFrameworkFactory frameworkFactory, IBlingoMovieEnvironment environment)
        {
            _frameworkFactory = frameworkFactory;
            _environment = environment;
        }
        public T Member<T>(int numberInCast = 0, string name = "") where T : BlingoMember => _frameworkFactory.CreateMember<T>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);

        public BlingoMemberBitmap Bitmap(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberBitmap(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public BlingoMemberSound Sound(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberSound(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public BlingoFilmLoopMember FilmLoop(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberFilmLoop(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public BlingoMemberShape Shape(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberShape(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public BlingoMemberText Text(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberText(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public BlingoMemberQuickTimeMedia QuickTimeMedia(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMember<BlingoMemberQuickTimeMedia>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public BlingoMemberRealMedia RealMedia(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMember<BlingoMemberRealMedia>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
    }
}

