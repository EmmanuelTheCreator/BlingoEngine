﻿namespace BlingoEngine.L3D.Core.Primitives;

/// <summary>
/// Model resource primitive of type #cylinder.
/// </summary>
public class BlingoCylinder
{
    public float TopRadius { get; set; } = 25.0f;
    public float BottomRadius { get; set; } = 25.0f;
    public float Height { get; set; } = 50.0f;
    public bool TopCap { get; set; } = false;
    public bool BottomCap { get; set; } = false;
}

