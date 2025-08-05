using LingoEngine.Movies;
using LingoEngine.Tempos;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.UI;
using LingoEngine.Gfx;
namespace LingoEngine.Director.Core.Scores.Tempos;


internal partial class DirScoreTempoGridChannel : DirScoreChannel<ILingoTempoSpriteManager, DirScoreTempoSprite, LingoTempoSprite>
{
    private int _editFrame;

    private bool groupTempoSelected;
    private bool groupWait;

    public bool GroupTempoSelected { get => groupTempoSelected; set => groupTempoSelected = value; }
    public bool GroupWait { get => groupWait; set => groupWait = value; }

    public DirScoreTempoGridChannel(IDirScoreManager scoreManager)
        : base(LingoTempoSprite.SpriteNumOffset+1, scoreManager)
    {
    }

    protected override DirScoreTempoSprite CreateUISprite(LingoTempoSprite sprite, IDirSpritesManager spritesManager) => new DirScoreTempoSprite(sprite, spritesManager);

    protected override ILingoTempoSpriteManager GetManager(LingoMovie movie) => movie.Tempos;


    internal override void ShowCreateSpriteDialog(int frameNumber, Action<LingoSprite?> newSprite)
    {
        var settings = new LingoTempoSpriteSettings();
        Action okAction = () =>
        {
            var sprite = _manager!.Add(frameNumber, settings);
            newSprite(sprite);
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }

  

    internal override void ShowSpriteDialog(LingoSprite sprite)
    {
        if (!(sprite is LingoTempoSprite tempoSprite)) return;
        var settings = tempoSprite.GetSettings();
        if (settings == null)
            settings = new LingoTempoSpriteSettings();
        Action okAction = () =>
        {
            if (settings != null)
                tempoSprite.SetSettings(settings);
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }


    private void ShowDialog(LingoTempoSpriteSettings settings, Action okAction)
    {
        var onClose = () => { };
        var panel = _scoreManager.Factory.CreatePanel("Create Tempo Sprite");
        panel.Width = 500;
        panel.Height = 300;
        panel.SetInputNumberAt(_scoreManager.Factory, settings, "Tempo", 100, 30, 50, s => s.Tempo, 1, 120);
        panel.SetStateButtonAt(_scoreManager.Factory, this, "ComboTempo", 10, 30, x => x.GroupTempoSelected, null, "Tempo");
        panel.SetStateButtonAt(_scoreManager.Factory, this, "ComboWait", 10, 60, x => x.GroupWait, null, "Wait");
        panel.SetButtonAt(_scoreManager.Factory, "OKBtn", "OK", 400, 30, () => { okAction(); onClose(); });
        panel.SetButtonAt(_scoreManager.Factory, "CancelBtn", "Cancel", 400, 60, onClose);
        var dialog = _showConfirmDialog?.Invoke("Frame Properties Tempo", (ILingoFrameworkGfxPanel)panel.FrameworkObj);
        if (dialog != null)
            onClose = dialog.Close;
    }

}
