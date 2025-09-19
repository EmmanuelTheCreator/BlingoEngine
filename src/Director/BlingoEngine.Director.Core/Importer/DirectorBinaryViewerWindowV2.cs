using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;
using System;
using System.IO;
using System.Numerics;

namespace BlingoEngine.Director.Core.Importer;

/// <summary>
/// Minimal binary viewer window that exposes the loaded bytes and optional annotations.
/// </summary>
public class DirectorBinaryViewerWindowV2 : DirectorWindow<IDirFrameworkBinaryViewerWindowV2>
{
    /// <summary>Gets the raw bytes associated with the currently loaded file.</summary>
    public byte[]? Data { get; private set; }

    /// <summary>Gets the annotation container associated with the loaded data.</summary>
    public BinaryAnnotationSet Annotations { get; } = new();

    public DirectorBinaryViewerWindowV2(IServiceProvider serviceProvider, IBlingoFrameworkFactory factory)
        : base(serviceProvider, DirectorMenuCodes.BinaryViewerWindowV2)
    {
        Width = 1400;
        Height = 600;
        MinimumWidth = 200;
        MinimumHeight = 150;
        X = 20;
        Y = 280;
    }

    public void LoadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Binary viewer file '{path}' was not found.", path);

        Data = File.ReadAllBytes(path);
        Annotations.Annotations.Clear();
        Annotations.StreamOffsetBase = 0;
    }
}
