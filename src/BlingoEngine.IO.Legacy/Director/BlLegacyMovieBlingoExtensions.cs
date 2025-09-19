using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts;

namespace BlingoEngine.IO.Legacy.Director;

/// <summary>
/// Extension helpers that translate legacy movie archives into BlingoEngine DTOs.
/// </summary>
public static class BlLegacyMovieBlingoExtensions
{
    public static BlingoStageDTO ToBlingoStage(this BlLegacyMovieArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        return new BlingoStageDTO();
    }

    public static BlingoMovieDTO ToBlingo(this BlLegacyMovieArchive archive, string movieName, DirFilesContainerDTO resources)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(resources);

        var movie = new BlingoMovieDTO
        {
            Name = movieName,
            Number = 0,
            Tempo = 0,
            FrameCount = 0,
            MaxSpriteChannelCount = 0
        };

        var bitmapExporter = new BlLegacyBitmapExporter();
        var soundExporter = new BlLegacySoundExporter();
        var usedNames = new HashSet<string>(resources.Files.Select(f => f.FileName), StringComparer.OrdinalIgnoreCase);

        var castNumber = 1;
        foreach (var cast in archive.CastLibraries)
        {
            var castDto = cast.ToBlingo(castNumber, archive, resources, usedNames, bitmapExporter, soundExporter);
            movie.Casts.Add(castDto);
            castNumber++;
        }

        return movie;
    }

    public static BlingoCastDTO ToBlingo(
        this BlLegacyCastLibrary cast,
        int castNumber,
        BlLegacyMovieArchive archive,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacyBitmapExporter bitmapExporter,
        BlLegacySoundExporter soundExporter)
    {
        ArgumentNullException.ThrowIfNull(cast);
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(usedNames);
        ArgumentNullException.ThrowIfNull(bitmapExporter);
        ArgumentNullException.ThrowIfNull(soundExporter);

        var castDto = new BlingoCastDTO
        {
            Name = $"Cast {castNumber}",
            Number = castNumber,
            PreLoadMode = PreLoadModeTypeDTO.WhenNeeded
        };

        foreach (var slot in cast.MemberSlots)
        {
            var member = slot.ToBlingo(archive, castDto, resources, usedNames, bitmapExporter, soundExporter);
            castDto.Members.Add(member);
        }

        return castDto;
    }

    public static BlingoMemberDTO ToBlingo(
        this BlLegacyCastMemberSlot slot,
        BlLegacyMovieArchive archive,
        BlingoCastDTO castDto,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacyBitmapExporter bitmapExporter,
        BlLegacySoundExporter soundExporter)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(castDto);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(usedNames);
        ArgumentNullException.ThrowIfNull(bitmapExporter);
        ArgumentNullException.ThrowIfNull(soundExporter);

        var memberIndex = slot.SlotIndex + 1;
        var memberName = string.IsNullOrWhiteSpace(slot.Name) ? $"Member {memberIndex}" : slot.Name;

        var baseDto = new BlingoMemberDTO
        {
            Name = memberName,
            CastLibNum = castDto.Number,
            NumberInCast = memberIndex,
            Type = MapMemberType(slot.MemberType),
            RegPoint = new BlingoPointDTO(),
            Width = 0,
            Height = 0,
            Size = 0,
            Comments = string.Empty,
            FileName = string.Empty,
            PurgePriority = 0
        };

        return slot.MemberType switch
        {
            BlLegacyCastMemberType.Text => baseDto.ToTextMember(archive, slot.ResourceId),
            BlLegacyCastMemberType.Field => baseDto.ToFieldMember(archive, slot.ResourceId),
            BlLegacyCastMemberType.Bitmap or BlLegacyCastMemberType.Picture => baseDto.ToBitmapMember(
                archive,
                slot.ResourceId,
                castDto,
                resources,
                usedNames,
                bitmapExporter),
            BlLegacyCastMemberType.Sound => baseDto.ToSoundMember(
                archive,
                slot.ResourceId,
                castDto,
                resources,
                usedNames,
                soundExporter),
            _ => baseDto
        };
    }

    public static BlingoMemberDTO ToTextMember(this BlingoMemberDTO baseDto, BlLegacyMovieArchive archive, int castResourceId)
    {
        if (!archive.TryGetText(castResourceId, out var text))
            return baseDto;

        var content = DecodeText(text, archive.DirectorVersion);
        return new BlingoMemberTextDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = text.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            MarkDownText = content
        };
    }

    public static BlingoMemberDTO ToFieldMember(this BlingoMemberDTO baseDto, BlLegacyMovieArchive archive, int castResourceId)
    {
        if (!archive.TryGetField(castResourceId, out var field))
            return baseDto;

        var content = DecodeField(field, archive.DirectorVersion);
        return new BlingoMemberFieldDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = field.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            MarkDownText = content
        };
    }

    public static BlingoMemberDTO ToBitmapMember(
        this BlingoMemberDTO baseDto,
        BlLegacyMovieArchive archive,
        int castResourceId,
        BlingoCastDTO castDto,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacyBitmapExporter exporter)
    {
        if (!archive.TryGetBitmap(castResourceId, out var bitmap))
            return baseDto;

        var resource = exporter.CreateResource(bitmap, castDto.Name, $"{castDto.Number}_{baseDto.NumberInCast}");
        var fileName = EnsureUniqueFileName(resource.FileName, usedNames);
        resource.FileName = fileName;
        resources.Files.Add(resource);

        return new BlingoMemberBitmapDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = bitmap.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            ImageFile = fileName
        };
    }

    public static BlingoMemberDTO ToSoundMember(
        this BlingoMemberDTO baseDto,
        BlLegacyMovieArchive archive,
        int castResourceId,
        BlingoCastDTO castDto,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacySoundExporter exporter)
    {
        if (!archive.TryGetSound(castResourceId, out var sound))
            return baseDto;

        var resource = exporter.CreateResource(sound, castDto.Name, $"{castDto.Number}_{baseDto.NumberInCast}");
        var fileName = EnsureUniqueFileName(resource.FileName, usedNames);
        resource.FileName = fileName;
        resources.Files.Add(resource);

        return new BlingoMemberSoundDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = sound.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            SoundFile = fileName
        };
    }

    private static string DecodeText(BlLegacyText text, int directorVersion)
    {
        return text.Format switch
        {
            BlLegacyTextFormatKind.Stxt => BlLegacyPlainTextDecoder.Decode(text.Bytes),
            BlLegacyTextFormatKind.Xmed => DecodeStyledText(text.Bytes, directorVersion),
            _ => string.Empty
        };
    }

    private static string DecodeField(BlLegacyField field, int directorVersion)
    {
        return field.Format switch
        {
            BlLegacyFieldFormatKind.Stxt => BlLegacyPlainTextDecoder.Decode(field.Bytes),
            BlLegacyFieldFormatKind.Xmed => DecodeStyledText(field.Bytes, directorVersion),
            _ => string.Empty
        };
    }

    private static string DecodeStyledText(byte[] data, int directorVersion)
    {
        var reader = new BlXmedTextReader();
        var document = directorVersion > 0 ? reader.Read(data, directorVersion) : reader.Read(data);
        return BlXmedMarkdownConverter.ToCustomMarkdown(document);
    }

    private static string EnsureUniqueFileName(string fileName, HashSet<string> usedNames)
    {
        if (usedNames.Add(fileName))
            return fileName;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var index = 1;
        string candidate;
        do
        {
            candidate = $"{nameWithoutExtension}_{index}{extension}";
            index++;
        }
        while (!usedNames.Add(candidate));

        return candidate;
    }

    private static BlingoMemberTypeDTO MapMemberType(BlLegacyCastMemberType type)
    {
        return type switch
        {
            BlLegacyCastMemberType.Bitmap => BlingoMemberTypeDTO.Bitmap,
            BlLegacyCastMemberType.FilmLoop => BlingoMemberTypeDTO.FilmLoop,
            BlLegacyCastMemberType.Text => BlingoMemberTypeDTO.Text,
            BlLegacyCastMemberType.Palette => BlingoMemberTypeDTO.Palette,
            BlLegacyCastMemberType.Picture => BlingoMemberTypeDTO.Picture,
            BlLegacyCastMemberType.Sound => BlingoMemberTypeDTO.Sound,
            BlLegacyCastMemberType.Button => BlingoMemberTypeDTO.Button,
            BlLegacyCastMemberType.Shape => BlingoMemberTypeDTO.Shape,
            BlLegacyCastMemberType.Movie => BlingoMemberTypeDTO.Movie,
            BlLegacyCastMemberType.DigitalVideo => BlingoMemberTypeDTO.DigitalVideo,
            BlLegacyCastMemberType.Script => BlingoMemberTypeDTO.Script,
            BlLegacyCastMemberType.Rte => BlingoMemberTypeDTO.Script,
            BlLegacyCastMemberType.Font => BlingoMemberTypeDTO.Font,
            BlLegacyCastMemberType.Field => BlingoMemberTypeDTO.Field,
            _ => BlingoMemberTypeDTO.Unknown
        };
    }
}
