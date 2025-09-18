using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Files;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlingoEngine.IO.Legacy.Tests.Helpers;

internal sealed class TestContextHarness : IDisposable
    {

    public static IReadOnlyList<BlLegacyCastLibrary> LoadCastLibraries(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        var libraries = harness.Context.ReadCastLibraries();
        return libraries;
    }

    private TestContextHarness(ReaderContext context)
        {
            Context = context;
        }

        public ReaderContext Context { get; }

        public static TestContextHarness Open(string relativePath)
        {
            var fullPath = TestFolder.AssetPath(relativePath);
            var stream = File.OpenRead(fullPath);
            var context = new ReaderContext(stream, Path.GetFileName(fullPath), leaveOpen: false);
            return new TestContextHarness(context);
        }

        public void ReadResources()
        {
            Context.ReadDirFilesContainer();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }

   

