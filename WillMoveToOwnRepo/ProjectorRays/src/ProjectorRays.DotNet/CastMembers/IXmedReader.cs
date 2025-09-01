namespace ProjectorRays.CastMembers;

using ProjectorRays.Common;

public interface IXmedReader
{
    XmedDocument Read(BufferView view);
}
