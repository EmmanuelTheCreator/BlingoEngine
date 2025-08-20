using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Primitives;


namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreChannelHeader
    {
        private AColor _textColor = AColor.FromHex("#a0a0a0");
        protected readonly DirScoreGfxValues _gfxValues;
        private readonly Action<DirScoreChannelHeader, bool> _muteStateChanged;

        public virtual string Icon { get; set; }
        public virtual string Label { get; set; }
        private bool _muted;
        public bool IsMuted => _muted;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public float Y { get; set; }
        public bool Visible { get; internal set; } = true;

        public DirScoreChannelHeader(string icon, string label, DirScoreGfxValues gfxValues, Action<DirScoreChannelHeader, bool> muteStateChanged)
        {
            _gfxValues = gfxValues;
            _muteStateChanged = muteStateChanged;
            Width = _gfxValues.ChannelInfoWidth;
            Height = _gfxValues.ChannelHeight;
            Icon = icon;
            Label = label ?? "";
        }

        public void Draw(AbstGfxCanvas canvas)
        {
            var xOffset = 1;
            var y = Y;


            canvas.DrawText(new APoint(xOffset + 20 + 25, y + 11), Icon, null, _textColor, 10);
            if (!string.IsNullOrEmpty(Label))
                canvas.DrawText(new APoint(xOffset + 20 + 2, y + 11), Label, null, _textColor, 10, 45, AbstUI.Texts.AbstTextAlignment.Right);

            // Visibility rect
            var btnWidth = 8;
            var btnHeight = 8;
            var btnLeft = xOffset + 5;
            var btnTop = y + 4;
            if (_muted)
            {
                canvas.DrawRect(new ARect(btnLeft, btnTop, btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineDarker);
                canvas.DrawLine(new APoint(btnLeft, btnTop + btnHeight), new APoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineLight);  // bottom line
                canvas.DrawLine(new APoint(btnLeft + btnWidth, btnTop), new APoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineLight); // right line
            }
            else
            {
                canvas.DrawLine(new APoint(btnLeft, btnTop), new APoint(btnLeft + btnWidth, btnTop), DirectorColors.LineLight);  // top line
                canvas.DrawLine(new APoint(btnLeft, btnTop), new APoint(btnLeft, btnTop + btnHeight), DirectorColors.LineLight); // left line
                canvas.DrawLine(new APoint(btnLeft, btnTop + btnHeight), new APoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineDarker);  // bottom line
                canvas.DrawLine(new APoint(btnLeft + btnWidth, btnTop), new APoint(btnLeft + btnWidth, btnTop + btnHeight), DirectorColors.LineDarker); // right line
            }

            // Horizontal line at the bottom
            canvas.DrawLine(new APoint(xOffset, y + _gfxValues.ChannelHeight), new APoint(Width, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
            canvas.DrawLine(new APoint(xOffset, y + _gfxValues.ChannelHeight + 1), new APoint(Width, y + _gfxValues.ChannelHeight + 1), _gfxValues.ColLineLight);

            // Vertical line after visibility
            var visOffset = _gfxValues.ChannelHeight;
            canvas.DrawLine(new APoint(_gfxValues.ChannelHeight + xOffset + 0, y + 1), new APoint(_gfxValues.ChannelHeight + xOffset + 0, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);
            canvas.DrawLine(new APoint(_gfxValues.ChannelHeight + xOffset + 1, y + 0), new APoint(_gfxValues.ChannelHeight + xOffset + 1, y + _gfxValues.ChannelHeight - 2), _gfxValues.ColLineLight);

            // Vertical line at the beginning
            canvas.DrawLine(new APoint(xOffset, y), new APoint(xOffset, y + _gfxValues.ChannelHeight), _gfxValues.ColLineLight);

            // Vertical line at the end
            canvas.DrawLine(new APoint(Width, y), new APoint(Width, y + _gfxValues.ChannelHeight), _gfxValues.ColLineDark);


        }
        public void SetMuted(bool state, bool notify = true)
        {
            _muted = state;
            if (notify)
                _muteStateChanged(this, state);
            OnMutedChanged(state);
        }
        public void SetMutedExternal(bool state) => SetMuted(state, false);
        protected virtual void OnMutedChanged(bool state) { }
        public void ToggleMute() => SetMuted(!_muted);
    }
}
