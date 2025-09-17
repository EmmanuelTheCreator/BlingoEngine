using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using BlingoEngine.IO.Data.DTO;
using ProjectorRays.director.Scores.Data;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.Director.Core.Importer;

internal static class DirectorRaysDtoExtensions
{
    public static (BlingoStageDTO Stage, BlingoMovieDTO Movie) ToDto(this RaysDirectorFile dir, string movieName, DirFilesContainerDTO resources)
    {
        var stage = new BlingoStageDTO
        {
            // todo
        };
        var movie = new BlingoMovieDTO
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

    public static BlingoCastDTO ToDto(this RaysCastChunk cast, int number, DirFilesContainerDTO resources)
    {
        var castDto = new BlingoCastDTO
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

    public static BlingoMemberDTO ToDto(this RaysCastMemberChunk mem, BlingoCastDTO cast, DirFilesContainerDTO resources)
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

    private static BlingoMemberDTO CreateBaseDto(RaysCastMemberChunk mem, BlingoCastDTO cast)
        => new BlingoMemberDTO
        {
            Name = mem.GetName(),
            CastLibNum = cast.Number,
            NumberInCast = mem.Id,
            Type = MapMemberType(mem.Type),
            RegPoint = new BlingoPointDTO(),
            Width = 0,
            Height = 0,
            Size = mem.SpecificData.Size,
            Comments = string.Empty,
            FileName = string.Empty,
            PurgePriority = 0
        };

    private static BlingoMemberTextDTO ToTextDto(this RaysCastMemberChunk mem, BlingoMemberDTO baseDto)
        => new BlingoMemberTextDTO
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

    private static BlingoMemberBitmapDTO ToPictureDto(this RaysCastMemberChunk mem, BlingoMemberDTO baseDto, BlingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var file = $"{cast.Number}_{mem.Id}.img";
        var dto = new BlingoMemberBitmapDTO
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

    private static BlingoMemberSoundDTO ToSoundDto(this RaysCastMemberChunk mem, BlingoMemberDTO baseDto, BlingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var file = $"{cast.Number}_{mem.Id}.snd";
        var dto = new BlingoMemberSoundDTO
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

    public static Blingo2DSpriteDTO ToDto(this RaySprite f)
        => new Blingo2DSpriteDTO
        {
            Name = $"Sprite{f.SpriteNumber}",
            SpriteNum = f.SpriteNumber,
            Member = f.MemberNum > 0 && f.MemberCastLib > 0
                ? new BlingoMemberRefDTO { MemberNum = f.MemberNum, CastLibNum = f.MemberCastLib }
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
            RegPoint = new BlingoPointDTO(),
            Ink = f.Ink,
            ForeColor = new BlingoColorDTO { Code = f.ForeColor }, // todo: map color
            BackColor = new BlingoColorDTO { Code = f.BackColor }, // todo : map color
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

    private static BlingoMemberTypeDTO MapMemberType(RaysMemberType t)
        => t switch
        {
            RaysMemberType.BitmapMember => BlingoMemberTypeDTO.Bitmap,
            RaysMemberType.FilmLoopMember => BlingoMemberTypeDTO.FilmLoop,
            RaysMemberType.TextMember => BlingoMemberTypeDTO.Text,
            RaysMemberType.PaletteMember => BlingoMemberTypeDTO.Palette,
            RaysMemberType.PictureMember => BlingoMemberTypeDTO.Picture,
            RaysMemberType.SoundMember => BlingoMemberTypeDTO.Sound,
            RaysMemberType.ButtonMember => BlingoMemberTypeDTO.Button,
            RaysMemberType.ShapeMember => BlingoMemberTypeDTO.Shape,
            RaysMemberType.MovieMember => BlingoMemberTypeDTO.Movie,
            RaysMemberType.DigitalVideoMember => BlingoMemberTypeDTO.DigitalVideo,
            RaysMemberType.ScriptMember => BlingoMemberTypeDTO.Script,
            RaysMemberType.FieldMember => BlingoMemberTypeDTO.Field,
            RaysMemberType.FontMember => BlingoMemberTypeDTO.Font,
            _ => BlingoMemberTypeDTO.Unknown
        };
}

