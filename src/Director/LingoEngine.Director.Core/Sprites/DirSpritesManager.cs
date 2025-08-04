using LingoEngine.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Stages.Commands;

namespace LingoEngine.Director.Core.Sprites
{
    public interface IDirSpritesManager
    {
        
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

    public class DirSpritesManager : IDirSpritesManager, IDisposable,
            ICommandHandler<ChangeSpriteRangeCommand>,
            ICommandHandler<AddSpriteCommand>,
            ICommandHandler<RemoveSpriteCommand>
    {
        private readonly IHistoryManager _historyManager;

        public IDirectorEventMediator Mediator { get; }
        public ILingoFrameworkFactory Factory { get; }
        public ILingoCommandManager CommandManager { get; }
        public DirSpritesSelection SpritesSelection { get; } = new();
        public DirScoreManager ScoreManager { get; }

        public ILingoKey Key { get; }

        public DirSpritesManager(IDirectorEventMediator mediator, ILingoFrameworkFactory factory, ILingoCommandManager commandManager, DirScoreManager scoreManager, IHistoryManager historyManager)
        {
            Mediator = mediator;
            Mediator.Subscribe(this);
            Factory = factory;
            CommandManager = commandManager;
            Key = factory.CreateKey();
            ScoreManager = scoreManager;
            _historyManager = historyManager;
            ScoreManager.SetSpritesManager(this);
        }
        public void Dispose()
        {
            Mediator.Unsubscribe(this);
            SpritesSelection.Clear();
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

        public void ChannelChanged(int spriteNumWithChannel)
        {

        }

        #region Commands

        public bool CanExecute(ChangeSpriteRangeCommand command) => true;
        public bool Handle(ChangeSpriteRangeCommand command)
        {
            if (command.EndChannel != command.Sprite.SpriteNum - 1)
                command.Movie.ChangeSpriteChannel(command.Sprite, command.EndChannel);
            command.Sprite.BeginFrame = command.EndBegin;
            command.Sprite.EndFrame = command.EndEnd;
            _historyManager.Push(command.ToUndo(() => ChannelChanged(command.Sprite.SpriteNumWithChannel)), command.ToRedo(() => ChannelChanged(command.Sprite.SpriteNumWithChannel)));
            ChannelChanged(command.Sprite.SpriteNumWithChannel);
            return true;
        }

        public bool CanExecute(AddSpriteCommand command) => true;
        public bool Handle(AddSpriteCommand command)
        {
            var sprite = command.Movie.AddSprite(command.Channel, command.BeginFrame, command.EndFrame, 0, 0,
                s => s.SetMember(command.Member));
            _historyManager.Push(command.ToUndo(sprite, () => ChannelChanged(sprite.SpriteNumWithChannel)), command.ToRedo(() => ChannelChanged(sprite.SpriteNumWithChannel)));
            //RefreshGrid(sprite.SpriteNumWithChannel);
            return true;
        }

        public bool CanExecute(RemoveSpriteCommand command) => true;
        public bool Handle(RemoveSpriteCommand command)
        {
            var movie = command.Movie;
            var sprite = command.Sprite;

            int channel = sprite.SpriteNum;
            int begin = sprite.BeginFrame;
            int end = sprite.EndFrame;
            var member = sprite.Member;
            string name = sprite.Name;
            float x = sprite.LocH;
            float y = sprite.LocV;

            sprite.RemoveMe();

            LingoSprite2D current = sprite;
            void refresh() => ChannelChanged(command.Sprite.SpriteNumWithChannel);

            Action undo = () =>
            {
                current = movie.AddSprite(channel, begin, end, x, y, s =>
                {
                    s.Name = name;
                    if (member != null)
                        s.SetMember(member);
                });
                refresh();
            };

            Action redo = () =>
            {
                current.RemoveMe();
                refresh();
            };

            _historyManager.Push(undo, redo);
            ChannelChanged(command.Sprite.SpriteNumWithChannel);
            return true;
        }


        #endregion
    }
}
