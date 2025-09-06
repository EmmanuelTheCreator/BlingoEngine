using System;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Members;
using LingoEngine.Texts;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotTextableMemberWindowV2 : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow, IFrameworkFor<DirectorTextEditWindowV2>
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly DirectorTextEditWindowV2 _editor;

        public DirGodotTextableMemberWindowV2(IDirectorEventMediator mediator, DirectorTextEditWindowV2 editor, IServiceProvider serviceProvider)
            : base("Edit Text", serviceProvider)
        {
            _mediator = mediator;
            _editor = editor;
            _mediator.Subscribe(this);
            Init(editor);
        }

        public void MemberSelected(ILingoMember member)
        {
            if (member is ILingoMemberTextBase textMember)
                _editor.SetMemberValues(textMember);
        }

        protected override void Dispose(bool disposing)
        {
            _mediator.Unsubscribe(this);
            base.Dispose(disposing);
        }
    }
}
