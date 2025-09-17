using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Events
{
    public interface IHasFindMemberEvent
    {
        void FindMember(IBlingoMember member);
    }
}

