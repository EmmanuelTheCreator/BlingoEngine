using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Movies;
using LingoEngine.Transitions;
using LingoEngine.Transitions.TransitionLibrary;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Sprites;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;

namespace LingoEngine.Director.Core.Scores.Transitions;

internal partial class DirScoreTransitionGridChannel : DirScoreChannel<ILingoSpriteTransitionManager, DirScoreTransitionSprite, LingoTransitionSprite>
{
    private readonly ILingoTransitionLibrary _transitionLibrary;
    private IAbstWindowDialogReference? _dialog;
    private float _duration;
    private float _smoothness;
    private AbstInputSlider<float>? _smoothnessSlider;
    private AbstLabel? _smoothnessVal;
    private AbstInputSlider<float>? _durationSlider;
    private AbstLabel? _durationVal;
    private readonly List<LingoBaseTransition> _transitions;

    /// <summary>
    /// Duration in Seconds from 0 to 30 seconds
    /// </summary>
    public float Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            if (_durationVal != null) _durationVal.Text = value.ToString();
            if (_durationSlider != null) _durationSlider.Value = value;
        }
    }
    public float Smoothness
    {
        get => _smoothness;
        set
        {
            _smoothness = value;
            if (_smoothnessVal != null) _smoothnessVal.Text = value.ToString();
            if (_smoothnessSlider != null) _smoothnessSlider.Value = value;
        }
    }

    public int RectX { get; set; }

    public int RectY { get; set; }

    public int RectWidth { get; set; }

    public int RectHeight { get; set; }

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
            SetNewValues(settings);
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
            SetNewValues(settings);
            transitionSprite.SetSettings(settings);
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }

    private void SetNewValues(LingoTransitionFrameSettings settings)
    {
        settings.Duration = Duration;
        settings.Smoothness = Smoothness;
        settings.Rect = ARect.New(RectX, RectY, RectWidth, RectHeight);
    }

    private void ShowDialog(LingoTransitionFrameSettings settings, Action okAction)
    {
        var panel = _scoreManager.Factory.CreatePanel("Panel Transition Sprite");
        panel.Width = 450;
        panel.Height = 350;

        var transitionList = panel.SetInputListAt(Array.Empty<KeyValuePair<string, string>>(), "TransitionList", 140, 25, 200, null, key =>
        {
            if (int.TryParse(key, out var id))
            {
                settings.TransitionId = id;
                var tr = _transitionLibrary.Get(id);
                settings.TransitionName = tr.Name;
            }
        });

        var categoryOptions = new List<KeyValuePair<string, string>> { new("All", "All") };
        categoryOptions.AddRange(_transitions.Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .Select(c => new KeyValuePair<string, string>(c, c)));

        panel.SetLabelAt("CategoryLabel", 10, 10, "Categories:");

        PopulateTransitions(settings, transitionList, "All");

        var categoryList = panel.SetInputListAt(categoryOptions, "CategoryList", 10, 25, 120, "All", key1 =>
        {
            PopulateTransitions(settings, transitionList, key1);
        });
        categoryList.Height = 200;

        panel.SetLabelAt("TransitionLabel", 140, 10, "Transitions:");

        transitionList.Height = 200;



        panel.SetLabelAt("DurationLabel", 10, 230, "Duration (secs):");
        _durationSlider = panel.SetSliderAt(this, "DurationSlider", 100, 230, 190, AOrientation.Horizontal, s => s.Duration, 1f, 30f, 0.1f);
        _durationVal = panel.SetLabelAt("DurationVal", 310, 230, "0");

        panel.SetLabelAt("SmoothnessLabel", 10, 260, "Smoothness:");
        _smoothnessSlider = panel.SetSliderAt(this, "SmoothnessSlider", 100, 260, 190, AOrientation.Horizontal, s => s.Smoothness, 0f, 100f, 1f);
        _smoothnessVal = panel.SetLabelAt("SmoothnessVal", 310, 260, "0");

        panel.SetLabelAt("AffectsLabel", 10, 290, "Affects:");
        var affectsOptions = new[]
        {
            new KeyValuePair<string, string>(LingoTransitionAffects.EntireStage.ToString(), "Entire Stage"),
            new KeyValuePair<string, string>(LingoTransitionAffects.ChangingAreaOnly.ToString(), "Changing Area Only"),
            new KeyValuePair<string, string>(LingoTransitionAffects.Custom.ToString(), "Custom")
        };
        var xLabel = panel.SetLabelAt("RectXLabel", 240, 275, "X:");
        var xInput = panel.SetInputNumberAt(this, "RectX", 240, 290, 40, s => s.RectX, 0, null);
        var yLabel = panel.SetLabelAt("RectYLabel", 285, 275, "Y:");
        var yInput = panel.SetInputNumberAt(this, "RectY", 285, 290, 40, s => s.RectY, 0, null);
        var wLabel = panel.SetLabelAt("RectWLabel", 330, 275, "W:");
        var wInput = panel.SetInputNumberAt(this, "RectWidth", 330, 290, 40, s => s.RectWidth, 1, null);
        var hLabel = panel.SetLabelAt("RectHLabel", 375, 275, "H:");
        var hInput = panel.SetInputNumberAt(this, "RectHeight", 375, 290, 40, s => s.RectHeight, 1, null);
        void ToggleRectInputs(bool visible)
        {
            xLabel.Visibility = xInput.Visibility = yLabel.Visibility = yInput.Visibility = wLabel.Visibility = wInput.Visibility = hLabel.Visibility = hInput.Visibility = visible;
        }
        ToggleRectInputs(settings.Affects == LingoTransitionAffects.Custom);
        panel.SetComboBoxAt(affectsOptions, "AffectsCombo", 70, 290, 160, settings.Affects.ToString(), key =>
        {
            if (Enum.TryParse<LingoTransitionAffects>(key, out var affects))
            {
                settings.Affects = affects;
                ToggleRectInputs(affects == LingoTransitionAffects.Custom);
            }
        });

        panel.AddPopupButtons(okAction, CloseDialog);

        _dialog = _showConfirmDialog?.Invoke("Frame Properties: Transition", (IAbstFrameworkPanel)panel.FrameworkObj);
        Smoothness = settings.Smoothness;
        Duration = settings.Duration;
        RectX = (int)settings.Rect.Left;
        RectY = (int)settings.Rect.Top;
        RectWidth = (int)settings.Rect.Width;
        RectHeight = (int)settings.Rect.Height;

    }
    private void PopulateTransitions(LingoTransitionFrameSettings settings, AbstItemList transitionList, string? category)
    {
        var filtered = _transitions
            .Where(t => category == "All" || t.Category == category)
            .Select(t => new KeyValuePair<string, string>(t.Id.ToString(), t.Id+". "+ t.Name))
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
            settings.TransitionName =  firstTr.Value;
        }
    }

    private void CloseDialog()
    {
        if (_dialog == null) return;
        _dialog.Close();
        _dialog = null;
    }
}

