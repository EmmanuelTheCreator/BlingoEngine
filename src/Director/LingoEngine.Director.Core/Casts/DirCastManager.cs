using LingoEngine.Casts;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Casts.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FilmLoops;
using LingoEngine.Sounds;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Casts;

public class DirCastManager : ICommandHandler<CreateFilmLoopCommand>
{
    private readonly IDirectorEventMediator _mediator;
    private readonly IHistoryManager _historyManager;

    public DirCastManager(IDirectorEventMediator mediator, IHistoryManager historyManager)
    {
        _mediator = mediator;
        _historyManager = historyManager;
    }

    public bool CanExecute(CreateFilmLoopCommand command) => true;

    public bool Handle(CreateFilmLoopCommand command)
    {
        var movie = command.Movie;
        var cast = (LingoCast)movie.CastLib.ActiveCast;
        var sprites = command.Sprites.ToList();

        var sprite2D = sprites.OfType<LingoSprite2D>().ToList();
        var soundSprites = sprites.OfType<LingoSpriteSound>().ToList();
        if (sprite2D.Count == 0 && soundSprites.Count == 0)
            return true;

        int minFrame = int.MaxValue;
        if (sprite2D.Count > 0) minFrame = Math.Min(minFrame, sprite2D.Min(s => s.BeginFrame));
        if (soundSprites.Count > 0) minFrame = Math.Min(minFrame, soundSprites.Min(s => s.BeginFrame));
        if (minFrame == int.MaxValue) minFrame = 1;

        int minChannel = sprite2D.Count > 0 ? sprite2D.Min(s => s.SpriteNum) : 1;

        var spriteEntries = sprite2D.Select(s => (
            Channel: s.SpriteNum - minChannel + 1,
            Begin: s.BeginFrame - minFrame + 1,
            End: s.EndFrame - minFrame + 1,
            Sprite: s)).ToList();

        var soundEntries = soundSprites
            .Where(s => s.Sound != null)
            .Select(s => (
                Channel: s.Channel,
                Start: s.BeginFrame - minFrame + 1,
                Sound: s.Sound!))
            .ToList();

        LingoFilmLoopMember member = cast.Add<LingoFilmLoopMember>(0, command.Name, fl =>
        {
            foreach (var e in spriteEntries)
                fl.AddSprite(movie.GetEnvironment(), e.Channel, e.Begin, e.End, e.Sprite);
            foreach (var e in soundEntries)
                fl.AddSound(e.Channel, e.Start, e.Sound);
        });

        _mediator.RaiseMemberSelected(member);

        int number = member.NumberInCast;
        LingoFilmLoopMember current = member;

        void Refresh() => _mediator.Raise(DirectorEventType.CastPropertiesChanged);

        Action undo = () =>
        {
            cast.Remove(current);
            Refresh();
        };

        Action redo = () =>
        {
            current = cast.Add<LingoFilmLoopMember>(number, command.Name, fl =>
            {
                foreach (var e in spriteEntries)
                    fl.AddSprite(movie.GetEnvironment(), e.Channel, e.Begin, e.End, e.Sprite);
                foreach (var e in soundEntries)
                    fl.AddSound(e.Channel, e.Start, e.Sound);
            });
            Refresh();
        };

        _historyManager.Push(undo, redo);
        Refresh();
        return true;
    }


}
