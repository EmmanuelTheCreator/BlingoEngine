using BlingoEngine.Bitmaps;
using BlingoEngine.ColorPalettes;
using BlingoEngine.FilmLoops;
using BlingoEngine.Members;
using BlingoEngine.Scripts;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;

namespace BlingoEngine.Director.Core.Icons;

/// <summary>
/// Helper to map cast member types to their representative editor icons.
/// </summary>
public static class BlingoMemberTypeIcons
{
    public static DirectorIcon? GetIconType(IBlingoMember member)
    {
        return member switch
        {
            BlingoMemberBitmap => DirectorIcon.MemberTypeBitmap,
            BlingoMemberSound => DirectorIcon.MemberTypSound,
            BlingoMemberField => DirectorIcon.MemberTypeField,
            IBlingoMemberTextBase => DirectorIcon.MemberTypeText,
            BlingoMemberShape => DirectorIcon.MemberTypeShape,
            BlingoFilmLoopMember => DirectorIcon.MemberTypeMovieClip, 
            BlingoColorPaletteMember => DirectorIcon.ColorPalette, 
            BlingoTransitionMember => DirectorIcon.Transition,
            BlingoMemberScript script => 
                script.ScriptType switch {
                    BlingoScriptType.Parent => DirectorIcon.ParentScript,
                    BlingoScriptType.Behavior=> DirectorIcon.BehaviorScript,
                    BlingoScriptType.Movie=> DirectorIcon.MovieScript,
                    _ => null
                },
            _ when member.Type == BlingoMemberType.Shape || member.Type == BlingoMemberType.VectorShape => DirectorIcon.MemberTypeShape,
            _ => null
        };
    }
}

