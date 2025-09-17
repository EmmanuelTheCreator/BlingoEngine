using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Components.Graphics;
using AbstUI.SDL2.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.SDL2.Stages;

internal class DirSdlStageWindow : AbstSdlWindow, IDirFrameworkStageWindow, IDisposable, IFrameworkFor<DirectorStageWindow>
{
    private readonly DirectorStageWindow _directorStageWindow;
    private readonly IDirectorEventSubscription _stageChangedSub;
    private readonly AbstSdlPanel _stagePanel;
    private readonly AbstSdlPanel _iconBarPanel;
    private readonly DirStageManager _stageManager;
    private readonly StageBoundingBoxesOverlay _boundingBoxes;
    private readonly StageMotionPathOverlay _motionPath;
    private readonly AbstSdlGfxCanvas _boundingBoxesCanvas;
    private readonly AbstSdlGfxCanvas _motionPathCanvas;
    private readonly AbstSdlGfxCanvas _selectionCanvas;
    private readonly BlingoStageMouse _stageMouse;
    private readonly BlingoKey _key;
    private readonly IAbstMouseSubscription _pointerClickSub;
    private readonly IBlingoPlayer _player;
    private const int _titleBarHeight = 24;

    public DirSdlStageWindow(IDirectorEventMediator mediator,
        IServiceProvider services,
        IBlingoPlayer player,
        DirectorStageWindow directorStageWindow,
        DirStageManager stageManager)
        : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        _directorStageWindow = directorStageWindow;
        _stageManager = stageManager;
        _player = player;
        Init(_directorStageWindow);

        var factory = (AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>();
        _stagePanel = factory.CreatePanel("StagePanel").Framework<AbstSdlPanel>();
        _stagePanel.X = 0;
        _stagePanel.Y = _titleBarHeight;
        _stagePanel.Width = _player.Stage.Width;
        _stagePanel.Height = _player.Stage.Height;
        _stagePanel.BackgroundColor = _player.Stage.BackgroundColor;
        AddItem(_stagePanel);

        _iconBarPanel = _directorStageWindow.IconBar.Panel.Framework<AbstSdlPanel>();
        _iconBarPanel.X = 0;
        _iconBarPanel.Y = _titleBarHeight + _stagePanel.Height;
        _iconBarPanel.Width = _stagePanel.Width;
        AddItem(_iconBarPanel);

        var lp = (BlingoPlayer)_player;
        _stageMouse = (BlingoStageMouse)lp.Mouse;
        _key = lp.Key;

        _boundingBoxes = new StageBoundingBoxesOverlay(lp.Factory, mediator);
        _boundingBoxes.SetInput(_stageMouse, _key);
        _boundingBoxesCanvas = _boundingBoxes.Canvas.Framework<AbstSdlGfxCanvas>();
        _boundingBoxesCanvas.Width = _stagePanel.Width;
        _boundingBoxesCanvas.Height = _stagePanel.Height;
        _boundingBoxesCanvas.X = 0;
        _boundingBoxesCanvas.Y = 0;
        _stagePanel.AddItem(_boundingBoxesCanvas);

        _motionPath = new StageMotionPathOverlay(lp.Factory);
        _motionPathCanvas = _motionPath.Canvas.Framework<AbstSdlGfxCanvas>();
        _motionPathCanvas.Width = _stagePanel.Width;
        _motionPathCanvas.Height = _stagePanel.Height;
        _motionPathCanvas.X = 0;
        _motionPathCanvas.Y = 0;
        _stagePanel.AddItem(_motionPathCanvas);

        var selCanvas = lp.Factory.CreateGfxCanvas("SelectionCanvas", (int)_player.Stage.Width,(int) _player.Stage.Height);
        _selectionCanvas = selCanvas.Framework<AbstSdlGfxCanvas>();
        _selectionCanvas.Visibility = false;
        _selectionCanvas.X = 0;
        _selectionCanvas.Y = 0;
        _stagePanel.AddItem(_selectionCanvas);

        Width = _stagePanel.Width;
        Height = _titleBarHeight + _stagePanel.Height + _iconBarPanel.Height;
        _directorStageWindow.ResizingContentFromFW(true, (int)Width, (int)Height - _titleBarHeight - (int)_iconBarPanel.Height);

        _stageChangedSub = mediator.Subscribe(DirectorEventType.StagePropertiesChanged, () =>
        {
            _stagePanel.Width = _player.Stage.Width;
            _stagePanel.Height = _player.Stage.Height;
            _stagePanel.BackgroundColor = _player.Stage.BackgroundColor;
            _iconBarPanel.Y = _titleBarHeight + _stagePanel.Height;
            _iconBarPanel.Width = _stagePanel.Width;
            _boundingBoxesCanvas.Width = _stagePanel.Width;
            _boundingBoxesCanvas.Height = _stagePanel.Height;
            _motionPathCanvas.Width = _stagePanel.Width;
            _motionPathCanvas.Height = _stagePanel.Height;
            _selectionCanvas.Width = _stagePanel.Width;
            _selectionCanvas.Height = _stagePanel.Height;
            Width = _stagePanel.Width;
            Height = _titleBarHeight + _stagePanel.Height + _iconBarPanel.Height;
            _directorStageWindow.ResizingContentFromFW(false, (int)Width, (int)Height - _titleBarHeight - (int)_iconBarPanel.Height);
            return true;
        });

        _pointerClickSub = _stageMouse.OnMouseDown(OnStageMouseDown);
        _stageManager.SelectionChanged += OnStageSelectionChanged;
        _stageManager.SpritesTransformed += OnStageSpritesTransformed;

        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnStageSelectionChanged()
    {
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnStageSpritesTransformed()
    {
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnStageMouseDown(BlingoMouseEvent e)
    {
        if (_directorStageWindow.SelectedTool != StageTool.Pointer)
            return;
        if (_player.ActiveMovie is not BlingoMovie movie)
            return;
        var sprite = movie.GetSpriteAtPoint(e.MouseH, e.MouseV, skipLockedSprites: true) as BlingoSprite2D;
        if (sprite != null)
        {
            _stageManager.HandlePointerClick(sprite, _key.ControlDown);
        }
        else if (!_key.ControlDown)
        {
            _stageManager.ClearSelection();
        }
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    public void UpdateBoundingBoxes()
    {
        if (_player.ActiveMovie is not BlingoMovie movie || movie.IsPlaying)
        {
            _boundingBoxes.Visible = false;
            return;
        }

        if (_stageManager.SelectedSprites.Count > 0)
        {
            _boundingBoxes.SetSprites(_stageManager.SelectedSprites);
            _boundingBoxes.Visible = true;
        }
        else
        {
            _boundingBoxes.Visible = false;
        }
    }

    public void UpdateSelectionBox()
    {
        if (_stageManager.SelectedSprites.Count == 0)
        {
            _selectionCanvas.Visibility = false;
            return;
        }
        var rect = _stageManager.ComputeSelectionRect();
        _selectionCanvas.Clear(AColors.Transparent);
        _selectionCanvas.DrawRect(rect, AColors.Yellow, false, 1);
        _selectionCanvas.Visibility = true;
    }

    private void UpdateMotionPath()
    {
        if (_player.ActiveMovie is not BlingoMovie movie || movie.IsPlaying)
        {
            _motionPath.Draw(null);
            return;
        }
        var sprite = _stageManager.PrimarySelectedSprite;
        var path = sprite != null ? _stageManager.GetMotionPath(sprite) : null;
        _motionPath.Draw(path);
    }

    public new void Dispose()
    {
        _stageChangedSub.Release();
        _pointerClickSub.Release();
        _stageManager.SelectionChanged -= OnStageSelectionChanged;
        _stageManager.SpritesTransformed -= OnStageSpritesTransformed;
        _boundingBoxes.Dispose();
        _motionPath.Dispose();
        base.Dispose();
    }
}


