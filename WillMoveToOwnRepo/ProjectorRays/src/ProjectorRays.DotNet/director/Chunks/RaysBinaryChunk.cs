using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class RaysBinaryChunk : RaysChunk
{
    public BufferView Data = BufferView.Empty;

    public RaysBinaryChunk(RaysDirectorFile? dir) : base(dir, ChunkType.Binary) { }

    public override void Read(ReadStream stream)
    {
        Data = stream.ReadByteView(stream.Size - stream.Pos);
    }
}
