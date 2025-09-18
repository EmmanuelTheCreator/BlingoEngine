using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Bitmaps;

public class BlLegacyBitmapReaderTests
{
    [Fact]
    public void ReadImgCastCst_LoadsPngAndAuxiliaryChunks()
    {
        var bitmaps = TestContextHarness.LoadBitmaps("Images/ImgCast.cst");

        bitmaps.Should().NotBeEmpty();

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 13
            && bitmap.Format == BlLegacyBitmapFormatKind.Png
            && bitmap.Bytes.Length == 529);

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 14
            && bitmap.Format == BlLegacyBitmapFormatKind.Bitd
            && bitmap.Bytes.Length == 1001);

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 15
            && bitmap.Format == BlLegacyBitmapFormatKind.AlphaMask
            && bitmap.Bytes.Length == 125);

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 16
            && bitmap.Format == BlLegacyBitmapFormatKind.Thumbnail
            && bitmap.Bytes.Length == 1303);
    }

    [Fact]
    public void ReadDirFileWithBitmap_FallsBackToBitdWhenEditorUnknown()
    {
        var bitmaps = TestContextHarness.LoadBitmaps("Images/Dir_With_One_Img_Sprite_Hallo.dir");

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 32
            && bitmap.Format == BlLegacyBitmapFormatKind.Bitd
            && bitmap.Bytes.Length == 1001);
    }
}
