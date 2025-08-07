using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Core;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Tools;

namespace LingoEngine.Director.Core.Stages;

public interface IDirStageManager
{
    IReadOnlyList<LingoSprite2D> SelectedSprites { get; }
    LingoSprite2D? PrimarySelectedSprite { get; }
    event Action SelectionChanged;
    event Action SpritesTransformed;
    bool RecordKeyframes { get; set; }
    void HandlePointerClick(LingoSprite2D? sprite, bool ctrlHeld);
    void ClearSelection();
    LingoPoint ComputeSelectionCenter();
    LingoRect ComputeSelectionRect();
    void BeginMove(LingoPoint start);
    void UpdateMove(LingoPoint current);
    void EndMove(LingoPoint end);
    void BeginRotate(LingoPoint start);
    void UpdateRotate(LingoPoint current);
    void EndRotate(LingoPoint end);
    void ChangeBackgroundColor(LingoColor color);
}

public class DirStageManager : IDirStageManager, IDisposable, ICommandHandler<StageChangeBackgroundColorCommand>
{
    private readonly IDirSpritesManager _spritesManager;
    private readonly ILingoCommandManager _commandManager;
    private readonly ILingoPlayer _player;
    private readonly IHistoryManager _historyManager;
    private readonly IDirectorEventMediator _mediator;
    private readonly List<LingoSprite2D> _selected = new();
    private LingoSprite2D? _primary;

    private LingoPoint? _dragStart;
    private Dictionary<LingoSprite2D, LingoPoint>? _initialPositions;
    private Dictionary<LingoSprite2D, float>? _initialRotations;
    private bool _rotating;

    public DirStageManager(IDirSpritesManager spritesManager, ILingoCommandManager commandManager, ILingoPlayer player, IHistoryManager historyManager, IDirectorEventMediator mediator)
    {
        _spritesManager = spritesManager;
        _commandManager = commandManager;
        _player = player;
        _historyManager = historyManager;
        _mediator = mediator;
        _spritesManager.SpritesSelection.SelectionChanged += SyncSelection;
        SyncSelection();
    }

    public IReadOnlyList<LingoSprite2D> SelectedSprites => _selected;
    public LingoSprite2D? PrimarySelectedSprite => _primary;

    public event Action? SelectionChanged;
    public event Action? SpritesTransformed;

    private bool _recordKeyframes;
    public bool RecordKeyframes
    {
        get => _recordKeyframes;
        set => _recordKeyframes = value;
    }

    private void SyncSelection()
    {
        _selected.Clear();
        foreach (var s in _spritesManager.SpritesSelection.Sprites.OfType<LingoSprite2D>())
            _selected.Add(s);
        if (_primary == null || !_selected.Contains(_primary))
            _primary = _selected.FirstOrDefault();
        SelectionChanged?.Invoke();
    }

    public void HandlePointerClick(LingoSprite2D? sprite, bool ctrlHeld)
    {
        if (sprite == null) return;
        if (ctrlHeld)
        {
            if (_selected.Contains(sprite))
                _spritesManager.DeselectSprite(sprite);
            else
                _spritesManager.SelectSprite(sprite);
        }
        else
        {
            _spritesManager.SpritesSelection.Clear();
            _spritesManager.SelectSprite(sprite);
        }
    }

    public void ClearSelection()
    {
        if (_selected.Count == 0) return;
        _spritesManager.SpritesSelection.Clear();
    }

    public LingoPoint ComputeSelectionCenter()
    {
        if (_selected.Count == 0)
            return new LingoPoint(0, 0);
        float left = _selected.Min(s => s.Rect.Left);
        float top = _selected.Min(s => s.Rect.Top);
        float right = _selected.Max(s => s.Rect.Right);
        float bottom = _selected.Max(s => s.Rect.Bottom);
        return new LingoPoint((left + right) / 2f, (top + bottom) / 2f);
    }

    public LingoRect ComputeSelectionRect()
    {
        if (_selected.Count == 0)
            return new LingoRect();
        float left = _selected.Min(s => s.Rect.Left);
        float top = _selected.Min(s => s.Rect.Top);
        float right = _selected.Max(s => s.Rect.Right);
        float bottom = _selected.Max(s => s.Rect.Bottom);
        return new LingoRect(left, top, right, bottom);
    }

    public void BeginMove(LingoPoint start)
    {
        if (_selected.Count == 0) return;
        _dragStart = start;
        _initialPositions = _selected.ToDictionary(s => s, s => new LingoPoint(s.LocH, s.LocV));
        if (RecordKeyframes && _player is LingoPlayer lp)
            foreach (var s in _selected)
                lp.Stage.AddKeyFrame(s);
    }

    public void UpdateMove(LingoPoint current)
    {
        if (_dragStart == null || _initialPositions == null) return;
        float dx = current.X - _dragStart.Value.X;
        float dy = current.Y - _dragStart.Value.Y;
        foreach (var s in _selected)
        {
            var start = _initialPositions[s];
            s.LocH = start.X + dx;
            s.LocV = start.Y + dy;
            if (RecordKeyframes && _player is LingoPlayer lp)
                lp.Stage.UpdateKeyFrame(s);
        }
        SpritesTransformed?.Invoke();
    }

    public void EndMove(LingoPoint end)
    {
        if (_dragStart == null || _initialPositions == null) return;
        var endPos = _selected.ToDictionary(s => s, s => new LingoPoint(s.LocH, s.LocV));
        _commandManager.Handle(new MoveSpritesCommand(_initialPositions, endPos));
        if (RecordKeyframes && _player is LingoPlayer lp)
            foreach (var s in _selected)
                lp.Stage.UpdateKeyFrame(s);
        _dragStart = null;
        _initialPositions = null;
        SpritesTransformed?.Invoke();
    }

    public void BeginRotate(LingoPoint start)
    {
        if (_selected.Count == 0) return;
        _dragStart = start;
        _initialRotations = _selected.ToDictionary(s => s, s => s.Rotation);
        _rotating = true;
        if (RecordKeyframes && _player is LingoPlayer lp)
            foreach (var s in _selected)
                lp.Stage.AddKeyFrame(s);
    }

    public void UpdateRotate(LingoPoint current)
    {
        if (!_rotating || _dragStart == null || _initialRotations == null) return;
        var center = ComputeSelectionCenter();
        float startAngle = MathF.Atan2(_dragStart.Value.Y - center.Y, _dragStart.Value.X - center.X);
        float currentAngle = MathF.Atan2(current.Y - center.Y, current.X - center.X);
        float delta = (currentAngle - startAngle) * (180f / MathF.PI);
        foreach (var s in _selected)
        {
            s.Rotation = _initialRotations[s] + delta;
            if (RecordKeyframes && _player is LingoPlayer lp)
                lp.Stage.UpdateKeyFrame(s);
        }
        SpritesTransformed?.Invoke();
    }

    public void EndRotate(LingoPoint end)
    {
        if (!_rotating || _initialRotations == null) return;
        var endRot = _selected.ToDictionary(s => s, s => s.Rotation);
        _commandManager.Handle(new RotateSpritesCommand(_initialRotations, endRot));
        if (RecordKeyframes && _player is LingoPlayer lp)
            foreach (var s in _selected)
                lp.Stage.UpdateKeyFrame(s);
        _rotating = false;
        _dragStart = null;
        _initialRotations = null;
        SpritesTransformed?.Invoke();
    }

    public void ChangeBackgroundColor(LingoColor color)
    {
        if (_player is not LingoPlayer lp) return;
        var current = lp.Stage.BackgroundColor;
        if (current.Equals(color)) return;
        _commandManager.Handle(new StageChangeBackgroundColorCommand(current, color));
    }

    public bool CanExecute(StageChangeBackgroundColorCommand command) => true;

    public bool Handle(StageChangeBackgroundColorCommand command)
    {
        if (_player is not LingoPlayer lp)
            return false;
        lp.Stage.BackgroundColor = command.NewColor;
        _historyManager.Push(
            () =>
            {
                lp.Stage.BackgroundColor = command.OldColor;
                _mediator.Raise(DirectorEventType.StagePropertiesChanged);
            },
            () =>
            {
                lp.Stage.BackgroundColor = command.NewColor;
                _mediator.Raise(DirectorEventType.StagePropertiesChanged);
            });
        _mediator.Raise(DirectorEventType.StagePropertiesChanged);
        return true;
    }

    public void Dispose()
    {
        _spritesManager.SpritesSelection.SelectionChanged -= SyncSelection;
    }
}

