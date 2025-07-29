using LingoEngine.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Sprites;

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

        void SelectSprite(LingoSprite sprite);
        void DeselectSprite(LingoSprite sprite);
    }

    public class DirSpritesManager : IDirSpritesManager
    {
        public DirScoreGfxValues GfxValues { get; } = new();
        public IDirectorEventMediator Mediator { get; }
        public ILingoFrameworkFactory Factory { get; }
        public ILingoCommandManager CommandManager { get; }
        public DirSpritesSelection SpritesSelection { get; } = new();

        public ILingoKey Key { get; }

        public DirSpritesManager(IDirectorEventMediator mediator, ILingoFrameworkFactory factory, ILingoCommandManager commandManager)
        {
            Mediator = mediator;
            Factory = factory;
            CommandManager = commandManager;
            Key = factory.CreateKey();
        }

        public void SelectSprite(LingoSprite sprite)
        {
            if (SpritesSelection.AlreadySelected(sprite))
                return;
            if (!Key.ControlDown && !Key.ShiftDown)
                SpritesSelection.Clear();
            SpritesSelection.Add(sprite);
        }
        public void DeselectSprite(LingoSprite sprite)
        {
            SpritesSelection.Remove(sprite);
        }
    }
}
