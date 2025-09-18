using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Base class for legacy Director file readers. The derived types share the same parsing pipeline so they simply expose the
/// reader context through this base class.
/// </summary>
public abstract class BlLegacyFileResourceBase : BlLegacyResourceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyFileResourceBase"/> class.
    /// </summary>
    protected BlLegacyFileResourceBase(ReaderContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Reads the container and converts the resources into <see cref="DirFileResourceDTO"/> instances.
    /// </summary>
    /// <returns>A DTO container populated with the exported resource bytes.</returns>
    public DirFilesContainerDTO Read()
    {
        return Context.ReadDirFilesContainer();
    }
}
