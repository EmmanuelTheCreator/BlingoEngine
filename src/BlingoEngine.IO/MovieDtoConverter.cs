using BlingoEngine.Casts;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using System.Linq;

namespace BlingoEngine.IO;

internal static class MovieDtoConverter
{
    public static BlingoMovieDTO ToDto(this BlingoMovie movie, JsonStateRepository.MovieStoreOptions options)
    {
        return new BlingoMovieDTO
        {
            Name = movie.Name,
            Number = movie.Number,
            Tempo = movie.Tempo,
            FrameCount = movie.FrameCount,
            MaxSpriteChannelCount = movie.MaxSpriteChannelCount,
            About = movie.About,
            Copyright = movie.Copyright,
            UserName = movie.UserName,
            CompanyName = movie.CompanyName,
            Casts = movie.CastLib.GetAll().Select(c => ((BlingoCast)c).ToDto(options)).ToList(),
            Sprite2Ds = movie.GetAll2DSpritesToStore().Select(sprite => sprite.ToDto()).ToList(),
            TempoSprites = movie.Tempos.GetAllSprites().Select(sprite => sprite.ToDto()).ToList(),
            ColorPaletteSprites = movie.ColorPalettes.GetAllSprites().Select(sprite => sprite.ToDto()).ToList(),
            TransitionSprites = movie.Transitions.GetAllSprites().Select(sprite => sprite.ToDto()).ToList(),
            SoundSprites = movie.Audio.GetAllSprites().Select(sprite => sprite.ToDto()).ToList()
        };
    }

    public static BlingoCastDTO ToDto(this BlingoCast cast, JsonStateRepository.MovieStoreOptions options)
    {
        return new BlingoCastDTO
        {
            Name = cast.Name,
            FileName = cast.FileName,
            Number = cast.Number,
            PreLoadMode = (PreLoadModeTypeDTO)cast.PreLoadMode,
            Members = cast.GetAll().Select(m => m.ToDto(options)).ToList()
        };
    }
}

