using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Tools
{
    public interface IDirectorEventSubscription
    {
        void Release();
    }
    public interface IDirectorEventMediator
    {
        IDirectorEventSubscription Subscribe<T>(DirectorEventType eventType, Func<T?, bool> action);
        IDirectorEventSubscription Subscribe(DirectorEventType eventType, Func<bool> action);
        void Subscribe(object listener);
        void Unsubscribe(object listener);
        void RaiseSpriteSelected(IBlingoSpriteBase sprite);
        void RaiseMemberSelected(IBlingoMember member);
        void RaiseFindMember(IBlingoMember member);
        void Raise(DirectorEventType eventType, object? userData = null);
        
    }
    public enum DirectorEventType
    {
        StagePropertiesChanged,
        CastPropertiesChanged,
        SpriteSelected,
        MemberSelected,
    }
    internal class DirectorEventMediator : IDirectorEventMediator
    {
        private readonly List<IHasSpriteSelectedEvent> _spriteSelected = new();
        private readonly List<IHasMemberSelectedEvent> _membersSelected = new();
        private readonly List<IHasFindMemberEvent> _findMemberEvents = new();

        private readonly Dictionary<DirectorEventType, List<EventTypedSubscription>> _subscriptions = new();

        public IDirectorEventSubscription Subscribe<T>(DirectorEventType eventType, Func<T?, bool> action)
        {
            var subscription = new EventTypedSubscription(eventType, (u) => action(u != null?(T)u : default), (x) => _subscriptions[eventType].Remove(x));
            return Subscribe(subscription);
        }
        public IDirectorEventSubscription Subscribe(DirectorEventType eventType, Func<bool> action)
        {
            var subscription = new EventTypedSubscription(eventType,(u) => action(), (x) => _subscriptions[eventType].Remove(x));
            return Subscribe(subscription);
        }
        private IDirectorEventSubscription Subscribe(EventTypedSubscription subscription)
        { 
            if (!_subscriptions.ContainsKey(subscription.Code))
                _subscriptions[subscription.Code] = new List<EventTypedSubscription>();
            _subscriptions[subscription.Code].Add(subscription);
            return subscription;
        }
        public void Subscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected) _spriteSelected.Add(spriteSelected);
            if (listener is IHasMemberSelectedEvent memberSelected) _membersSelected.Add(memberSelected);
            if (listener is IHasFindMemberEvent findMember)
                _findMemberEvents.Add(findMember);
        }

        public void Unsubscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected)
                _spriteSelected.Remove(spriteSelected);
            if (listener is IHasMemberSelectedEvent memberSelected)
                _membersSelected.Remove(memberSelected);
            if (listener is IHasFindMemberEvent findMember)
                _findMemberEvents.Remove(findMember);
        }

        public void RaiseSpriteSelected(IBlingoSpriteBase sprite)
        {
            _spriteSelected.ForEach(x => x.SpriteSelected(sprite));
            Raise(DirectorEventType.SpriteSelected, sprite);
        }

        public void RaiseMemberSelected(IBlingoMember member)
        {
            _membersSelected.ForEach(x => x.MemberSelected(member));
            Raise(DirectorEventType.MemberSelected, member);
        }

        public void RaiseFindMember(IBlingoMember member)
            => _findMemberEvents.ForEach(x => x.FindMember(member));

        public void Raise(DirectorEventType eventType, object? userData = null)
        {
            if (!_subscriptions.TryGetValue(eventType, out var subscriptions))
                return;
            foreach (var subscription in subscriptions)
            {
                var result = subscription.Do(userData);
                if (!result) break;
            }
        }

        private class EventSubscription : IDirectorEventSubscription
        {
            private readonly Action<EventSubscription> _onRelease;
            private readonly Func<bool> _action;

            public string Code { get; }

            public EventSubscription(string code, Func<bool> action, Action<EventSubscription> onRelease)
            {
                Code = code;
                _action = action;
                _onRelease = onRelease;
            }

            public void Do() => _action();
            public void Release() => _onRelease(this);
        }
        private class EventTypedSubscription : IDirectorEventSubscription
        {
            private readonly Func<object?, bool> _action;
            private readonly Action<EventTypedSubscription> _onRelease;

            public DirectorEventType Code { get; }

            public EventTypedSubscription(DirectorEventType code, Func<object?, bool> action, Action<EventTypedSubscription> onRelease)
            {
                Code = code;
                _action = action;
                _onRelease = onRelease;
            }

            public bool Do(object? userData) => _action(userData);
            public void Release() => _onRelease(this);
        }
    }

   
}

