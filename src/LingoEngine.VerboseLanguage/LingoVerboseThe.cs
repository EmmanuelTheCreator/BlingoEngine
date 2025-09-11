using AbstUI.Texts;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Texts;

namespace LingoEngine.VerboseLanguage
{
    public interface ILingoVerbosePropAccess<T>
    {
        T Value { get; set; }
        void To(T value);
    }
    /// <summary>
    /// the visibility of sprite 5
    /// </summary>
    public interface ILingoVerboseThe
    {
        #region Sprites

        ILingoVerboseTheOfSprite<bool> Visibility { get; }
        ILingoVerboseTheOfSprite<bool> Puppet { get; }
        ILingoVerboseTheOfSprite<float> LocH { get; }
        ILingoVerboseTheOfSprite<float> LocV { get; }
        ILingoVerboseTheOfSprite<int> LocZ { get; }
        ILingoVerboseTheOfSprite<float> Blend { get; } 
        ILingoVerboseTheOfSprite<float> WidthSprite { get; } 
        ILingoVerboseTheOfSprite<float> HeightSprite { get; }
        #endregion

        #region Members
        ILingoVerboseTheOfMember<int> WidthMember { get; }
        ILingoVerboseTheOfMember<int> HeightMember { get; }
        #endregion

        #region Text Members
        ILingoVerboseTheOfMember<AbstTextAlignment> Alignment { get; }
        #endregion

    }
    public interface ILingoVerboseTheOfMember<T>
    {
        ILingoTheTargetMember<T> Of { get; }
    }
    public interface ILingoVerboseTheOfSprite<T>
    {
        ILingoTheTargetSprite<T> Of { get; }

    }
    public interface ILingoTheTargetMember<T>
    {
        ILingoVerbosePropAccess<T> Member(string memberName, string? castlibName = null);
        ILingoVerbosePropAccess<T> Member(string memberName, int castlib);
        ILingoVerbosePropAccess<T> Member(int numberInCast, int castlib);
    }
    public interface ILingoTheTargetSprite<T>
    {
        ILingoVerbosePropAccess<T> Sprite(int number);
    }


    /// <inheritdoc/>
    public record LingoVerboseThe : LingoVerboseBase, ILingoVerboseThe
    {


        public LingoVerboseThe(LingoPlayer lingoPlayer)
            : base(lingoPlayer)
        {
        }
        #region Sprites

        public ILingoVerboseTheOfSprite<bool> Visibility => new LingoTheTargetSprite<bool>(_player, s => s.Visibility, (s,v) => s.Visibility = v);
        public ILingoVerboseTheOfSprite<bool> Puppet => new LingoTheTargetSprite<bool>(_player, s => s.Visibility, (s,v) => s.Visibility = v);
        public ILingoVerboseTheOfSprite<float> LocH => new LingoTheTargetSprite<float>(_player, s => s.LocH, (s,v) => s.LocH = v);
        public ILingoVerboseTheOfSprite<float> LocV => new LingoTheTargetSprite<float>(_player, s => s.LocV, (s,v) => s.LocV = v);
        public ILingoVerboseTheOfSprite<int> LocZ => new LingoTheTargetSprite<int>(_player, s => s.LocZ, (s,v) => s.LocZ = v);
        public ILingoVerboseTheOfSprite<float> Blend => new LingoTheTargetSprite<float>(_player, s => s.Blend, (s,v) => s.Blend = v);
        public ILingoVerboseTheOfSprite<float> WidthSprite => new LingoTheTargetSprite<float>(_player, s => s.Width, (s,v) => s.Width = v);
        public ILingoVerboseTheOfSprite<float> HeightSprite => new LingoTheTargetSprite<float>(_player, s => s.Height, (s,v) => s.Height = v);

        #endregion



        #region Members
        public ILingoVerboseTheOfMember<int> WidthMember => new LingoTheTargetMember<ILingoMember, int>(_player, s => s.Width,(s, v) => s.Width = v);
        public ILingoVerboseTheOfMember<int> HeightMember => new LingoTheTargetMember<ILingoMember, int>(_player, s => s.Height,  (s, v) => s.Height = v);
        #endregion

        #region Text Members
        public ILingoVerboseTheOfMember<AbstTextAlignment> Alignment => new LingoTheTargetMember<ILingoMemberTextBase,AbstTextAlignment>(_player, s => s.Alignment, (s, v) => s.Alignment = v);
        #endregion

        public record LingoTheTargetSprite<TValue> : ILingoTheTargetSprite<TValue> , ILingoVerboseTheOfSprite<TValue> , ILingoVerbosePropAccess<TValue>
        {
            private readonly LingoPlayer _lingoPlayer;
            private readonly Func<ILingoSprite, TValue> _actionGet;
            private readonly Action<ILingoSprite, TValue> _actionSet;
            private ILingoSpriteChannel? _sprite;

            public LingoTheTargetSprite(LingoPlayer lingoPlayer, Func<ILingoSprite, TValue> actionGet, Action<ILingoSprite, TValue> actionSet)
            {
                // we could use an expsression tree but this is slower in performance.
                _lingoPlayer = lingoPlayer;
                _actionGet = actionGet;
                _actionSet = actionSet;
            }

            public ILingoTheTargetSprite<TValue> Of => this;

            public TValue Value
            {
                get
                {
                    if (_sprite == null) return default!;
                    return _actionGet(_sprite);
                }
                set
                {
                    if (_sprite == null) return;
                    _actionSet(_sprite, value);
                }
            }

            public ILingoVerbosePropAccess<TValue> Sprite(int number)
            {
                _sprite = _lingoPlayer.ActiveMovie?.GetActiveSprite(number);
                return this;
            }

            public void To(TValue value) => Value = value;
        }


        public record LingoTheTargetMember<TMember, TValue> : LingoVerboseBase, ILingoTheTargetMember<TValue> , ILingoVerboseTheOfMember<TValue>, ILingoVerbosePropAccess<TValue>
            where TMember : ILingoMember
        {
            private readonly LingoPlayer _lingoPlayer;
            private readonly Func<TMember, TValue> _actionGet;
            private readonly Action<TMember, TValue> _actionSet;
            private TMember? _member;

            public LingoTheTargetMember(LingoPlayer lingoPlayer, Func<TMember, TValue> actionGet, Action<TMember, TValue> actionSet)
                :base(lingoPlayer)
            {
                // we could use an expsression tree but this is slower in performance.
                _lingoPlayer = lingoPlayer;
                _actionGet = actionGet;
                _actionSet = actionSet;
            }


            //public TValue Value { get => DoOnMember(memberName, castlibName, _action); set => DoOnMember(memberName, castlibName, _action); }

            public ILingoTheTargetMember<TValue> Of => this;

            public TValue Value
            {
                get
                {
                    if (_member == null) return default!;
                    return _actionGet(_member);
                }
                set
                {
                    if (_member == null) return;
                    _actionSet(_member, value);
                }
            }

            public ILingoVerbosePropAccess<TValue> Member(string memberName, string? castlibName = null)
            {
                _member = !string.IsNullOrWhiteSpace(castlibName)
                    ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                    : _player.CastLibs.GetMember<TMember>(memberName);
                return this;
            }

            public ILingoVerbosePropAccess<TValue> Member(string memberName, int castlib)
            {
                _member = _lingoPlayer.CastLib(castlib).GetMember<TMember>(memberName);
                return this;
            }

            public ILingoVerbosePropAccess<TValue> Member(int numberInCast, int castlib)
            {
                _member = _lingoPlayer.CastLib(castlib).GetMember<TMember>(numberInCast);
                return this;
            }

            public void To(TValue value) => Value = value;

        }
    }

}



