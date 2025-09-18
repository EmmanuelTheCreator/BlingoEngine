using ProjectorRays.Common;

namespace ProjectorRays.Director;

public enum RaysMemberType
{
    NullMember = 0,
    BitmapMember = 1,
    FilmLoopMember = 2,
    TextMember = 3,
    PaletteMember = 4,
    PictureMember = 5,
    SoundMember = 6,
    ButtonMember = 7,
    ShapeMember = 8,
    MovieMember = 9,
    DigitalVideoMember = 10,
    ScriptMember = 11,
    RTEMember = 12,
    FontMember = 13,        // some references use 13
    XrayMember = 14,        // internal metadata
    FieldMember = 15,
}

public class RaysCastMember
{
    public RaysDirectorFile? Dir;
    public RaysMemberType Type;

    public RaysCastMember(RaysDirectorFile? dir, RaysMemberType type)
    {
        Dir = dir;
        Type = type;
    }

    public virtual void Read(ReadStream stream) { }

    public virtual void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.EndObject();
    }
}

public enum RaysScriptType
{
    ScoreScript = 1,
    MovieScript = 3,
    ParentScript = 7
}

public class RaysScriptMember : RaysCastMember
{
    public RaysScriptType ScriptType;

    public RaysScriptMember(RaysDirectorFile? dir) : base(dir, RaysMemberType.ScriptMember) {}

    public override void Read(ReadStream stream)
    {
        ScriptType = (RaysScriptType)stream.ReadUint16();
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("scriptType", (int)ScriptType);
        json.EndObject();
    }
}

public readonly record struct RaysQuickDrawRect(short Top, short Left, short Bottom, short Right)
{
    public short Width => (short)(Right - Left);
    public short Height => (short)(Bottom - Top);
}

public class RaysShapeMember : RaysCastMember
{
    private const int _shapeRecordLength = 17;

    public ushort ShapeType { get; private set; }

    public RaysQuickDrawRect InitialRect { get; private set; }

    public ushort PatternId { get; private set; }

    public byte ForegroundColor { get; private set; }

    public byte BackgroundColor { get; private set; }

    public byte FillType { get; private set; }

    public byte Ink => (byte)(FillType & 0x3F);

    public byte LineThickness { get; private set; }

    public byte LineDirection { get; private set; }

    public bool IsStub { get; private set; }

    public RaysShapeMember(RaysDirectorFile? dir)
        : base(dir, RaysMemberType.ShapeMember)
    {
    }

    public override void Read(ReadStream stream)
    {
        if (stream.BytesLeft < _shapeRecordLength)
        {
            ApplyStubDefaults();
            return;
        }

        ShapeType = stream.ReadUint16();
        short top = stream.ReadInt16();
        short left = stream.ReadInt16();
        short bottom = stream.ReadInt16();
        short right = stream.ReadInt16();
        InitialRect = new RaysQuickDrawRect(top, left, bottom, right);

        PatternId = stream.ReadUint16();

        var directorVersion = Dir?.Version ?? 0;

        if (directorVersion != 0 && directorVersion < 0x400)
        {
            ForegroundColor = NormalizeSignedColor(stream.ReadInt8());
            BackgroundColor = NormalizeSignedColor(stream.ReadInt8());
        }
        else
        {
            ForegroundColor = stream.ReadUint8();
            BackgroundColor = stream.ReadUint8();
        }

        FillType = stream.ReadUint8();
        LineThickness = stream.ReadUint8();
        LineDirection = stream.ReadUint8();

        if (directorVersion >= 0x1100)
        {
            // Director 11+ shapes are not fully documented yet.
            // Mark the record so callers can choose to treat the data cautiously.
            IsStub = false;
        }
    }

    private static byte NormalizeSignedColor(sbyte value)
        => (byte)((value + 128) & 0xFF);

    private void ApplyStubDefaults()
    {
        ShapeType = 0;
        InitialRect = new RaysQuickDrawRect(0, 0, 0, 0);
        PatternId = 0;
        ForegroundColor = 0;
        BackgroundColor = 0;
        FillType = 0;
        LineThickness = 1;
        LineDirection = 0;
        IsStub = true;
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("shapeType", ShapeType);
        json.WriteField("patternId", PatternId);
        json.WriteField("foregroundColor", ForegroundColor);
        json.WriteField("backgroundColor", BackgroundColor);
        json.WriteField("fillType", FillType);
        json.WriteField("lineThickness", LineThickness);
        json.WriteField("lineDirection", LineDirection);
        json.WriteField("ink", Ink);
        json.WriteKey("initialRect");
        json.StartObject();
        json.WriteField("top", InitialRect.Top);
        json.WriteField("left", InitialRect.Left);
        json.WriteField("bottom", InitialRect.Bottom);
        json.WriteField("right", InitialRect.Right);
        json.EndObject();
        json.EndObject();
    }
}

