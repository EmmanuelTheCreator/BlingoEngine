using System;
namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// A full stage frame snapshot.
/// </summary>
/// <param name="Width">Width of the frame in pixels.</param>
/// <param name="Height">Height of the frame in pixels.</param>
/// <param name="FrameId">Sequential frame identifier.</param>
/// <param name="TimestampUtc">UTC timestamp when captured.</param>
/// <param name="Argb32">Raw pixel data in ARGB32 format.</param>
public sealed record StageFrameDto(int Width, int Height, long FrameId, DateTime TimestampUtc, byte[] Argb32);

