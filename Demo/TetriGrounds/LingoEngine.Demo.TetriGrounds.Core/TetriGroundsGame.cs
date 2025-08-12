using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core
{
    public class TetriGroundsGame
    {
        public const string MovieName = "Aaark Noid";
        private ILingoPlayer _lingoPlayer;
        private ILingoMovie _movie;


        public ILingoPlayer LingoPlayer => _lingoPlayer;

        public TetriGroundsGame(ILingoPlayer player)
        {
            _lingoPlayer = (LingoPlayer)player;
        }
       
        public void Play()
        {
            _movie.Play();
        }
       



    }
}
