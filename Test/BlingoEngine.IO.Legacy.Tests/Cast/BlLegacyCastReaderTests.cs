using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;
using Xunit;

using BlingoEngine.IO.Legacy;
using BlingoEngine.IO.Legacy.Cast;

namespace BlingoEngine.IO.Legacy.Tests.Cast;

public class BlLegacyCastReaderTests
{
    [Fact]
    public void ReadDirFileWithThreeSounds_RegistersCastEntries()
    {
        using var harness = LegacyContextHarness.Open("WillMoveToOwnRepo/ProjectorRays/Test/TestData/Sounds/DirFileWith_3_Sounds.dir");
        harness.ReadResources();

        harness.Context.Resources.Entries.Should().Contain(entry => entry.Tag == BlTag.CasStar);

        var libraries = harness.Context.ReadCastLibraries();
        var soundLibrary = libraries.First(library => library.Members.Any(member => member.ResourceId == 4));

        soundLibrary.EntryCount.Should().Be(4);
        soundLibrary.Members.Should().HaveCount(3);
        soundLibrary.Members.Select(member => member.ResourceId).Should().BeEquivalentTo(new[] { 4, 5, 6 });
    }

    [Fact]
    public void ReadDirFileWithThreeSounds_ReadsMemberNames()
    {
        var libraries = LoadCastLibraries("WillMoveToOwnRepo/ProjectorRays/Test/TestData/Sounds/DirFileWith_3_Sounds.dir");
        var soundLibrary = libraries.First(library => library.Members.Any(member => member.ResourceId == 4));

        soundLibrary.Members.Select(member => member.Name)
            .Should()
            .Contain(new[] { "level_up", "go", "blockfall_1" });

        soundLibrary.Members.Select(member => member.MemberType)
            .Should()
            .AllBeEquivalentTo(BlLegacyCastMemberType.Sound);
    }

    [Fact]
    public void ReadFieldCast_ReadsMemberName()
    {
        var libraries = LoadCastLibraries("WillMoveToOwnRepo/ProjectorRays/Test/TestData/Texts_Fields/Field_Hallo_3lines.cst");
        var fieldLibrary = libraries.First();

        fieldLibrary.Members.Should().ContainSingle();
        var member = fieldLibrary.Members[0];

        member.Name.Should().Be("My field");
        member.MemberType.Should().BeOneOf(BlLegacyCastMemberType.Field, BlLegacyCastMemberType.Text);
    }

    [Fact]
    public void ReaderContext_Dispose_ClosesUnderlyingStream()
    {
        var stream = new TrackingStream();

        using (var context = new ReaderContext(stream, "mock.dir", leaveOpen: false))
        {
            context.Should().NotBeNull();
        }

        stream.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ReaderContext_LeaveOpen_DoesNotDisposeStream()
    {
        var stream = new TrackingStream();

        using (var context = new ReaderContext(stream, "mock.dir", leaveOpen: true))
        {
            context.Should().NotBeNull();
        }

        stream.IsDisposed.Should().BeFalse();
        stream.Dispose();
    }

    private static IReadOnlyList<BlLegacyCastLibrary> LoadCastLibraries(string relativePath)
    {
        using var harness = LegacyContextHarness.Open(relativePath);
        harness.ReadResources();
        var libraries = harness.Context.ReadCastLibraries();
        return libraries;
    }

    private sealed class LegacyContextHarness : IDisposable
    {
        private LegacyContextHarness(ReaderContext context)
        {
            Context = context;
        }

        public ReaderContext Context { get; }

        public static LegacyContextHarness Open(string relativePath)
        {
            var fullPath = ResolveTestAssetPath(relativePath);
            var stream = File.OpenRead(fullPath);
            var context = new ReaderContext(stream, Path.GetFileName(fullPath), leaveOpen: false);
            return new LegacyContextHarness(context);
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

    private sealed class TrackingStream : MemoryStream
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }

    private static string ResolveTestAssetPath(string relativePath)
    {
        var root = GetRepositoryRoot();
        var normalized = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(root, normalized);
    }

    private static string GetRepositoryRoot()
    {
        var baseDirectory = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", ".."));
    }
}
