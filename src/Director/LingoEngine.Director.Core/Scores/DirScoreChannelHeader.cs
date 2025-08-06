using LingoEngine.Gfx;
using LingoEngine.Primitives;


namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreChannelHeader
    {
        private LingoColor _textColor = LingoColor.FromHex("#a0a0a0");
        protected readonly DirScoreGfxValues _gfxValues;
        private readonly Action<DirScoreChannelHeader,bool> _muteStateChanged;

        public virtual string Icon { get; set; }
        public virtual string Label { get; set; }
        private bool _muted;
        public bool IsMuted => _muted;

        public int Width { get; private set; }  
        public int Height { get; private set; }  
        public float Y { get; set; }
        public bool Visible { get; internal set; } = true;

        public DirScoreChannelHeader(string icon, string label, DirScoreGfxValues gfxValues, Action<DirScoreChannelHeader,bool> muteStateChanged)
        {
            _gfxValues = gfxValues;
            _muteStateChanged = muteStateChanged;
            Width = _gfxValues.ChannelInfoWidth;
            Height = _gfxValues.ChannelHeight;
            Icon = icon;
            Label = label ?? "";
        }

        public void Draw(LingoGfxCanvas canvas)
        {
            
            var y = Y;
            
            //canvas.DrawRect(new LingoRect(0, y, Width, Height), DirectorColors.BG_WhiteMenus); // new Color("#f0f0f0"));
            canvas.DrawText(new LingoPoint(20 + 4, y+ 11), Icon,null, _textColor, 10);
            if (!string.IsNullOrEmpty(Label))
                canvas.DrawText(new LingoPoint(20 + 2, y+ 11), Label, null, _textColor, 10,45, LingoEngine.Texts.LingoTextAlignment.Right);

            // Visibility rect
            LingoColor vis = !_muted ? LingoColorList.LightGray : new LingoColor(40, 40, 40);
            canvas.DrawRect(new LingoRect(2, y + 2, _gfxValues.ChannelHeight - 4, y+_gfxValues.ChannelHeight - 4), vis);

            // Horizontal line at the bottom
            canvas.DrawLine(new LingoPoint(0, y + _gfxValues.ChannelHeight), new LingoPoint(Width, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
            canvas.DrawLine(new LingoPoint(0, y + _gfxValues.ChannelHeight + 1), new LingoPoint(Width, y + _gfxValues.ChannelHeight + 1), _gfxValues.ColLineLight);

            // Vertical line after visibility
            canvas.DrawLine(new LingoPoint(_gfxValues.ChannelHeight + 0, y + 1), new LingoPoint(_gfxValues.ChannelHeight + 0, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
            canvas.DrawLine(new LingoPoint(_gfxValues.ChannelHeight + 1, y + 0), new LingoPoint(_gfxValues.ChannelHeight + 1, y + _gfxValues.ChannelHeight - 2), _gfxValues.ColLineLight);

            // Vertical line at the end
            canvas.DrawLine(new LingoPoint(Width, y), new LingoPoint(Width, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
        }
        public void SetMuted(bool state)
        {
            _muted = state;
            _muteStateChanged(this, state);
            OnMutedChanged(state);
        }
        protected virtual void OnMutedChanged(bool state) { }
        public void ToggleMute() => SetMuted(!_muted);
    }
}
