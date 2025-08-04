using LingoEngine.Director.Core.Scores.ColorPalettes;
using LingoEngine.Director.Core.Scores.FrameScripts;
using LingoEngine.Director.Core.Scores.Sounds;
using LingoEngine.Director.Core.Scores.Tempos;
using LingoEngine.Director.Core.Scores.Transitions;
using LingoEngine.Inputs;

namespace LingoEngine.Director.Core.Scores
{
    public interface IDirScoreFrameworkGridContainer
    {
        float CurrentFrameX { get; set; }
        void RequireRedrawChannels();
        void UpdateSize();
    }
    public class DirScoreTopGridContainer : DirScoreGridContainer
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


        public DirScoreTopGridContainer(IDirScoreManager scoreManager, ILingoMouse mouse)
            :base(scoreManager, mouse, 6) 
        {
            _channels =
            [
                new DirScoreTempoGridChannel(_scoreManager),
                new DirScoreColorPaletteGridChannel(_scoreManager),
                new DirScoreTransitionGridChannel(_scoreManager),
                new DirScoreAudioGridChannel(1, _scoreManager),
                new DirScoreAudioGridChannel(2, _scoreManager),
                new DirScoreFrameScriptGridChannel(_scoreManager),
            ];
            _frameScriptIndex = _channels.Length - 1;
            UpdateChannelsVisibility();
            UpdateSize();
        }


        protected override DirScoreChannel? GetChannelByDisplayIndex(int index)
        {
            if (_collapsed) return _channels[_frameScriptIndex];
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
