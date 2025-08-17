using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.UI;
using LingoEngine.Texts;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.Director.Core.Casts
{
    /// <summary>
    /// List representation for a cast member showing multiple columns.
    /// </summary>
    public class DirCastListItem : IDirCastItem, IDisposable
    {
        private readonly AbstPanel _panel;
        private readonly AbstLabel _name;
        private readonly AbstLabel _number;
        private readonly AbstLabel _script;
        private readonly AbstLabel _modified;
        private readonly AbstLabel _comments;
        private readonly AbstGfxCanvas _iconCanvas;
        private readonly ILingoFrameworkFactory _factory;
        private readonly IDirectorIconManager _iconManager;
        private IAbstUITextureUserSubscription? _iconSubscription;
        private bool _selected;
        private bool _hovered;

        public const int RowHeight = 20;
        public ILingoMember? Member { get; private set; }
        public AbstPanel Panel => _panel;

        public DirCastListItem(ILingoFrameworkFactory factory, ILingoMember? member, IDirectorIconManager iconManager)
        {
            _factory = factory;
            _iconManager = iconManager;
            _panel = factory.CreatePanel("CastListRow");
            _panel.Height = RowHeight;
            _panel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _iconCanvas = _panel.SetGfxCanvasAt("Icon", 2, 2, 16, 16);
            _name = _panel.SetLabelAt("Name", 22, 2, fontSize: 10, labelWidth: 120);
            _number = _panel.SetLabelAt("Number", 150, 2, fontSize: 10, labelWidth: 40);
            _script = _panel.SetLabelAt("Script", 190, 2, fontSize: 10, labelWidth: 80);
            _modified = _panel.SetLabelAt("Modified", 270, 2, fontSize: 10, labelWidth: 90);
            _comments = _panel.SetLabelAt("Comments", 362, 2, fontSize: 10, labelWidth: 200);
            SetMember(member);
        }

        public void SetMember(ILingoMember? member)
        {
            Member = member;
            _iconSubscription?.Release();
            _iconSubscription = null;
            _iconCanvas.Clear(AColors.Transparent);
            if (member != null)
            {
                var iconType = LingoMemberTypeIcons.GetIconType(member);
                if (iconType.HasValue)
                {
                    var tex = _iconManager.Get(iconType.Value);
                    _iconSubscription = tex.AddUser(this);
                    _iconCanvas.DrawPicture(tex, 16, 16, new APoint(0, 0));
                }
                _name.Text = member.Name;
                _number.Text = member.NumberInCast.ToString();
                _script.Text = member is LingoEngine.Scripts.LingoMemberScript script ? script.ScriptType.ToString() : "";
                _modified.Text = member.ModifiedDate.ToShortDateString();
                _comments.Text = member.Comments;
            }
            else
            {
                _name.Text = string.Empty;
                _number.Text = string.Empty;
                _script.Text = string.Empty;
                _modified.Text = string.Empty;
                _comments.Text = string.Empty;
            }
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            UpdateColors();
        }

        public void SetHovered(bool hovered)
        {
            _hovered = hovered;
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (_selected)
            {
                _panel.BackgroundColor = DirectorColors.BlueSelectColor;
                _name.FontColor = AColors.White;
                _number.FontColor = AColors.White;
                _script.FontColor = AColors.White;
                _modified.FontColor = AColors.White;
                _comments.FontColor = AColors.White;
            }
            else if (_hovered)
            {
                _panel.BackgroundColor = DirectorColors.LineLight;
                _name.FontColor = AColors.Black;
                _number.FontColor = AColors.Black;
                _script.FontColor = AColors.Black;
                _modified.FontColor = AColors.Black;
                _comments.FontColor = AColors.Black;
            }
            else
            {
                _panel.BackgroundColor = DirectorColors.BG_WhiteMenus;
                _name.FontColor = AColors.Black;
                _number.FontColor = AColors.Black;
                _script.FontColor = AColors.Black;
                _modified.FontColor = AColors.Black;
                _comments.FontColor = AColors.Black;
            }
        }

        public void Dispose()
        {
            _iconSubscription?.Release();
            _iconSubscription = null;
            _iconCanvas.Dispose();
        }
    }
}
