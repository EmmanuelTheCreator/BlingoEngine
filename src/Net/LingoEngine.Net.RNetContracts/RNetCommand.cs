namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Base type for commands sent over the network.
/// </summary>
public abstract record RNetCommand : IRNetCommand;

/// <summary>Sets a sprite property.</summary>
/// <param name="SpriteNum">Sprite channel number.</param>
/// <param name="BeginFrame">Sprite begin frame.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
public sealed record SetSpritePropCmd(int SpriteNum, int BeginFrame, string Prop, string Value) : RNetCommand;

/// <summary>Sets a cast member property.</summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
public sealed record SetMemberPropCmd(int CastLibNum, int MemberNum, string Prop, string Value) : RNetCommand;

/// <summary>Changes playback to a specific frame.</summary>
/// <param name="Frame">Target frame number.</param>
public sealed record GoToFrameCmd(int Frame) : RNetCommand;

/// <summary>Pauses playback.</summary>
public sealed record PauseCmd() : RNetCommand;

/// <summary>Resumes playback.</summary>
public sealed record ResumeCmd() : RNetCommand;
