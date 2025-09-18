using System;

namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Provides shared access to the <see cref="ReaderContext"/> used to interpret byte-level Director structures.
/// </summary>
public abstract class BlLegacyResourceBase
{
    private readonly ReaderContext _context;

    /// <summary>
    /// Gets the active reader context.
    /// </summary>
    protected ReaderContext Context => _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyResourceBase"/> class.
    /// </summary>
    protected BlLegacyResourceBase(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }
}
