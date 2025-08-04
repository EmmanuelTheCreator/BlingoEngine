using LingoEngine.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Stages.Commands;
using System.Linq;

namespace LingoEngine.Director.Core.Sprites
{
    public interface IDirSpritesManager
    {
        DirScoreGfxValues GfxValues { get; }
        IDirectorEventMediator Mediator { get; }
        ILingoFrameworkFactory Factory { get; }
        ILingoCommandManager CommandManager { get; }
        DirSpritesSelection SpritesSelection { get; }
        ILingoKey Key { get; }
        DirScoreManager ScoreManager { get; }

        void SelectSprite(LingoSprite sprite);
        void DeselectSprite(LingoSprite sprite);
        void DeleteSelected(LingoMovie movie);
    }

    public class DirSpritesManager : IDirSpritesManager
    {
        public DirScoreGfxValues GfxValues { get; } = new();
        public IDirectorEventMediator Mediator { get; }
        public ILingoFrameworkFactory Factory { get; }
        public ILingoCommandManager CommandManager { get; }
        public DirSpritesSelection SpritesSelection { get; } = new();
        public DirScoreManager ScoreManager { get; }

        public ILingoKey Key { get; }

        public DirSpritesManager(IDirectorEventMediator mediator, ILingoFrameworkFactory factory, ILingoCommandManager commandManager, DirScoreManager scoreManager)
        {
            Mediator = mediator;
            Factory = factory;
            CommandManager = commandManager;
            Key = factory.CreateKey();
            ScoreManager = scoreManager;
            ScoreManager.SetSpritesManager(this);
        }

        public void SelectSprite(LingoSprite sprite)
        {
            if (SpritesSelection.AlreadySelected(sprite))
                return;
            if (!Key.ControlDown && !Key.ShiftDown)
                SpritesSelection.Clear();
            SpritesSelection.Add(sprite);
            ScoreManager.SelectSprite(sprite);
            Mediator.RaiseSpriteSelected(sprite);
        }
        public void DeselectSprite(LingoSprite sprite)
        {
            SpritesSelection.Remove(sprite);
            ScoreManager.DeselectSprite(sprite);
        }

        public void DeleteSelected(LingoMovie movie)
        {
            var sprites = SpritesSelection.Sprites.OfType<LingoSprite2D>().ToArray();
            foreach (var s in sprites)
                CommandManager.Handle(new RemoveSpriteCommand(movie, s));
        }
    }
}
