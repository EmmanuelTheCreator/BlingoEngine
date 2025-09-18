using System.IO;
using System.Text;

using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Tests.Helpers;

using FluentAssertions;

using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Fields;

public class BlLegacyFieldReaderTests
{
    [Fact]
    public void Read_ResolvesStxtPayload()
    {
        var fields = TestContextHarness.LoadFields("Texts_Fields/Field_Hallo.cst");

        fields.Should().HaveCount(1);
        var field = fields[0];
        field.Format.Should().Be(BlLegacyFieldFormatKind.Stxt);
        field.Bytes.Length.Should().Be(39);
    }

    [Fact]
    public void Read_HandlesDifferentFieldLengths()
    {
        var fields = TestContextHarness.LoadFields("Texts_Fields/Field_Hallo_3lines.cst");

        fields.Should().HaveCount(1);
        var field = fields[0];
        field.Format.Should().Be(BlLegacyFieldFormatKind.Stxt);
        field.Bytes.Length.Should().Be(51);
    }

    [Fact]
    public void ReadDirector2StandaloneField_LoadsStxtPayload()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyFieldWriter(stream);
        var payload = Encoding.ASCII.GetBytes("Editable");
        var entry = writer.WriteStxt(410, payload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(entry);

        var fields = context.ReadFields();

        fields.Should().HaveCount(1);
        var field = fields[0];
        field.ResourceId.Should().Be(entry.Id);
        field.Format.Should().Be(BlLegacyFieldFormatKind.Stxt);
        field.Bytes.Should().Equal(payload);
    }

    [Fact]
    public void ReadDirector4LinkedField_LoadsStxtPayload()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyFieldWriter(stream);
        var payload = Encoding.ASCII.GetBytes("Linked field");
        var entry = writer.WriteStxt(420, payload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(entry);
        context.AddResourceRelationship(new BlResourceKeyLink(entry.Id, 512, entry.Tag));

        var fields = context.ReadFields();

        fields.Should().HaveCount(1);
        var field = fields[0];
        field.ResourceId.Should().Be(entry.Id);
        field.Format.Should().Be(BlLegacyFieldFormatKind.Stxt);
        field.Bytes.Should().Equal(payload);
    }
}
