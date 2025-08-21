
namespace LingoEngine.Texts
{
    /// <summary>
    /// Lingo Member Field interface.
    /// </summary>
    public interface ILingoMemberField : ILingoMemberTextBase
    {


        /// Returns TRUE if the field is currently focused (has keyboard input).
        /// </summary>
        bool IsFocused { get; }



    }

}



