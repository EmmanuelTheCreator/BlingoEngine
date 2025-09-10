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
    private readonly List<LingoBaseTransition> _transitions;

    public DirScoreTransitionGridChannel(IDirScoreManager scoreManager, ILingoTransitionLibrary transitionLibrary)
        : base(LingoTransitionSprite.SpriteNumOffset + 1, scoreManager)
    {
        _transitionLibrary = transitionLibrary;
        _transitions = _transitionLibrary.GetAll().ToList();
        IsSingleFrame = true;
    }

    protected override DirScoreTransitionSprite CreateUISprite(LingoTransitionSprite sprite, IDirSpritesManager spritesManager)
        => new DirScoreTransitionSprite(sprite, spritesManager);

    protected override ILingoSpriteTransitionManager GetManager(LingoMovie movie) => movie.Transitions;

    internal override void ShowCreateSpriteDialog(int frameNumber, Action<LingoSprite?> newSprite)
    {
        var first = _transitions.First();
        var settings = new LingoTransitionFrameSettings
        {
            TransitionId = first.Id,
            TransitionName = first.Name
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
        panel.Width = 450;
        panel.Height = 300;

        var categoryOptions = new List<KeyValuePair<string, string>> { new("All", "All") };
        categoryOptions.AddRange(_transitions.Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .Select(c => new KeyValuePair<string, string>(c, c)));

        panel.SetLabelAt("TransitionLabel", 140, 10, "Transitions:");
        var transitionList = panel.SetInputListAt(Array.Empty<KeyValuePair<string, string>>(), "TransitionList", 140, 25, 200, null, key =>
        {
            if (int.TryParse(key, out var id))
            {
                settings.TransitionId = id;
                var tr = _transitionLibrary.Get(id);
                settings.TransitionName = tr.Name;
            }
        });
        transitionList.Height = 200;

        panel.SetLabelAt("CategoryLabel", 10, 10, "Categories:");
        var categoryList = panel.SetInputListAt(categoryOptions, "CategoryList", 10, 25, 120, "All", key =>
        {
            PopulateTransitions(key);
        });
        categoryList.Height = 200;

        PopulateTransitions("All");

        panel.SetLabelAt("DurationLabel", 10, 230, "Duration:");
        panel.SetSliderAt(settings, "DurationSlider", 70, 230, 260, AOrientation.Horizontal, s => s.Duration, 1f, 30f, 0.1f);

        panel.SetLabelAt("SmoothnessLabel", 10, 260, "Smoothness:");
        panel.SetSliderAt(settings, "SmoothnessSlider", 70, 260, 260, AOrientation.Horizontal, s => s.Smoothness, 0f, 100f, 1f);

        panel.SetLabelAt("AffectsLabel", 10, 290, "Affects:");
        var affectsOptions = new[]
        {
            new KeyValuePair<string, string>(LingoTransitionAffects.EntireStage.ToString(), "Entire Stage"),
            new KeyValuePair<string, string>(LingoTransitionAffects.ChangingAreaOnly.ToString(), "Changing Area Only")
        };
        panel.SetComboBoxAt(affectsOptions, "AffectsCombo", 70, 290, 160, settings.Affects.ToString(), key =>
        {
            if (Enum.TryParse<LingoTransitionAffects>(key, out var affects))
                settings.Affects = affects;
        });

        panel.AddPopupButtons(okAction, CloseDialog);

        _dialog = _showConfirmDialog?.Invoke("Frame Properties: Transition", (IAbstFrameworkPanel)panel.FrameworkObj);

        void PopulateTransitions(string? category)
        {
            var filtered = _transitions
                .Where(t => category == "All" || t.Category == category)
                .Select(t => new KeyValuePair<string, string>(t.Id.ToString(), t.Name))
                .ToList();

            transitionList.ClearItems();
            foreach (var item in filtered)
                transitionList.AddItem(item.Key, item.Value);

            var selectedKey = settings.TransitionId.ToString();
            if (filtered.Any(t => t.Key == selectedKey))
                transitionList.SelectedKey = selectedKey;
            else if (filtered.Count > 0)
            {
                var firstTr = filtered[0];
                transitionList.SelectedKey = firstTr.Key;
                settings.TransitionId = int.Parse(firstTr.Key);
                settings.TransitionName = firstTr.Value;
            }
        }
    }

    private void CloseDialog()
    {
        if (_dialog == null) return;
        _dialog.Close();
        _dialog = null;
    }
}

