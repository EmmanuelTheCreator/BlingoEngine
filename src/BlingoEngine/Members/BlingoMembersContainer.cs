namespace BlingoEngine.Members
{
    /// <summary>
    /// Lingo Members Container interface.
    /// </summary>
    public interface IBlingoMembersContainer
    {
        /// <summary>
        /// Retrieves a cast member by its number in the library.
        /// Lingo: member x of castLib
        /// </summary>
        /// <param name="number">The cast member number (1-based).</param>
        /// <returns>The specified cast member.</returns>
        IBlingoMember? this[int number] { get; }
        IBlingoMember? this[string name] { get; }
        /// <summary>
        /// Retrieves a cast member by its number in the library.
        /// Lingo: member x of castLib
        /// </summary>
        /// <param name="number">The cast member number (1-based).</param>
        /// <returns>The specified cast member.</returns>
        T? Member<T>(int number) where T : IBlingoMember;
        T? Member<T>(string name) where T : IBlingoMember;
        event Action<IBlingoMember>? MemberAdded;
        event Action<IBlingoMember>? MemberDeleted;
    }

    internal class BlingoMembersContainer : IBlingoMembersContainer
    {
        private readonly Dictionary<int, IBlingoMember> _members = new();
        private readonly bool _containerForAll;
        private readonly Dictionary<string, IBlingoMember> _membersByName;

        public event Action<IBlingoMember>? MemberAdded;
        public event Action<IBlingoMember>? MemberDeleted;
        /// <summary>
        /// Returns a copy array
        /// </summary>
        internal IReadOnlyCollection<IBlingoMember> All => _members.Values.ToArray();
        public int Count => _members.Count;
        internal BlingoMembersContainer(bool containerForAll, Dictionary<string, IBlingoMember>? membersByName = null)
        {
            this._containerForAll = containerForAll;
            _membersByName = membersByName ?? new();
        }
        internal void Add(BlingoMember member)
        {
            if (_containerForAll && member.CastLibNum > 1)
            {
                // From the second castlib, the numbers needs to increament
                if (_members.ContainsKey(member.Number))
                    _members.Add((member.CastLibNum * 131114) + member.Number, member);
                else
                    _members.Add(member.Number, member);
            }
            else
            {
                if (member.NumberInCast == 0)
                    member.NumberInCast = FindEmpty(); // GetNextNumber(member.Cast.Number,0);
                _members[member.NumberInCast] = member;
            }
            var name = member.Name.ToLower();
            if (!_membersByName.ContainsKey(name))
                _membersByName.Add(name, member);
            MemberAdded?.Invoke(member);
        }

        internal void Remove(IBlingoMember member)
        {
            var name = member.Name.ToLower();
            if (_containerForAll)
                _members.Remove(member.Number);
            else
                _members.Remove(member.NumberInCast);
            if (_membersByName.ContainsKey(name))
                _membersByName.Remove(name);
            MemberDeleted?.Invoke(member);
        }

        internal void ChangeNumber(int oldNumber, int newNumber)
        {
            if (oldNumber == newNumber || _containerForAll)
                return;

            if (_members.TryGetValue(oldNumber, out var member))
            {
                _members.Remove(oldNumber);
                _members[newNumber] = member;
                ((BlingoMember)member).NumberInCast = newNumber;
            }
        }

        internal void MemberNameChanged(string oldName, BlingoMember member)
        {
            oldName = oldName.ToLower();
            var name = member.Name.ToLower();
            if (!string.IsNullOrWhiteSpace(oldName) && _membersByName.ContainsKey(oldName))
                _membersByName.Remove(oldName);
            if (!_membersByName.ContainsKey(name))
                _membersByName.Add(name.ToLower(), member);
        }

        public IBlingoMember? this[int number]
            => _members.TryGetValue(number, out var member) ? member : null;
        public IBlingoMember? this[string name] => _membersByName.TryGetValue(name.ToLower(), out var theValue) ? theValue : null;

        public T? Member<T>(int number) where T : IBlingoMember
        {
            if (!_members.TryGetValue(number, out IBlingoMember? member)) return default;
            if (member is T tMember) return tMember;
            return default;
        }

        public T? Member<T>(string name) where T : IBlingoMember
        {
            if (!_membersByName.TryGetValue(name.ToLower(System.Globalization.CultureInfo.CurrentCulture), out var member)) return default;
            if (member is T tMember) return tMember;
            return default;
        }

        public int GetNextNumber(int castNumber, int numberInCast)
        {
            if (castNumber > 0)
            {
                // for the first cast, numbers follow the numberInCast
                if (numberInCast <= 0)
                    return _members.Keys.Any() ? _members.Keys.Max() + 1 : 1;
                if (castNumber == 1)
                    return numberInCast;
                var startNumber = ((castNumber - 1) * 131114) + numberInCast;
                if (_members.Keys.Contains(startNumber))
                    return _members.Keys.Any() ? _members.Keys.Max() + 1 : 1;
                return startNumber;
            }
            return _members.Keys.Any() ? _members.Keys.Max() + 1 : 1;
        }

        private int _lastHighestNumber = 1;
        internal int FindEmpty()
        {
            var newNumber = 1;
            for (int i = _lastHighestNumber; i < 5000; i++)
            {
                if (!_members.ContainsKey(i))
                {
                    _lastHighestNumber = i;
                    return i;
                }
            }
            return newNumber;
        }
    }
}

