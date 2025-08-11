using LingoEngine.Bitmaps;
using LingoEngine.ColorPalettes;
using LingoEngine.FilmLoops;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Scripts;
using LingoEngine.Shapes;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using LingoEngine.Transitions;
using System;
using System.Linq;

namespace LingoEngine.Casts
{
    public class CastMemberSelection
    {
        // Todo
    }

    /// <inheritdoc/>
    public class LingoCast : ILingoCast
    {

        private readonly LingoCastLibsContainer _castLibsContainer;
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoMembersContainer _MembersContainer = new(false);

        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public string FileName { get; set; } = "";
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public PreLoadModeType PreLoadMode { get; set; } = PreLoadModeType.WhenNeeded;
        /// <inheritdoc/>
        public bool IsInternal { get; }
        /// <inheritdoc/>
        public CastMemberSelection? Selection { get; set; } = null;

        public ILingoMembersContainer Member => _MembersContainer;

        public event Action<ILingoMember>? MemberAdded { add => _MembersContainer.MemberAdded +=value; remove => _MembersContainer.MemberAdded -= value; }
        public event Action<ILingoMember>? MemberDeleted { add => _MembersContainer.MemberDeleted += value; remove => _MembersContainer.MemberDeleted -= value; }
        public event Action<ILingoMember>? MemberNameChanged;
        

        internal LingoCast(LingoCastLibsContainer castLibsContainer, ILingoFrameworkFactory factory, string name, bool isInternal)
        {
            _castLibsContainer = castLibsContainer;
            _factory = factory;
            Name = name;
            Number = castLibsContainer.GetNextCastNumber();
            IsInternal = isInternal;
        }

        /// <inheritdoc/>
        public T? GetMember<T>(int number) where T : class, ILingoMember => _MembersContainer[number] as T;
        /// <inheritdoc/>
        public T? GetMember<T>(string name) where T : class, ILingoMember => _MembersContainer[name] as T;
        /// <inheritdoc/>
        internal ILingoCast Add(LingoMember member)
        {
#if DEBUG
            if (member.Name.Contains("blokred") || member.NumberInCast == 30)
            {

            }
#endif
            _castLibsContainer.AddMember(member);
            _MembersContainer.Add(member);
            return this;
        }
        public ILingoCast Remove(LingoMember member)
        {
            member.Dispose();

            _castLibsContainer.RemoveMember(member);
            _MembersContainer.Remove(member);
            return this;
        }
        internal void MemberNameHasChanged(string oldName, LingoMember member)
        {
            _castLibsContainer.MemberNameChanged(oldName, member);
            _MembersContainer.MemberNameChanged(oldName, member);
            MemberNameChanged?.Invoke(member);
        }
        /// <inheritdoc/>
        public int FindEmpty() => _MembersContainer.FindEmpty();
        internal int GetUniqueNumber(int numberInCast)
        {
            //if (Number == 1 ? ((member.CastLibNum - 1) * 131114 : numberInCast : _cast.GetUniqueNumber();
            return _castLibsContainer.GetNextMemberNumber(Number, numberInCast);
        }

        internal void RemoveAll()
        {
            var allMembers = _MembersContainer.All;
            foreach (var member in allMembers)
                Remove(member);
        }

        public ILingoMember Add(LingoMemberType type, int numberInCast, string name, string fileName = "", LingoPoint regPoint = default)
        {
            switch (type)
            {
                case LingoMemberType.Bitmap: return _factory.CreateMemberBitmap(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.Sound: return _factory.CreateMemberSound(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.FilmLoop: return _factory.CreateMemberFilmLoop(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.Text: return _factory.CreateMemberText(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.Field: return _factory.CreateMemberField(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.Shape: return _factory.CreateMemberShape(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.Script: return _factory.CreateScript(this, numberInCast, name, fileName, regPoint);
                case LingoMemberType.Palette: return new LingoColorPaletteMember(this, numberInCast, name);
                case LingoMemberType.Transition: return new LingoTransitionMember(this, numberInCast, name);
                default:
                    return _factory.CreateEmpty(this, numberInCast, name, fileName, regPoint);
            }

        }
        public T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T : ILingoMember
        {
            var member = (T)Add(GetByType<T>(), numberInCast, name, "", default);
            if (configure != null)
                configure(member);
            
            return member;
        }

        private static LingoMemberType GetByType<T>()
            where T : ILingoMember
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(LingoMemberBitmap):return LingoMemberType.Bitmap;
                case Type t when t == typeof(LingoMemberSound):return LingoMemberType.Sound;
                case Type t when t == typeof(LingoFilmLoopMember):return LingoMemberType.FilmLoop;
                case Type t when t == typeof(LingoMemberText):return LingoMemberType.Text;
                case Type t when t == typeof(LingoMemberField):return LingoMemberType.Field;
                case Type t when t == typeof(LingoMemberShape):return LingoMemberType.Shape;
                case Type t when t == typeof(LingoMemberScript):return LingoMemberType.Script;
                case Type t when t == typeof(LingoColorPaletteMember):return LingoMemberType.Palette;
                case Type t when t == typeof(LingoTransitionMember):return LingoMemberType.Transition;
                default:
                    return LingoMemberType.Unknown;
            }
        }
        public IEnumerable<ILingoMember> GetAll() => _MembersContainer.All;

        public void SwapMembers(int slot1, int slot2)
        {
            if (slot1 == slot2) return;

            var tempSlot = _MembersContainer.FindEmpty();
            _MembersContainer.ChangeNumber(slot1, tempSlot);
            _MembersContainer.ChangeNumber(slot2, slot1);
            _MembersContainer.ChangeNumber(tempSlot, slot2);
        }

        public void Dispose() => RemoveAll();
    }
}
