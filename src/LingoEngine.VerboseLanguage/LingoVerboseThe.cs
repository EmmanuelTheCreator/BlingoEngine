using AbstUI.Texts;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Texts;

namespace LingoEngine.VerboseLanguage
{
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
        ILingoVerboseTheOfMember<string> Text { get; }
        #endregion

        #region Text Members
        ILingoVerboseTheOfMember<AbstTextAlignment> Alignment { get; }
        #endregion

    }
   
    

    /// <inheritdoc/>
    public partial record LingoVerboseThe : LingoVerboseBase, ILingoVerboseThe
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
        public ILingoVerboseTheOfMember<string> Text => new LingoTheTargetMember<ILingoMemberTextBase, string>(_player, s => s.Text,  (s, v) => s.Text = v);
        public ILingoVerboseTheOfMember<AbstTextAlignment> Alignment => new LingoTheTargetMember<ILingoMemberTextBase,AbstTextAlignment>(_player, s => s.Alignment, (s, v) => s.Alignment = v);

        #endregion
    }

}



