using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Holds the decoded resource metadata gathered from the <c>mmap</c> table and Afterburner <c>ABMP</c> stream.
/// Entries map resource IDs to their tags, offsets, and storage descriptors while inline segments preserve the byte payloads
/// of resources that live entirely inside the decompressed initial load segment.
/// </summary>
public sealed class BlResourceContainer
{
    private readonly List<BlLegacyResourceEntry> _entries = new();
    private readonly Dictionary<int, BlLegacyResourceEntry> _byId = new();
    private readonly Dictionary<int, byte[]> _inlineSegments = new();
    private readonly Dictionary<int, List<BlResourceKeyLink>> _childrenByParent = new();
    private readonly Dictionary<int, BlResourceKeyLink> _parentByChild = new();

    public IReadOnlyList<BlLegacyResourceEntry> Entries => _entries;

    public IReadOnlyDictionary<int, BlLegacyResourceEntry> EntriesById => _byId;

    public IReadOnlyDictionary<int, byte[]> InlineSegments => _inlineSegments;

    public IReadOnlyDictionary<int, List<BlResourceKeyLink>> ChildrenByParent => _childrenByParent;

    public IReadOnlyDictionary<int, BlResourceKeyLink> ParentByChild => _parentByChild;

    public void Reset()
    {
        _entries.Clear();
        _byId.Clear();
        _inlineSegments.Clear();
        _childrenByParent.Clear();
        _parentByChild.Clear();
    }

    /// <summary>
    /// Adds a resource entry parsed from either <c>mmap</c> or <c>ABMP</c> metadata.
    /// </summary>
    /// <param name="entry">Resource description decoded from the archive bytes.</param>
    public void Add(BlLegacyResourceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
        _byId[entry.Id] = entry;
    }

    /// <summary>
    /// Attempts to retrieve a resource entry by its numeric identifier.
    /// </summary>
    /// <param name="id">Identifier assigned to the resource in the map.</param>
    /// <param name="entry">When the method returns, contains the located entry.</param>
    /// <returns><c>true</c> when an entry is registered for the supplied id; otherwise, <c>false</c>.</returns>
    public bool TryGetEntry(int id, [NotNullWhen(true)] out BlLegacyResourceEntry? entry)
    {
        if (_byId.TryGetValue(id, out var value))
        {
            entry = value;
            return true;
        }

        entry = null;
        return false;
    }

    /// <summary>
    /// Stores the raw payload bytes for a resource that lives in the inline segment table.
    /// </summary>
    /// <param name="resourceId">Identifier of the resource whose bytes are stored inline.</param>
    /// <param name="data">Raw payload extracted from the inline segment buffer.</param>
    public void SetInlineSegment(int resourceId, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _inlineSegments[resourceId] = data;
    }

    /// <summary>
    /// Attempts to retrieve the inline payload bytes for a resource with a negative offset.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the inline resource.</param>
    /// <param name="data">When the method returns, contains the stored inline payload.</param>
    /// <returns><c>true</c> when inline data is registered for the resource; otherwise, <c>false</c>.</returns>
    public bool TryGetInlineSegment(int resourceId, [NotNullWhen(true)] out byte[]? data)
    {
        if (_inlineSegments.TryGetValue(resourceId, out var value))
        {
            data = value;
            return true;
        }

        data = null;
        return false;
    }

    /// <summary>
    /// Records the relationship between a parent resource and a child referenced in the <c>KEY*</c> table.
    /// </summary>
    /// <param name="link">Relationship describing the parent and child IDs.</param>
    public void AddRelationship(BlResourceKeyLink link)
    {
        if (!_childrenByParent.TryGetValue(link.ParentId, out var children))
        {
            children = new List<BlResourceKeyLink>();
            _childrenByParent[link.ParentId] = children;
        }

        children.Add(link);
        _parentByChild[link.ChildId] = link;
    }
}
