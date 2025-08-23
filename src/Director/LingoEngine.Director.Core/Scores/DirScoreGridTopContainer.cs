using AbstUI.Components.Containers;
using AbstUI.Windowing;
using LingoEngine.ColorPalettes;
using LingoEngine.Director.Core.Scores.ColorPalettes;
using LingoEngine.Director.Core.Scores.FrameScripts;
using LingoEngine.Director.Core.Scores.Sounds;
using LingoEngine.Director.Core.Scores.Tempos;
using LingoEngine.Director.Core.Scores.Transitions;
using LingoEngine.Transitions.TransitionLibrary;


namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreGridTopContainer : DirScoreGridContainer
    {
        private readonly int _frameScriptIndex;
        private bool _collapsed;
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                _collapsed = value;
                UpdateChannelsVisibility();
                UpdateSize();
            }
        }


        public DirScoreGridTopContainer(IDirScoreManager scoreManager, ILingoColorPaletteDefinitions paletteDefinitions, ILingoTransitionLibrary transitionLibrary, Func<string, IAbstFrameworkPanel, IAbstWindowDialogReference?> showConfirmDialog)
            : base(scoreManager, 6, showConfirmDialog)
        {
            SetChannels(
            [
                new DirScoreTempoGridChannel(_scoreManager),
                new DirScoreColorPaletteGridChannel(_scoreManager, paletteDefinitions),
                new DirScoreTransitionGridChannel(_scoreManager, transitionLibrary),
                new DirScoreAudioGridChannel(1, _scoreManager),
                new DirScoreAudioGridChannel(2, _scoreManager),
                new DirScoreFrameScriptGridChannel(_scoreManager),
            ]);
            _frameScriptIndex = _channelsDic.Count - 1;
            UpdateChannelsVisibility();
            UpdateSize();
        }


        protected override DirScoreChannel? GetChannelByDisplayIndex(int index)
        {
            if (_collapsed) return _channelsDic[_frameScriptIndex + 1];
            return base.GetChannelByDisplayIndex(index);
        }
        protected void UpdateChannelsVisibility()
        {
            for (int i = 0; i < _channels.Length; i++)
            {
                _channels[i].Visible = i == _frameScriptIndex || !_collapsed;
            }
            UpdateChannelsPosition();
        }


    }

}
