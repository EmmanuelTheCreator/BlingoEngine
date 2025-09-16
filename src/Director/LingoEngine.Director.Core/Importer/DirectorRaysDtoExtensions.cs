using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using LingoEngine.IO.Data.DTO;
using ProjectorRays.director.Scores.Data;
using LingoEngine.IO.Data.DTO.Members;
using LingoEngine.IO.Data.DTO.Sprites;

namespace LingoEngine.Director.Core.Importer;

internal static class DirectorRaysDtoExtensions
{
    public static (LingoStageDTO Stage, LingoMovieDTO Movie) ToDto(this RaysDirectorFile dir, string movieName, DirFilesContainerDTO resources)
    {
        var stage = new LingoStageDTO
        {
            // todo
        };
        var movie = new LingoMovieDTO
        {
            Name = movieName,
            Number = 0,
            Tempo = 0,
            FrameCount = dir.Score?.Sprites.Count ?? 0
        };

        int castNum = 1;
        foreach (var cast in dir.Casts)
            movie.Casts.Add(cast.ToDto(castNum++, resources));

        if (dir.Score != null)
        {
            foreach (var f in dir.Score.Sprites)
                movie.Sprite2Ds.Add(f.ToDto());
        }

        return (stage, movie);
    }

    public static LingoCastDTO ToDto(this RaysCastChunk cast, int number, DirFilesContainerDTO resources)
    {
        var castDto = new LingoCastDTO
        {
            Name = cast.Name,
            Number = number,
            FileName = string.Empty,
            PreLoadMode = PreLoadModeTypeDTO.WhenNeeded
        };

        foreach (var mem in cast.Members.Values)
            castDto.Members.Add(mem.ToDto(castDto, resources));

        return castDto;
    }

    public static LingoMemberDTO ToDto(this RaysCastMemberChunk mem, LingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var baseDto = CreateBaseDto(mem, cast);

        return mem.Type switch
        {
            RaysMemberType.FieldMember or RaysMemberType.TextMember => mem.ToTextDto(baseDto),
            RaysMemberType.BitmapMember or RaysMemberType.PictureMember => mem.ToPictureDto(baseDto, cast, resources),
            RaysMemberType.SoundMember => mem.ToSoundDto(baseDto, cast, resources),
            _ => baseDto
        };
    }

    private static LingoMemberDTO CreateBaseDto(RaysCastMemberChunk mem, LingoCastDTO cast)
        => new LingoMemberDTO
        {
            Name = mem.GetName(),
            CastLibNum = cast.Number,
            NumberInCast = mem.Id,
            Type = MapMemberType(mem.Type),
            RegPoint = new LingoPointDTO(),
            Width = 0,
            Height = 0,
            Size = mem.SpecificData.Size,
            Comments = string.Empty,
            FileName = string.Empty,
            PurgePriority = 0
        };

    private static LingoMemberTextDTO ToTextDto(this RaysCastMemberChunk mem, LingoMemberDTO baseDto)
        => new LingoMemberTextDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = baseDto.Size,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            MarkDownText = mem.DecodedText?.Text ?? string.Empty
        };

    private static LingoMemberBitmapDTO ToPictureDto(this RaysCastMemberChunk mem, LingoMemberDTO baseDto, LingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var file = $"{cast.Number}_{mem.Id}.img";
        var dto = new LingoMemberBitmapDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = baseDto.Size,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            ImageFile = file
        };
        var source = mem.ImageData.Size > 0 ? mem.ImageData : mem.SpecificData;
        var bytes = source.Data.AsSpan(source.Offset, source.Size).ToArray();
        resources.Files.Add(new DirFileResourceDTO
        {
            CastName = cast.Name,
            FileName = file,
            Bytes = bytes
        });
        return dto;
    }

    private static LingoMemberSoundDTO ToSoundDto(this RaysCastMemberChunk mem, LingoMemberDTO baseDto, LingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var file = $"{cast.Number}_{mem.Id}.snd";
        var dto = new LingoMemberSoundDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = baseDto.Size,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            SoundFile = file
        };
        var bytes = mem.SpecificData.Data.AsSpan(mem.SpecificData.Offset, mem.SpecificData.Size).ToArray();
        resources.Files.Add(new DirFileResourceDTO
        {
            CastName = cast.Name,
            FileName = file,
            Bytes = bytes
        });
        return dto;
    }

    public static Lingo2DSpriteDTO ToDto(this RaySprite f)
        => new Lingo2DSpriteDTO
        {
            Name = $"Sprite{f.SpriteNumber}",
            SpriteNum = f.SpriteNumber,
            Member = f.MemberNum > 0 && f.MemberCastLib > 0
                ? new LingoMemberRefDTO { MemberNum = f.MemberNum, CastLibNum = f.MemberCastLib }
                : null,
            // todo : DisplayMember = f.DisplayMember,
            SpritePropertiesOffset = f.SpritePropertiesOffset,
            Lock = false,
            Visibility = true,
            LocH = f.LocH,
            LocV = f.LocV,
            LocZ = f.LocZ,
            Rotation = f.Rotation,
            Skew = f.Skew,
            RegPoint = new LingoPointDTO(),
            Ink = f.Ink,
            ForeColor = new LingoColorDTO { Code = f.ForeColor }, // todo: map color
            BackColor = new LingoColorDTO { Code = f.BackColor }, // todo : map color
            Blend = f.Blend,
            Editable = f.Editable,
            FlipH = f.FlipH,
            FlipV = f.FlipV,
            ScoreColor = f.ScoreColor,
            Width = f.Width,
            Height = f.Height,
            BeginFrame = f.StartFrame,
            EndFrame = f.EndFrame
        };

    private static LingoMemberTypeDTO MapMemberType(RaysMemberType t)
        => t switch
        {
            RaysMemberType.BitmapMember => LingoMemberTypeDTO.Bitmap,
            RaysMemberType.FilmLoopMember => LingoMemberTypeDTO.FilmLoop,
            RaysMemberType.TextMember => LingoMemberTypeDTO.Text,
            RaysMemberType.PaletteMember => LingoMemberTypeDTO.Palette,
            RaysMemberType.PictureMember => LingoMemberTypeDTO.Picture,
            RaysMemberType.SoundMember => LingoMemberTypeDTO.Sound,
            RaysMemberType.ButtonMember => LingoMemberTypeDTO.Button,
            RaysMemberType.ShapeMember => LingoMemberTypeDTO.Shape,
            RaysMemberType.MovieMember => LingoMemberTypeDTO.Movie,
            RaysMemberType.DigitalVideoMember => LingoMemberTypeDTO.DigitalVideo,
            RaysMemberType.ScriptMember => LingoMemberTypeDTO.Script,
            RaysMemberType.FieldMember => LingoMemberTypeDTO.Field,
            RaysMemberType.FontMember => LingoMemberTypeDTO.Font,
            _ => LingoMemberTypeDTO.Unknown
        };
}
