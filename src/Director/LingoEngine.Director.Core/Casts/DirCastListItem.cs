using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.UI;
using LingoEngine.Texts;
using AbstUI.Primitives;
using System.Xml.Linq;
using AbstUI.Components.Graphics;

namespace LingoEngine.Director.Core.Casts
{
    /// <summary>
    /// List representation for a cast member showing multiple columns.
    /// </summary>
    public class DirCastListItem : IDirCastItem, IDisposable
    {
        private readonly AbstGfxCanvas _panel;
        private readonly IDirectorIconManager _iconManager;
        private IAbstTexture2D? _icon;
        private IAbstUITextureUserSubscription? _iconSubscription;
        private bool _selected;
        private bool _hovered;

        public const int RowHeight = 20;
        public ILingoMember? Member { get; private set; }
        public AbstGfxCanvas Panel => _panel;

        public DirCastListItem(ILingoFrameworkFactory factory, ILingoMember member, IDirectorIconManager iconManager)
        {
            _iconManager = iconManager;
            _panel = factory.CreateGfxCanvas("CastListRow", 370, RowHeight);
            _panel.Height = RowHeight;
            SetMember(member);
        }

        private void DrawRow()
        {
            if (Member == null)
                return;
            var member = Member;
            var textColor = AColors.Black;
            var bgColor = AColors.White;
            var vPosText = 2;
            if (_selected)
            {
                bgColor = DirectorColors.BlueSelectColor;
                textColor = AColors.White;
            }
            else if (_hovered)
            {
                bgColor = DirectorColors.ListHoverColor;
                textColor = AColors.Black;
            }
            _panel.Clear(bgColor);
            if (_icon != null)
                _panel.DrawPicture(_icon, 16, 16, new APoint(2, 2));
            _panel.DrawText(new APoint(22, vPosText), member.Name, null, textColor, 9, 120);
            _panel.DrawText(new APoint(150, vPosText), member.NumberInCast.ToString(), null, textColor, 9, 40);
            _panel.DrawText(new APoint(190, vPosText), member is LingoEngine.Scripts.LingoMemberScript script ? script.ScriptType.ToString() : "", null, textColor, 10, 280);
            _panel.DrawText(new APoint(250, vPosText), member.ModifiedDate.ToShortDateString(), null, textColor, 9, 90);
            var size = (int)_panel.Width - 362;
            if (size > 20)
                _panel.DrawText(new APoint(342, vPosText), member.Comments, null, textColor, 9, size);
            _panel.DrawLine(new APoint(0, 0), new APoint(0, RowHeight ), DirectorColors.LineDark);
            _panel.DrawLine(new APoint(_panel.Width, 0), new APoint(_panel.Width, RowHeight ), DirectorColors.LineDark);
        }

        public void SetMember(ILingoMember member)
        {
            Member = member;
            _iconSubscription?.Release();
            _iconSubscription = null;
           var iconType = LingoMemberTypeIcons.GetIconType(member);
            if (iconType.HasValue)
            {
                _icon = _iconManager.Get(iconType.Value);
                _iconSubscription = _icon.AddUser(this);
            }
            DrawRow();
        }
        public void SetWidth(int width)
        {
            _panel.Width = width;
            DrawRow();
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            DrawRow();
        }

        public void SetHovered(bool hovered)
        {
            _hovered = hovered;
            DrawRow();
        }


        public void Dispose()
        {
            _panel.Dispose();
            _iconSubscription?.Release();
            _iconSubscription = null;
        }
    }
}
