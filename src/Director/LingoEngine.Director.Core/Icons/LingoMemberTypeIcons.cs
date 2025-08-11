using LingoEngine.Bitmaps;
using LingoEngine.ColorPalettes;
using LingoEngine.FilmLoops;
using LingoEngine.Members;
using LingoEngine.Scripts;
using LingoEngine.Shapes;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using LingoEngine.Transitions;

namespace LingoEngine.Director.Core.Icons;

/// <summary>
/// Helper to map cast member types to their representative editor icons.
/// </summary>
public static class LingoMemberTypeIcons
{
    public static DirectorIcon? GetIconType(ILingoMember member)
    {
        return member switch
        {
            LingoMemberBitmap => DirectorIcon.MemberTypeBitmap,
            LingoMemberSound => DirectorIcon.MemberTypSound,
            LingoMemberField => DirectorIcon.MemberTypeField,
            ILingoMemberTextBase => DirectorIcon.MemberTypeText,
            LingoMemberShape => DirectorIcon.MemberTypeShape,
            LingoFilmLoopMember => DirectorIcon.MemberTypeMovieClip, 
            LingoColorPaletteMember => DirectorIcon.ColorPalette, 
            LingoTransitionMember => DirectorIcon.Transition,
            LingoMemberScript script => 
                script.ScriptType switch {
                    LingoScriptType.Parent => DirectorIcon.ParentScript,
                    LingoScriptType.Behavior=> DirectorIcon.BehaviorScript,
                    LingoScriptType.Movie=> DirectorIcon.MovieScript,
                    _ => null
                },
            _ when member.Type == LingoMemberType.Shape || member.Type == LingoMemberType.VectorShape => DirectorIcon.MemberTypeShape,
            _ => null
        };
    }
}
