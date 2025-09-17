using BlingoEngine.Bitmaps;
using BlingoEngine.IO.Data.DTO.Members;
using System.IO;

namespace BlingoEngine.IO;

internal static class BitmapMemberDtoConverter
{
    public static BlingoMemberBitmapDTO ToDto(this BlingoMemberBitmap picture, BlingoMemberDTO baseDto, JsonStateRepository.MovieStoreOptions options)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new BlingoMemberBitmapDTO());
        dto.ImageFile = SavePicture(picture, options);
        return dto;
    }

    private static string SavePicture(BlingoMemberBitmap picture, JsonStateRepository.MovieStoreOptions options)
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

    private static string GetPictureExtension(BlingoMemberBitmap picture)
    {
        var format = picture.Format.ToLowerInvariant();
        if (format.Contains("png") || format.Contains("gif") || format.Contains("tiff"))
        {
            return "png";
        }

        return "bmp";
    }
}

