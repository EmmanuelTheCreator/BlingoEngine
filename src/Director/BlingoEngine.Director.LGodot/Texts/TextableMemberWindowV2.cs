using System;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.Texts;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Members;
using BlingoEngine.Texts;

namespace BlingoEngine.Director.LGodot.Casts
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

        public void MemberSelected(IBlingoMember member)
        {
            if (member is IBlingoMemberTextBase textMember)
                _editor.SetMemberValues(textMember);
        }

        protected override void Dispose(bool disposing)
        {
            _mediator.Unsubscribe(this);
            base.Dispose(disposing);
        }
    }
}

