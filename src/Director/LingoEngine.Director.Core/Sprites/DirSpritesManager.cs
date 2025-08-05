using LingoEngine.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Tempos;
using LingoEngine.ColorPalettes;
using LingoEngine.Scripts;
using LingoEngine.Sounds;
using LingoEngine.Transitions;
using LingoEngine.Members;
using LingoEngine.Bitmaps;

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
            var sprites = SpritesSelection.Sprites.ToArray();
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
            var sprite = CreateSprite(command.Movie, command.Member, command.SpriteNumWithChannel, command.BeginFrame, command.EndFrame);
            if (sprite == null) return true;
            _historyManager.Push(command.ToUndo(sprite, () => ChannelChanged(sprite.SpriteNumWithChannel)), command.ToRedo(() => ChannelChanged(sprite.SpriteNumWithChannel)));
            ChannelChanged(sprite.SpriteNumWithChannel);
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
            LingoMemberSound? memberSound = null;
            if (sprite is LingoSpriteSound sound)
                memberSound = sound.Sound;
          
            Action<LingoSprite> action = sprite.GetCloneAction();

            sprite.RemoveMe();

            LingoSprite current = sprite;
            void refresh() => ChannelChanged(command.Sprite.SpriteNumWithChannel);

            Action undo = () =>
            {
                current = CreateSprite(movie, sprite, channel, begin,end, memberSound, action, current);
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

        private static LingoSprite CreateSprite(LingoMovie movie, LingoSprite sprite, int spriteNumWithChannel, int begin,int end, LingoMemberSound? memberSound, Action<LingoSprite> action, LingoSprite current)
        {
            switch (sprite)
            {
                case LingoSprite2D:
                    {
                        var sprite2D = movie.Sprite2DManager.Add(spriteNumWithChannel - movie.Sprite2DManager.SpriteNumChannelOffset, begin,end);
                        action(sprite2D);
                        current = sprite2D;
                        break;
                    }
                case LingoTempoSprite:
                    {
                        var tempoSprite1 = movie.Tempos.Add(begin);
                        action(tempoSprite1);
                        current = tempoSprite1;
                        break;
                    }
                case LingoColorPaletteSprite:
                    {
                        var colorSprite = movie.ColorPalettes.Add(begin);
                        action(colorSprite);
                        current = colorSprite;
                        break;
                    }
                case LingoFrameScriptSprite:
                    {
                        var scriptSprite = movie.FrameScripts.Add(begin);
                        action(scriptSprite);
                        current = scriptSprite;
                        break;
                    }
                case LingoTransitionSprite:
                    {
                        var transitionSprite = movie.Transitions.Add(begin);
                        action(transitionSprite);
                        current = transitionSprite;
                        break;
                    }
                case LingoSpriteSound:
                    {
                        if (memberSound != null)
                        {
                            var soundSprite = movie.Audio.Add(spriteNumWithChannel - movie.Audio.SpriteNumChannelOffset, begin, memberSound);
                            action(soundSprite);
                            current = soundSprite;
                        }
                        break;
                    }
            }

            return current;
        } 
        public static LingoSprite? CreateSprite(LingoMovie movie, ILingoMember member, int spriteNumWithChannel, int begin, int end)
        {
            switch (member)
            {
                case LingoMemberBitmap:return movie.Sprite2DManager.Add(spriteNumWithChannel - movie.Sprite2DManager.SpriteNumChannelOffset, begin,end, c => c.SetMember(member));
                //case LingoTempoSprite:
                case LingoColorPaletteMember memberPalette:
                    {
                        var colorSprite = movie.ColorPalettes.Add(begin);
                        colorSprite.SetMember(memberPalette);
                        return colorSprite;
                    }
                case LingoMemberScript lingoMemberScript:return movie.FrameScripts.Add(begin, c => c.SetMember(lingoMemberScript));
                case LingoTransitionMember transitionMember:return movie.Transitions.Add(begin, transitionMember);
                case LingoMemberSound memberSound:return movie.Audio.Add(spriteNumWithChannel - movie.Audio.SpriteNumChannelOffset, begin, memberSound);
            }

            return null;
        }


        #endregion
    }
}
