
namespace BlingoEngine.Texts
{
    /// <summary>
    /// Lingo Member Field interface.
    /// </summary>
    public interface IBlingoMemberField : IBlingoMemberTextBase
    {


        /// Returns TRUE if the field is currently focused (has keyboard input).
        /// </summary>
        bool IsFocused { get; }



    }

}




