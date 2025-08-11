namespace LingoEngine.Members;

/// <summary>
/// Represents an object that holds a reference to a <see cref="LingoMember"/>.
/// When the referenced member is removed, <see cref="MemberHasBeenRemoved"/> is invoked
/// to allow the user to clean up its reference.
/// </summary>
public interface IMemberRefUser
{
    /// <summary>
    /// Called when the referenced member has been removed or disposed.
    /// Implementations should clear their reference to the member.
    /// </summary>
    void MemberHasBeenRemoved();
}
