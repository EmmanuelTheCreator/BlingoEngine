using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstUI.Primitives;
using AbstUI.Resources;
using AbstUI.Texts;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Texts;
using LingoEngine.Tools;
using LingoEngine.Core;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using Xunit;

namespace LingoEngine.Tests;

public class CsvImporterTests
{
    [Fact]
    public void ImportCsvCastFile_ReturnsExpectedRow()
    {
        var csvContent = "Number,Type,Name,Registration Point,Filename\n" +
                         "1,Text,Sample,\"(1, 2)\",sample.txt";
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        var importer = new CsvImporter(new TestResourceManager());
        var rows = importer.ImportCsvCastFile(tempFile);

        var row = Assert.Single(rows);
        Assert.Equal(1, row.Number);
        Assert.Equal(LingoMemberType.Text, row.Type);
        Assert.Equal("Sample", row.Name);
        Assert.Equal(1, row.RegPoint.X);
        Assert.Equal(2, row.RegPoint.Y);
        Assert.Equal("sample.txt", row.FileName);
    }
    [Fact]
    public void ImportInCastFromCsvFile_PrefersMarkdown()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var csvPath = Path.Combine(tempDir, "cast.csv");
        var baseFile = Path.Combine(tempDir, "sample.txt");
        var mdPath = Path.ChangeExtension(baseFile, ".md");
        var rtfPath = Path.ChangeExtension(baseFile, ".rtf");

        File.WriteAllText(csvPath, "Number,Type,Name,Registration Point,Filename\n1,Text,Sample,\"(0, 0)\",sample.txt");
        File.WriteAllText(mdPath, "md text");
        File.WriteAllText(rtfPath, "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 rtf}} ");

        var cast = new DummyCast();
        var importer = new CsvImporter(new TestResourceManager());

        importer.ImportInCastFromCsvFile(cast, csvPath);

        var member = Assert.IsType<DummyTextMember>(cast.LastAddedMember);
        Assert.Equal("md text", member.Text);
        Assert.False(member.LoadFileCalled);
    }

#if DEBUG
    [Fact]
    public void ImportInCastFromCsvFile_CreatesMarkdownWhenReadingRtf()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var csvPath = Path.Combine(tempDir, "cast.csv");
        var baseFile = Path.Combine(tempDir, "sample.txt");
        var mdPath = Path.ChangeExtension(baseFile, ".md");
        var rtfPath = Path.ChangeExtension(baseFile, ".rtf");

        File.WriteAllText(csvPath, "Number,Type,Name,Registration Point,Filename\n1,Text,Sample,\"(0, 0)\",sample.txt");
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 Hello}}";
        File.WriteAllText(rtfPath, rtf);

        var cast = new DummyCast();
        var importer = new CsvImporter(new TestResourceManager());

        importer.ImportInCastFromCsvFile(cast, csvPath);

        var member = Assert.IsType<DummyTextMember>(cast.LastAddedMember);
        Assert.Equal("{{PARA:0}}Hello", member.Text);
        Assert.True(File.Exists(mdPath));
    }
#endif

    private class TestResourceManager : IAbstResourceManager
    {
        public bool FileExists(string fileName) => File.Exists(fileName);
        public string? ReadTextFile(string fileName) => File.Exists(fileName) ? File.ReadAllText(fileName) : null;

        public byte[]? ReadBytes(string fileName) => File.Exists(fileName) ? File.ReadAllBytes(fileName) : null;
    }

    private class DummyCast : ILingoCast
    {
        public string Name => "Dummy";
        public string FileName { get; set; } = string.Empty;
        public int Number => 1;
        public PreLoadModeType PreLoadMode { get; set; }
        public bool IsInternal => true;
        public CastMemberSelection? Selection { get; set; }
        public ILingoMembersContainer Member { get; } = new DummyMembersContainer();
        public ILingoMember? LastAddedMember { get; private set; }
        public event Action<ILingoMember>? MemberAdded;
        public event Action<ILingoMember>? MemberDeleted;
        public event Action<ILingoMember>? MemberNameChanged;
        public void Dispose() { }
        public T? GetMember<T>(int number) where T : class, ILingoMember => null;
        public T? GetMember<T>(string name) where T : class, ILingoMember => null;
        public int FindEmpty() => 1;
        public ILingoMember Add(LingoMemberType type, int numberInCast, string name, string fileName = "", APoint regPoint = default)
        {
            var member = new DummyTextMember(this) { Name = name };
            member.SetFileName(fileName);
            LastAddedMember = member;
            MemberAdded?.Invoke(member);
            return member;
        }
        public T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T : ILingoMember => throw new NotImplementedException();
        public IEnumerable<ILingoMember> GetAll() => Array.Empty<ILingoMember>();
        public void SwapMembers(int slot1, int slot2) { }
        public void Save() { }
    }

    private class DummyMembersContainer : ILingoMembersContainer
    {
        public ILingoMember? this[int number] => null;
        public ILingoMember? this[string name] => null;
        public event Action<ILingoMember>? MemberAdded;
        public event Action<ILingoMember>? MemberDeleted;
        public T? Member<T>(int number) where T : class, ILingoMember => null;
        public T? Member<T>(string name) where T : class, ILingoMember => null;
    }

    private class DummyTextMember : ILingoMemberTextBase, ILingoMemberTextBaseInteral
    {
        private readonly ILingoCast _cast;
        private readonly DummyFrameworkMember _framework = new();
        private string _fileName = string.Empty;

        public DummyTextMember(ILingoCast cast) => _cast = cast;

        public ILingoFrameworkMember FrameworkObj => _framework;
        public string Name { get; set; } = string.Empty;
        public int Number => 0;
        public DateTime CreationDate => DateTime.Now;
        public DateTime ModifiedDate => DateTime.Now;
        public bool Hilite => false;
        public int CastLibNum => 0;
        public int NumberInCast => 0;
        public int PurgePriority { get; set; }
        public APoint RegPoint { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Size { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string FileName => _fileName;
        public LingoMemberType Type { get; set; } = LingoMemberType.Text;
        public string CastName => string.Empty;
        public ILingoCast Cast => _cast;
        public void CopyToClipBoard() { }
        public void Erase() { }
        public void ImportFileInto() { }
        public void Move() { }
        public void PasteClipBoardInto() { }
        public void Preload() { Preloaded = true; }
        public void Unload() { }
        public ILingoMember Duplicate(int? newNumber = null) => this;
        public ILingoMember? GetMemberInCastByOffset(int numberOffset) => null;
        public bool IsPixelTransparent(int x, int y) => false;
        public void Dispose() { }
        public IAbstTexture2D? TextureLingo => null;
        public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor) => null;
        public string Text { get; set; } = string.Empty;
        public void SetTextMD(AbstMarkdownData data)
        {
            Text = data.Markdown;
            MarkdownData = data;
        }
        public LingoLines Line => new(() => { });
        public LingoWords Word => new(Text);
        public LingoChars Char => new();
        public void Clear() { Text = string.Empty; }
        public void Copy() { }
        public void Cut() { }
        public void Paste() { }
        public void InsertText(string text) { }
        public void ReplaceSelection(string replacement) { }
        public void SetSelection(int start, int end) { }
        public void RequireRedraw() { }
        public void InitDefaults() { }
        public IAbstTexture2D? GetTexture() => null;
        public bool Editable { get; set; }
        public bool WordWrap { get; set; }
        public int ScrollTop { get; set; }
        public string Font { get; set; } = string.Empty;
        public int FontSize { get; set; }
        public LingoTextStyle FontStyle { get; set; }
        public AColor TextColor { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public AbstTextAlignment Alignment { get; set; }
        public int Margin { get; set; }
        public void ChangesHasBeenApplied() { }
        public void LoadFile() { LoadFileCalled = true; }
        public bool Preloaded { get; private set; }
        public bool LoadFileCalled { get; private set; }
        public AbstMarkdownData? MarkdownData { get; private set; }
        public void SetFileName(string name) => _fileName = name;
    }

    private class DummyFrameworkMember : ILingoFrameworkMember
    {
        public bool IsLoaded => true;
        public void CopyToClipboard() { }
        public void Erase() { }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void Preload() { }
        public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
        public void Unload() { }
        public bool IsPixelTransparent(int x, int y) => false;
    }
}
