using LingoEngine.Director.Core.Styles;
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
            var xOffset = 1;
            var y = Y;
            

            canvas.DrawText(new LingoPoint(xOffset+20 + 25, y+ 11), Icon,null, _textColor, 10);
            if (!string.IsNullOrEmpty(Label))
                canvas.DrawText(new LingoPoint(xOffset+20 + 2, y+ 11), Label, null, _textColor, 10,45, LingoEngine.Texts.LingoTextAlignment.Right);

            // Visibility rect
            var btnWidth = 8;
            var btnHeight = 8;
            var btnLeft = xOffset+5;
            var btnTop = y+ 4;
            if (_muted)
            {
                canvas.DrawRect(new LingoRect(btnLeft, btnTop, btnLeft+ btnWidth, btnTop+btnHeight), DirectorColors.LineDarker);
                canvas.DrawLine(new LingoPoint(btnLeft, btnTop + btnHeight), new LingoPoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineLight);  // bottom line
                canvas.DrawLine(new LingoPoint(btnLeft + btnWidth, btnTop), new LingoPoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineLight); // right line
            }
            else
            {
                canvas.DrawLine(new LingoPoint(btnLeft, btnTop), new LingoPoint(btnLeft + btnWidth, btnTop), DirectorColors.LineLight);  // top line
                canvas.DrawLine(new LingoPoint(btnLeft, btnTop), new LingoPoint(btnLeft, btnTop + btnHeight), DirectorColors.LineLight); // left line
                canvas.DrawLine(new LingoPoint(btnLeft, btnTop + btnHeight), new LingoPoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineDarker);  // bottom line
                canvas.DrawLine(new LingoPoint(btnLeft + btnWidth, btnTop), new LingoPoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineDarker); // right line
            }

            // Horizontal line at the bottom
            canvas.DrawLine(new LingoPoint(xOffset, y + _gfxValues.ChannelHeight), new LingoPoint(Width, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
            canvas.DrawLine(new LingoPoint(xOffset, y + _gfxValues.ChannelHeight + 1), new LingoPoint(Width, y + _gfxValues.ChannelHeight + 1), _gfxValues.ColLineLight);

            // Vertical line after visibility
            var visOffset = _gfxValues.ChannelHeight;
            canvas.DrawLine(new LingoPoint(_gfxValues.ChannelHeight + xOffset + 0, y + 1), new LingoPoint(_gfxValues.ChannelHeight + xOffset + 0, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
            canvas.DrawLine(new LingoPoint(_gfxValues.ChannelHeight + xOffset + 1, y + 0), new LingoPoint(_gfxValues.ChannelHeight + xOffset + 1, y + _gfxValues.ChannelHeight - 2), _gfxValues.ColLineLight);

            // Vertical line at the beginning
            canvas.DrawLine(new LingoPoint(xOffset, y), new LingoPoint(xOffset, y + _gfxValues.ChannelHeight), _gfxValues.ColLineLight);

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
