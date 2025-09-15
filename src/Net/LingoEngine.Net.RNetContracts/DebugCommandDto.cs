namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Base type for commands sent over the network.
/// </summary>
public abstract record DebugCommandDto : INetCommand;

/// <summary>Sets a sprite property.</summary>
/// <param name="SpriteNum">Sprite channel number.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
public sealed record SetSpritePropCmd(int SpriteNum, string Prop, string Value) : DebugCommandDto;

/// <summary>Sets a cast member property.</summary>
/// <param name="MemberName">Member name.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
public sealed record SetMemberPropCmd(string MemberName, string Prop, string Value) : DebugCommandDto;

/// <summary>Changes playback to a specific frame.</summary>
/// <param name="Frame">Target frame number.</param>
public sealed record GoToFrameCmd(int Frame) : DebugCommandDto;

/// <summary>Pauses playback.</summary>
public sealed record PauseCmd() : DebugCommandDto;

/// <summary>Resumes playback.</summary>
public sealed record ResumeCmd() : DebugCommandDto;
