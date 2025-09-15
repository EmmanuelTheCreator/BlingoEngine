using LingoEngine.Casts;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Members;
using LingoEngine.Movies;
using System.Linq;

namespace LingoEngine.IO;

internal static class MovieDtoConverter
{
    public static LingoMovieDTO ToDto(this LingoMovie movie, JsonStateRepository.MovieStoreOptions options)
    {
        return new LingoMovieDTO
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
            Casts = movie.CastLib.GetAll().Select(c => ((LingoCast)c).ToDto(options)).ToList(),
            Sprite2Ds = movie.GetAll2DSpritesToStore().Select(sprite => sprite.ToDto()).ToList(),
            TempoSprites = movie.Tempos.GetAllSprites().Select(sprite => sprite.ToDto()).ToList(),
            ColorPaletteSprites = movie.ColorPalettes.GetAllSprites().Select(sprite => sprite.ToDto()).ToList(),
            TransitionSprites = movie.Transitions.GetAllSprites().Select(sprite => sprite.ToDto()).ToList(),
            SoundSprites = movie.Audio.GetAllSprites().Select(sprite => sprite.ToDto()).ToList()
        };
    }

    public static LingoCastDTO ToDto(this LingoCast cast, JsonStateRepository.MovieStoreOptions options)
    {
        return new LingoCastDTO
        {
            Name = cast.Name,
            FileName = cast.FileName,
            Number = cast.Number,
            PreLoadMode = (PreLoadModeTypeDTO)cast.PreLoadMode,
            Members = cast.GetAll().Select(m => m.ToDto(options)).ToList()
        };
    }
}
