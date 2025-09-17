using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Events
{
    public interface IHasMemberSelectedEvent
    {
        void MemberSelected(IBlingoMember member);
    }
}

