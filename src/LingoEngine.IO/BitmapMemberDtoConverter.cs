using LingoEngine.Bitmaps;
using LingoEngine.IO.Data.DTO;
using System.IO;

namespace LingoEngine.IO;

internal static class BitmapMemberDtoConverter
{
    public static LingoMemberPictureDTO ToDto(this LingoMemberBitmap picture, LingoMemberDTO baseDto, JsonStateRepository.MovieStoreOptions options)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberPictureDTO());
        dto.ImageFile = SavePicture(picture, options);
        return dto;
    }

    private static string SavePicture(LingoMemberBitmap picture, JsonStateRepository.MovieStoreOptions options)
    {
        if (picture.ImageData == null || string.IsNullOrWhiteSpace(options.TargetDirectory) || !options.WithMedia)
        {
            return string.Empty;
        }

        var ext = GetPictureExtension(picture);
        var name = $"{picture.NumberInCast}_{MediaFileNameHelper.SanitizeFileName(picture.Name)}.{ext}";
        var path = Path.Combine(options.TargetDirectory, name);
        File.WriteAllBytes(path, picture.ImageData);
        return name;
    }

    private static string GetPictureExtension(LingoMemberBitmap picture)
    {
        var format = picture.Format.ToLowerInvariant();
        if (format.Contains("png") || format.Contains("gif") || format.Contains("tiff"))
        {
            return "png";
        }

        return "bmp";
    }
}
