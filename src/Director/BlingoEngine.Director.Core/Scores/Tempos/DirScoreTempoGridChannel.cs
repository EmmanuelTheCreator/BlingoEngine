using BlingoEngine.Movies;
using BlingoEngine.Tempos;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Sprites;
using BlingoEngine.Director.Core.Windowing;
using AbstUI.Windowing;
using AbstUI.Components.Containers;
namespace BlingoEngine.Director.Core.Scores.Tempos;


internal partial class DirScoreTempoGridChannel : DirScoreChannel<IBlingoTempoSpriteManager, DirScoreTempoSprite, BlingoTempoSprite>
{
    
    private KeyValuePair<string, string>[] _values = new[]
                {
                    new KeyValuePair<string, string>("ChangeTempo","Change Tempo"),
                    new KeyValuePair<string, string>("WaitSeconds","Wait"),
                    new KeyValuePair<string, string>("WaitForUserInput","Wait for Mouse Click or Key Press"),
                    new KeyValuePair<string, string>("WaitForCuePoint","Wait for Cue Point"),
                };
    private IAbstWindowDialogReference? _dialog;

    public DirScoreTempoGridChannel(IDirScoreManager scoreManager)
        : base(BlingoTempoSprite.SpriteNumOffset+1, scoreManager)
    {
        IsSingleFrame = true;
    }

    protected override DirScoreTempoSprite CreateUISprite(BlingoTempoSprite sprite, IDirSpritesManager spritesManager) => new DirScoreTempoSprite(sprite, spritesManager);

    protected override IBlingoTempoSpriteManager GetManager(BlingoMovie movie) => movie.Tempos;


    internal override void ShowCreateSpriteDialog(int frameNumber, Action<BlingoSprite?> newSprite)
    {
        var settings = new BlingoTempoSpriteSettings();
        Action okAction = () =>
        {
            var sprite = _manager!.Add(frameNumber, settings);
            newSprite(sprite);
            _hasDirtySpriteList = true;
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }

  

    internal override void ShowSpriteDialog(BlingoSprite sprite)
    {
        if (!(sprite is BlingoTempoSprite tempoSprite)) return;
        var settings = tempoSprite.GetSettings();
        if (settings == null)
            settings = new BlingoTempoSpriteSettings();
        Action okAction = () =>
        {
            if (settings != null)
                tempoSprite.SetSettings(settings);
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }


    private void ShowDialog(BlingoTempoSpriteSettings settings, Action okAction)
    {
        
        var panel = _scoreManager.Factory.CreatePanel("Create Tempo Sprite");
        panel.Width = 500;
        panel.Height = 100;
        var tempo1 = panel.SetLabelAt("TempoLbl", 10, 60, "Tempo:");
        var tempo2 = panel.SetInputNumberAt(settings, "Tempo", 100, 60, 50, s => s.Tempo, 1, 120);

        var wait1 = panel.SetLabelAt("WaitLbl", 10, 60, "Wait in seconds:");
        var wait2 = panel.SetInputNumberAt(settings, "Wait", 100, 60, 50, s => s.WaitSeconds, 1, 120);

        // todo : add channel combo and cue point combo

        wait1.Visibility = false;
        wait2.Visibility = false;
        Action<BlingoTempoSpriteAction> applyVisibility = (actionType) =>
        {
            switch (actionType)
            {
                case BlingoTempoSpriteAction.ChangeTempo:
                    tempo1.Visibility = true;
                    tempo2.Visibility = true;
                    wait1.Visibility = false;
                    wait2.Visibility = false;
                    break;
                case BlingoTempoSpriteAction.WaitSeconds:
                    tempo1.Visibility = false;
                    tempo2.Visibility = false;
                    wait1.Visibility = true;
                    wait2.Visibility = true;
                    break;
                case BlingoTempoSpriteAction.WaitForUserInput:
                    tempo1.Visibility = false;
                    tempo2.Visibility = false;
                    wait1.Visibility = false;
                    wait2.Visibility = false;
                    break;
                case BlingoTempoSpriteAction.WaitForCuePoint:
                    tempo1.Visibility = false;
                    tempo2.Visibility = false;
                    wait1.Visibility = false;
                    wait2.Visibility = false;
                    break;
                default:
                    break;
            }
        };
        applyVisibility(settings.Action);

        panel.SetComboBoxAt(_values, "ComboTempo", 10, 30, 250, settings.Action.ToString(), actionKey =>
        {
            if (actionKey == null) return;
            var actionType = Enum.Parse<BlingoTempoSpriteAction>(actionKey);
            settings.Action = actionType;
            applyVisibility(actionType);
        });

        panel.AddPopupButtons(okAction, CloseDialog);

        _dialog = _showConfirmDialog?.Invoke("Frame Properties: Tempo", (IAbstFrameworkPanel)panel.FrameworkObj);
    }

    private void CloseDialog()
    {
        if (_dialog == null) return;
        _dialog.Close();
        _dialog = null;
    }
   
}

