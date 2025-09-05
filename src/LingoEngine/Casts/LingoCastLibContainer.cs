﻿using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;

namespace LingoEngine.Casts
{
    /// <summary>
    /// Lingo Cast Libs Container interface.
    /// </summary>
    public interface ILingoCastLibsContainer
    {
        ILingoCast ActiveCast { get; set; }
        int Count { get; }
        ILingoCast this[int index] { get; }
        ILingoCast this[string name] { get; }
        /// <summary>
        /// Seacch in all members
        /// </summary>
        ILingoMembersContainer Member { get; }
        ILingoMember? GetMember(int number, int? castLibNum = null);
        ILingoMember? GetMember(string name, int? castLibNum = null);
        T? GetMember<T>(int number, int? castLibNum = null) where T : class, ILingoMember;
        T? GetMember<T>(string name, int? castLibNum = null) where T : class, ILingoMember;
        ILingoCast AddCast(string name, bool isInternal = false);
        ILingoCast GetCast(int number);
        string GetCastName(int number);
        IEnumerable<ILingoCast> GetAll();
    }


    public class LingoCastLibsContainer : ILingoCastLibsContainer
    {
        private Dictionary<string, LingoMember> _allMembersByName = new();
        private Dictionary<string, LingoCast> _castsByName = new();
        private List<LingoCast> _casts = new();
        private ILingoCast _activeCast = null!;
        private readonly LingoMembersContainer _allMembersContainer;
        private readonly ILingoFrameworkFactory _factory;

        public ILingoMembersContainer Member => _allMembersContainer;
        public int Count => _casts.Count;

        public ILingoCast ActiveCast
        {
            get => _activeCast; set
            {
                if (_casts.Contains(value))
                    _activeCast = value;
            }
        }
        public LingoCastLibsContainer(ILingoFrameworkFactory factory)
        {
            _allMembersContainer = new LingoMembersContainer(true, _allMembersByName);
            _factory = factory;
        }

        public ILingoCast this[int number] => _casts[number - 1];
        public ILingoCast this[string name] => _castsByName[name.ToLower()];
        public string GetCastName(int number) => _casts[number - 1].Name;
        public ILingoCast GetCast(int number) => _casts[number - 1];

        public ILingoCast AddCast(string name, bool isInternal = false)
        {
            var nameL = name.ToLower();
            var cast = new LingoCast(this, _factory, name, isInternal);
            _casts.Add(cast);
            _castsByName.Add(nameL, cast);
            if (ActiveCast == null)
                ActiveCast = cast;
            return cast;
        }
        public ILingoCast RemoveCast(ILingoCast cast)
        {
            var nameL = cast.Name.ToLower();
            var castTyped = (LingoCast)cast;
            castTyped.Dispose();
            _casts.Remove(castTyped);
            _castsByName.Remove(nameL);
            if (_activeCast == cast && _casts.Count > 0)
                _activeCast = _casts[0];

            return cast;
        }

        public void RemoveInternal()
        {
            for (int i = _casts.Count - 1; i >= 0; i--)
            {
                var cast = _casts[i];
                if (cast.IsInternal)
                    RemoveCast(cast);
            }
        }

        public int GetNextMemberNumber(int castNumber, int numberInCast) => _allMembersContainer.GetNextNumber(castNumber, numberInCast);
        public T? GetMember<T>(int number, int? castLibNum = null) where T : class, ILingoMember
            => !castLibNum.HasValue
             ? _allMembersContainer.Member<T>(number)
             : _casts[castLibNum.Value - 1].GetMember<T>(number);
        public ILingoMember? GetMember(int number, int? castLibNum = null)
            => !castLibNum.HasValue
             ? _allMembersContainer[number]
             : _casts[castLibNum.Value - 1].Member[number];
        public ILingoMember? GetMember(string name, int? castLibNum = null)
            => !castLibNum.HasValue
             ? _allMembersContainer[name]
             : _casts[castLibNum.Value - 1].Member[name];

        public T? GetMember<T>(string name, int? castLibNum = null) where T : class, ILingoMember
            => !castLibNum.HasValue
             ? _allMembersContainer.Member<T>(name)
             : _casts[castLibNum.Value - 1].GetMember<T>(name);

        internal void AddMember(LingoMember member)
        {
            _allMembersContainer.Add(member);
            if (!_allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Add(member.Name, member);
        }

        internal void RemoveMember(LingoMember member)
        {
            _allMembersContainer.Remove(member);
            _allMembersByName.Remove(member.Name);
        }

        internal void MemberNameChanged(string oldName, LingoMember member)
        {
            if (!string.IsNullOrWhiteSpace(oldName) && _allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Remove(member.Name);
            if (!_allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Add(member.Name, member);
            _allMembersContainer.MemberNameChanged(oldName, member);
        }

        internal int GetNextCastNumber() => _casts.Count + 1;

        public IEnumerable<ILingoCast> GetAll() => _casts;
    }
}
