using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Members;

namespace LingoEngine.Tests.Casts;

internal class DummyCast : ILingoCast
{
    // fields
    private readonly DummyMembersContainer _members = new();
    private ILingoMember? _lastAddedMember;

    // properties
    public string Name => "Dummy";
    public string FileName { get; set; } = string.Empty;
    public int Number => 1;
    public PreLoadModeType PreLoadMode { get; set; }
    public bool IsInternal => true;
    public CastMemberSelection? Selection { get; set; }
    public ILingoMembersContainer Member => _members;
    public ILingoMember? LastAddedMember => _lastAddedMember;

    // constructors
    public DummyCast() { }

    // events
    public event Action<ILingoMember>? MemberAdded;
    public event Action<ILingoMember>? MemberDeleted;
    public event Action<ILingoMember>? MemberNameChanged;

    // methods
    public void Dispose() { }
    public T? GetMember<T>(int number) where T :  ILingoMember => default;
    public T? GetMember<T>(string name) where T : ILingoMember => default;
    public int FindEmpty() => 1;
    public ILingoMember Add(LingoMemberType type, int numberInCast, string name, string fileName = "", APoint regPoint = default)
    {
        var member = new DummyTextMember(this) { Name = name };
        member.SetFileName(fileName);
        _lastAddedMember = member;
        MemberAdded?.Invoke(member);
        return member;
    }
    public T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T : ILingoMember => throw new NotImplementedException();
    public IEnumerable<ILingoMember> GetAll() => Array.Empty<ILingoMember>();
    public void SwapMembers(int slot1, int slot2) { }
    public void Save() { }

    private class DummyMembersContainer : ILingoMembersContainer
    {
        public ILingoMember? this[int number] => null;
        public ILingoMember? this[string name] => null;
        public event Action<ILingoMember>? MemberAdded;
        public event Action<ILingoMember>? MemberDeleted;
        public T? Member<T>(int number) where T : ILingoMember => default;
        public T? Member<T>(string name) where T : ILingoMember => default;
    }
}

