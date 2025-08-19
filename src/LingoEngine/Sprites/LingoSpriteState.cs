using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Sprites;

public class LingoSpriteState
{
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Puppet { get; set; }
    public int SpriteNum { get; set; }
    public bool Lock { get; set; }
}

public class LingoSprite2DState : LingoSpriteState
{
    public LingoMember? Member { get; set; }
    public int DisplayMember { get; set; }
    public int SpritePropertiesOffset { get; set; }
    public int Ink { get; set; }
    public bool Visibility { get; set; }
    public bool Hilite { get; set; }
    public bool Linked { get; set; }
    public bool Loaded { get; set; }
    public bool MediaReady { get; set; }
    public float Blend { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int Cursor { get; set; }
    public int Constraint { get; set; }
    public bool DirectToStage { get; set; }
    public APoint RegPoint { get; set; }
    public AColor ForeColor { get; set; }
    public AColor BackColor { get; set; }
    public bool Editable { get; set; }
    public bool IsDraggable { get; set; }
}

public class LingoSprite2DVirtualState : LingoSpriteState
{
    public LingoMember? Member { get; set; }
    public int DisplayMember { get; set; }
    public int Ink { get; set; }
    public bool Hilite { get; set; }
    public bool Linked { get; set; }
    public bool Loaded { get; set; }
    public float Blend { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int Constraint { get; set; }
    public APoint RegPoint { get; set; }
    public AColor ForeColor { get; set; }
    public AColor BackColor { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class LingoSpriteSoundState : LingoSpriteState
{
    public int Channel { get; set; }
    public LingoMemberSound? Sound { get; set; }
}
