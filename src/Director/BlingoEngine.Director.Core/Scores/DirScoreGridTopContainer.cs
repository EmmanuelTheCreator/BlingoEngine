using AbstUI.Components.Containers;
using AbstUI.Windowing;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Director.Core.Scores.ColorPalettes;
using BlingoEngine.Director.Core.Scores.FrameScripts;
using BlingoEngine.Director.Core.Scores.Sounds;
using BlingoEngine.Director.Core.Scores.Tempos;
using BlingoEngine.Director.Core.Scores.Transitions;
using BlingoEngine.Transitions.TransitionLibrary;


namespace BlingoEngine.Director.Core.Scores
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


        public DirScoreGridTopContainer(IDirScoreManager scoreManager, IBlingoColorPaletteDefinitions paletteDefinitions, IBlingoTransitionLibrary transitionLibrary, Func<string, IAbstFrameworkPanel, IAbstWindowDialogReference?> showConfirmDialog)
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

