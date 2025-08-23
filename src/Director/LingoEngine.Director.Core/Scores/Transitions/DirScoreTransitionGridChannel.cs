using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Movies;
using LingoEngine.Transitions;
using LingoEngine.Transitions.TransitionLibrary;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;

namespace LingoEngine.Director.Core.Scores.Transitions;

internal partial class DirScoreTransitionGridChannel : DirScoreChannel<ILingoSpriteTransitionManager, DirScoreTransitionSprite, LingoTransitionSprite>
{
    private readonly ILingoTransitionLibrary _transitionLibrary;
    private IAbstWindowDialogReference? _dialog;
    private readonly IList<KeyValuePair<string, string>> _transitionOptions;

    public DirScoreTransitionGridChannel(IDirScoreManager scoreManager, ILingoTransitionLibrary transitionLibrary)
        : base(LingoTransitionSprite.SpriteNumOffset + 1, scoreManager)
    {
        _transitionLibrary = transitionLibrary;
        _transitionOptions = _transitionLibrary.GetAll()
            .Select(t => new KeyValuePair<string, string>(t.Id.ToString(), t.Name))
            .ToList();
        IsSingleFrame = true;
    }

    protected override DirScoreTransitionSprite CreateUISprite(LingoTransitionSprite sprite, IDirSpritesManager spritesManager)
        => new DirScoreTransitionSprite(sprite, spritesManager);

    protected override ILingoSpriteTransitionManager GetManager(LingoMovie movie) => movie.Transitions;

    internal override void ShowCreateSpriteDialog(int frameNumber, Action<LingoSprite?> newSprite)
    {
        var first = _transitionOptions.First();
        var settings = new LingoTransitionFrameSettings
        {
            TransitionId = int.Parse(first.Key),
            TransitionName = first.Value
        };
        Action okAction = () =>
        {
            var sprite = _manager!.Add(frameNumber, settings);
            newSprite(sprite);
            _hasDirtySpriteList = true;
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }

    internal override void ShowSpriteDialog(LingoSprite sprite)
    {
        if (sprite is not LingoTransitionSprite transitionSprite) return;
        var settings = transitionSprite.GetSettings() ?? new LingoTransitionFrameSettings();
        Action okAction = () =>
        {
            transitionSprite.SetSettings(settings);
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }

    private void ShowDialog(LingoTransitionFrameSettings settings, Action okAction)
    {
        var panel = _scoreManager.Factory.CreatePanel("Panel Transition Sprite");
        panel.Width = 250;
        panel.Height = 150;

        panel.SetLabelAt("TransitionLabel", 10, 10, "Transition:", 11, 80, AbstUI.Texts.AbstTextAlignment.Right);
        panel.SetInputListAt(_transitionOptions, "TransitionList", 100, 10, 120, settings.TransitionId.ToString(), key =>
        {
            if (int.TryParse(key, out var id))
            {
                settings.TransitionId = id;
                var tr = _transitionLibrary.Get(id);
                settings.TransitionName = tr.Name;
            }
        });

        panel.AddPopupButtons(okAction, CloseDialog);

        _dialog = _showConfirmDialog?.Invoke("Frame Properties: Transition", (IAbstFrameworkPanel)panel.FrameworkObj);
    }

    private void CloseDialog()
    {
        if (_dialog == null) return;
        _dialog.Close();
        _dialog = null;
    }
}

