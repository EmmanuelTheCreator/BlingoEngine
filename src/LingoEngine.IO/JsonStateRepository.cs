using AbstUI.Primitives;
using LingoEngine.Animations;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.FilmLoops;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using System.Reflection;
using System.Text.Json;
using static LingoEngine.IO.JsonStateRepository;

namespace LingoEngine.IO;

public interface IJsonStateRepository
{
    LingoMovie Load(LingoProjectDTO dto, LingoPlayer player, string resourceDir);
    LingoMovie Load(LingoStageDTO stageDto, LingoMovieDTO movieDto, LingoPlayer player, string resourceDir);
    LingoMovie Load(string filePath, LingoPlayer player);
    (string JsonString, LingoMovieDTO MovieDto) Save(string filePath, LingoMovie movie, MovieStoreOptions? options = null);
}

public class JsonStateRepository : IJsonStateRepository
{
    public class MovieStoreOptions
    {
        public bool WithMedia { get; set; }
        public string TargetDirectory { get; set; } = "";
    }
    public (string JsonString, LingoMovieDTO MovieDto) Save(string filePath, LingoMovie movie, MovieStoreOptions? options = null)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();
        if (options == null) options = new();
        options.TargetDirectory = dir;
        var jsonTuple = Serialize(movie, options);
        File.WriteAllText(filePath, jsonTuple.JsonString);
        return jsonTuple;
    }

    public (string JsonString, LingoMovieDTO MovieDto) Serialize(LingoMovie movie, MovieStoreOptions options)
    {
        var dto = ToDto(movie, options);
        var joptions = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(dto, joptions);
        return (json, dto);
    }

    public LingoMovie Load(string filePath, LingoPlayer player)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();

        var json = File.ReadAllText(filePath);
        LingoProjectDTO dto = Deserialize(json);

        return Load(dto, player, dir);
    }

    public LingoProjectDTO Deserialize(string json)
    {
        return JsonSerializer.Deserialize<LingoProjectDTO>(json) ?? throw new Exception("Invalid movie file");
    }

    public LingoMovie Load(LingoProjectDTO dto, LingoPlayer player, string resourceDir)
    {
        if (string.IsNullOrEmpty(resourceDir))
            resourceDir = Directory.GetCurrentDirectory();
        BuildStageFromDto(dto.Stage, player, resourceDir);
        return BuildMovieFromDto(dto.Movies.First(), player, resourceDir);
    }
    public LingoMovie Load(LingoStageDTO stageDto, LingoMovieDTO movieDto, LingoPlayer player, string resourceDir)
    {
        if (string.IsNullOrEmpty(resourceDir))
            resourceDir = Directory.GetCurrentDirectory();

        BuildStageFromDto(stageDto, player, resourceDir);
        return BuildMovieFromDto(movieDto, player, resourceDir);
    }

    private static void BuildStageFromDto(LingoStageDTO dtoStage, LingoPlayer player, string resourceDir)
    {
        player.LoadStage(dtoStage.Width, dtoStage.Height, FromDto(dtoStage.BackgroundColor));
    }
    private static LingoMovie BuildMovieFromDto(LingoMovieDTO dto, LingoPlayer player, string resourceDir)
    {
        ILingoEventMediator mediator = player.GetEventMediator();
        var movie = (LingoMovie)player.NewMovie(dto.Name);
        movie.Tempo = dto.Tempo;
        movie.About = dto.About;
        movie.Copyright = dto.Copyright;
        movie.UserName = dto.UserName;
        movie.CompanyName = dto.CompanyName;

        var castMap = new Dictionary<int, LingoCast>();
        var memberMap = new Dictionary<int, LingoMember>();

        foreach (var castDto in dto.Casts)
        {
            var cast = (LingoCast)movie.CastLib.AddCast(castDto.Name);
            cast.PreLoadMode = (PreLoadModeType)castDto.PreLoadMode;
            castMap[castDto.Number] = cast;

            foreach (var memDto in castDto.Members)
            {
                var reg = new APoint(memDto.RegPoint.X, memDto.RegPoint.Y);
                string fileName = memDto.FileName;
                if (memDto is LingoMemberPictureDTO pic && !string.IsNullOrEmpty(pic.ImageFile))
                    fileName = Path.Combine(resourceDir, pic.ImageFile);
                if (memDto is LingoMemberSoundDTO snd && !string.IsNullOrEmpty(snd.SoundFile))
                    fileName = Path.Combine(resourceDir, snd.SoundFile);

                var member = (LingoMember)cast.Add((LingoMemberType)memDto.Type, memDto.NumberInCast, memDto.Name, fileName, reg);
                member.Width = memDto.Width;
                member.Height = memDto.Height;
                member.Size = memDto.Size;
                member.Comments = memDto.Comments;
                member.PurgePriority = memDto.PurgePriority;

                if (member is LingoMemberText txt && memDto is LingoMemberTextDTO txtDto)
                    txt.SetTextMD(txtDto.MarkDownText);
                if (member is LingoMemberField fld && memDto is LingoMemberFieldDTO fldDto)
                    fld.SetTextMD(fldDto.MarkDownText);
                if (member is LingoMemberSound sndMem && memDto is LingoMemberSoundDTO sndDto)
                {
                    sndMem.Loop = sndDto.Loop;
                    sndMem.IsLinked = sndDto.IsLinked;
                    sndMem.LinkedFilePath = sndDto.LinkedFilePath;
                }
                if (member is LingoFilmLoopMember flMem && memDto is LingoMemberFilmLoopDTO flDto)
                {
                    flMem.Framing = (LingoFilmLoopFraming)flDto.Framing;
                    flMem.Loop = flDto.Loop;
                    foreach (var sEntry in flDto.SpriteEntries)
                        flMem.AddSprite(BuildSprite2DVirtualFromDto(sEntry));
                    foreach (var sndEntry in flDto.SoundEntries)
                    {
                        if (memberMap.TryGetValue(sndEntry.SoundMemberNum, out var sm) && sm is LingoMemberSound sndMem2)
                            flMem.AddSound(sndEntry.Channel, sndEntry.StartFrame, sndMem2);
                    }
                }

                memberMap[memDto.Number] = member;
            }
        }

        foreach (var sDto in dto.Sprites)
            BuildSpriteFromDto(sDto, movie, memberMap, mediator);

        return movie;
    }

    private static LingoSprite2D BuildSpriteFromDto(LingoSpriteDTO sDto, LingoMovie movie, Dictionary<int, LingoMember> memberMap, ILingoEventMediator eventMediator)
    {
        LingoSprite2D sprite;

        sprite = movie.AddSprite(sDto.SpriteNum, sDto.Name, s =>
        {
            s.Puppet = sDto.Puppet;
            s.Lock = sDto.Lock;
            s.Visibility = sDto.Visibility;
            s.BeginFrame = sDto.BeginFrame;
            s.EndFrame = sDto.EndFrame;
        });

        var state = new LingoSprite2DState
        {
            Name = sDto.Name,
            DisplayMember = sDto.DisplayMember,
            SpritePropertiesOffset = sDto.SpritePropertiesOffset,
            Ink = sDto.Ink,
            Blend = sDto.Blend,
            LocH = sDto.LocH,
            LocV = sDto.LocV,
            LocZ = sDto.LocZ,
            Rotation = sDto.Rotation,
            Skew = sDto.Skew,
            RegPoint = new APoint(sDto.RegPoint.X, sDto.RegPoint.Y),
            ForeColor = FromDto(sDto.ForeColor),
            BackColor = FromDto(sDto.BackColor),
            Editable = sDto.Editable,
            Width = sDto.Width,
            Height = sDto.Height,
            FlipH = sDto.FlipH,
            FlipV = sDto.FlipV,
        };

        if (memberMap.TryGetValue(sDto.MemberNum, out var mem))
        {
            state.Member = mem;
            sprite.SetMember(mem);
        }

        sprite.InitialState = state;
        sprite.LoadState(state);

        AddAnimator(sDto, sprite, movie, eventMediator);
        return sprite;
    }
    private static LingoFilmLoopMemberSprite BuildSprite2DVirtualFromDto(LingoFilmLoopMemberSpriteDTO sDto)
    {

        var sprite = new LingoFilmLoopMemberSprite
        {
            LocH = sDto.LocH,
            LocV = sDto.LocV,
            LocZ = sDto.LocZ,
            Rotation = sDto.Rotation,
            Skew = sDto.Skew,
            RegPoint = new APoint(sDto.RegPoint.X, sDto.RegPoint.Y),
            Ink = sDto.Ink,
            ForeColor = FromDto(sDto.ForeColor),
            BackColor = FromDto(sDto.BackColor),
            Blend = sDto.Blend,
            Width = sDto.Width,
            Height = sDto.Height,
            BeginFrame = sDto.BeginFrame,
            EndFrame = sDto.EndFrame,
            DisplayMember = sDto.DisplayMember,
            Channel = sDto.Channel,
            CastNum = sDto.CastNum,
            FlipH = sDto.FlipH,
            FlipV = sDto.FlipV,
            Hilite = sDto.Hilite,
            MemberNumberInCast = sDto.MemberNumberInCast,
            SpriteNum = sDto.SpriteNum,
            Name = sDto.Name,
        };
        var anim = sDto.Animator;
        ApplyOptions(sprite.AnimatorProperties.Position.Options, anim.PositionOptions);
        ApplyOptions(sprite.AnimatorProperties.Rotation.Options, anim.RotationOptions);
        ApplyOptions(sprite.AnimatorProperties.Skew.Options, anim.SkewOptions);
        ApplyOptions(sprite.AnimatorProperties.ForegroundColor.Options, anim.ForegroundColorOptions);
        ApplyOptions(sprite.AnimatorProperties.BackgroundColor.Options, anim.BackgroundColorOptions);
        ApplyOptions(sprite.AnimatorProperties.Blend.Options, anim.BlendOptions);

        foreach (var k in anim.Position)
            sprite.AnimatorProperties.Position.AddKeyFrame(k.Frame, new APoint(k.Value.X, k.Value.Y), (LingoEaseType)k.Ease);
        foreach (var k in anim.Rotation)
            sprite.AnimatorProperties.Rotation.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);
        foreach (var k in anim.Skew)
            sprite.AnimatorProperties.Skew.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);
        foreach (var k in anim.ForegroundColor)
            sprite.AnimatorProperties.ForegroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (LingoEaseType)k.Ease);
        foreach (var k in anim.BackgroundColor)
            sprite.AnimatorProperties.BackgroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (LingoEaseType)k.Ease);
        foreach (var k in anim.Blend)
            sprite.AnimatorProperties.Blend.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);

        return sprite;
    }
    private static void AddAnimator(LingoSpriteDTO sDto, LingoSprite2D sprite, ILingoSpritesPlayer spritesPlayer, ILingoEventMediator eventMediator)
    {
        if (sDto.Animator == null)
            return;
        var animatorProps = CreateAnimatorProperties(sDto.Animator, sprite);
        sprite.AddAnimator(animatorProps, eventMediator);
    }

    private static LingoSpriteAnimatorProperties CreateAnimatorProperties(LingoSpriteAnimatorDTO sDto, ILingoSprite2DLight sprite)
    {
        var animator = new LingoSpriteAnimatorProperties();


        ApplyOptions(animator.Position.Options, sDto.PositionOptions);
        ApplyOptions(animator.Rotation.Options, sDto.RotationOptions);
        ApplyOptions(animator.Skew.Options, sDto.SkewOptions);
        ApplyOptions(animator.ForegroundColor.Options, sDto.ForegroundColorOptions);
        ApplyOptions(animator.BackgroundColor.Options, sDto.BackgroundColorOptions);
        ApplyOptions(animator.Blend.Options, sDto.BlendOptions);

        foreach (var k in sDto.Position)
            animator.Position.AddKeyFrame(k.Frame, new APoint(k.Value.X, k.Value.Y), (LingoEaseType)k.Ease);
        foreach (var k in sDto.Rotation)
            animator.Rotation.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);
        foreach (var k in sDto.Skew)
            animator.Skew.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);
        foreach (var k in sDto.ForegroundColor)
            animator.ForegroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (LingoEaseType)k.Ease);
        foreach (var k in sDto.BackgroundColor)
            animator.BackgroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (LingoEaseType)k.Ease);
        foreach (var k in sDto.Blend)
            animator.Blend.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);

        return animator;
    }

    private static LingoMovieDTO ToDto(LingoMovie movie, MovieStoreOptions options)
    {
        return new LingoMovieDTO
        {
            Name = movie.Name,
            Number = movie.Number,
            Tempo = movie.Tempo,
            FrameCount = movie.FrameCount,
            About = movie.About,
            Copyright = movie.Copyright,
            UserName = movie.UserName,
            CompanyName = movie.CompanyName,
            Casts = movie.CastLib.GetAll().Select(c => ToDto((LingoCast)c, options)).ToList(),
            Sprites = GetAllSprites(movie).Select(ToDto).ToList()
        };
    }

    private static LingoCastDTO ToDto(LingoCast cast, MovieStoreOptions options)
    {
        return new LingoCastDTO
        {
            Name = cast.Name,
            FileName = cast.FileName,
            Number = cast.Number,
            PreLoadMode = (PreLoadModeTypeDTO)cast.PreLoadMode,
            Members = cast.GetAll().Select(m => ToDto(m, options)).ToList()
        };
    }

    private static LingoMemberDTO ToDto(ILingoMember member, MovieStoreOptions options)
    {
        var baseDto = new LingoMemberDTO
        {
            Name = member.Name,
            Number = member.Number,
            CastLibNum = member.CastLibNum,
            NumberInCast = member.NumberInCast,
            Type = (LingoMemberTypeDTO)member.Type,
            RegPoint = new LingoPointDTO { X = member.RegPoint.X, Y = member.RegPoint.Y },
            Width = member.Width,
            Height = member.Height,
            Size = member.Size,
            Comments = member.Comments,
            FileName = member.FileName,
            PurgePriority = member.PurgePriority
        };

        return member switch
        {
            LingoMemberField field => new LingoMemberFieldDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
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
                MarkDownText = field.InitialMarkdown != null? field.InitialMarkdown.Markdown :  field.Text
            },
            LingoMemberSound sound => new LingoMemberSoundDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
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
                Stereo = sound.Stereo,
                Length = sound.Length,
                Loop = sound.Loop,
                IsLinked = sound.IsLinked,
                LinkedFilePath = sound.LinkedFilePath,
                SoundFile = SaveSound(sound, options)
            },
            LingoMemberText text => new LingoMemberTextDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
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
                MarkDownText = text.InitialMarkdown != null ? text.InitialMarkdown.Markdown : text.Text
            },
            LingoMemberBitmap picture => new LingoMemberPictureDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
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
                ImageFile = SavePicture(picture, options)
            },
            LingoFilmLoopMember filmLoop => new LingoMemberFilmLoopDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
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
                Framing = (LingoFilmLoopFramingDTO)filmLoop.Framing,
                Loop = filmLoop.Loop,
                FrameCount = filmLoop.FrameCount,
                SpriteEntries = filmLoop.SpriteEntries.Select(ToDto).ToList(),
                SoundEntries = filmLoop.SoundEntries.Select(e => new LingoFilmLoopSoundEntryDTO
                {
                    Channel = e.Channel,
                    StartFrame = e.StartFrame,
                    SoundMemberNum = e.Sound.Number,
                    CastlibNum = e.Sound.CastLibNum
                }).ToList()
            },
            _ => baseDto
        };
    }

    private static LingoSpriteDTO ToDto(LingoSprite2D sprite)
    {
        var state = sprite.InitialState as LingoSprite2DState ?? (LingoSprite2DState)sprite.GetState();
        var dto = new LingoSpriteDTO
        {
            Name = state.Name,
            SpriteNum = sprite.SpriteNum,
            MemberNum = state.Member?.NumberInCast ?? sprite.MemberNum,
            DisplayMember = state.DisplayMember,
            SpritePropertiesOffset = state.SpritePropertiesOffset,
            Puppet = sprite.Puppet,
            Lock = sprite.Lock,
            Visibility = sprite.Visibility,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new LingoPointDTO { X = state.RegPoint.X, Y = state.RegPoint.Y },
            Ink = state.Ink,
            ForeColor = ToDto(state.ForeColor),
            BackColor = ToDto(state.BackColor),
            Blend = state.Blend,
            Editable = state.Editable,
            FlipH = state.FlipH,
            FlipV = state.FlipV,
            Width = state.Width,
            Height = state.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };

        foreach (var actor in GetSpriteActors(sprite))
        {
            switch (actor)
            {
                case LingoSpriteAnimator anim:
                    dto.Animator = ToDto(anim);
                    break;
                case LingoFilmLoopPlayer fl:
                    // Film loop player state is transient, no need to save
                    break;
            }
        }

        return dto;
    }
    private static LingoSpriteDTO ToDto(LingoSprite2DVirtual sprite)
    {
        var state = sprite.InitialState as LingoSprite2DVirtualState ?? (LingoSprite2DVirtualState)sprite.GetState();
        var dto = new LingoSpriteDTO
        {
            Name = state.Name,
            SpriteNum = sprite.SpriteNum,
            MemberNum = state.Member?.NumberInCast ?? sprite.MemberNum,
            DisplayMember = state.DisplayMember,
            Puppet = sprite.Puppet,
            Lock = sprite.Lock,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new LingoPointDTO { X = state.RegPoint.X, Y = state.RegPoint.Y },
            Ink = state.Ink,
            ForeColor = ToDto(state.ForeColor),
            BackColor = ToDto(state.BackColor),
            Blend = state.Blend,
            Width = state.Width,
            Height = state.Height,
            FlipH = state.FlipH,
            FlipV = state.FlipV,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };

        //foreach (var actor in GetSpriteActors(sprite))
        //{
        //    switch (actor)
        //    {
        //        case LingoSpriteAnimator anim:
        //            dto.Animator = ToDto(anim);
        //            break;
        //        case LingoFilmLoopPlayer fl:
        //            // Film loop player state is transient, no need to save
        //            break;
        //    }
        //}

        return dto;
    }
    private static LingoFilmLoopMemberSpriteDTO ToDto(LingoFilmLoopMemberSprite sprite)
    {
        var dto = new LingoFilmLoopMemberSpriteDTO
        {
            Name = sprite.Name,
            SpriteNum = sprite.SpriteNum,
            MemberNumberInCast = sprite.MemberNumberInCast,
            DisplayMember = sprite.DisplayMember,
            LocH = sprite.LocH,
            LocV = sprite.LocV,
            LocZ = sprite.LocZ,
            Rotation = sprite.Rotation,
            Skew = sprite.Skew,
            RegPoint = new LingoPointDTO { X = sprite.RegPoint.X, Y = sprite.RegPoint.Y },
            Ink = sprite.Ink,
            ForeColor = ToDto(sprite.ForeColor),
            BackColor = ToDto(sprite.BackColor),
            Blend = sprite.Blend,
            Width = sprite.Width,
            Height = sprite.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Channel = sprite.Channel,
            CastNum = sprite.CastNum,
            FlipH = sprite.FlipH,
            FlipV = sprite.FlipV,
            Hilite = sprite.Hilite,
            Animator = ToDto(sprite.AnimatorProperties),
        };

        return dto;
    }

    private static IEnumerable<LingoSprite2D> GetAllSprites(LingoMovie movie)
    {
        var field = typeof(LingoMovie).GetField("_allTimeSprites", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(movie) is IEnumerable<LingoSprite2D> sprites)
            return sprites.Where(x => !x.Puppet && !x.IsDeleted);
        return Enumerable.Empty<LingoSprite2D>();
    }

    private static IEnumerable<object> GetSpriteActors(LingoSprite2D sprite)
    {
        var field = typeof(LingoSprite2D).GetField("_spriteActors", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(sprite) is IEnumerable<object> actors)
            return actors;
        return Enumerable.Empty<object>();
    }

    private static LingoSpriteAnimatorDTO ToDto(LingoSpriteAnimator animatorA)
    {
        return ToDto(animatorA.Properties);
    }

    private static LingoSpriteAnimatorDTO ToDto(LingoSpriteAnimatorProperties animProps)
    {
        return new LingoSpriteAnimatorDTO
        {
            Position = animProps.Position.KeyFrames.Select(k => new LingoPointKeyFrameDTO
            {
                Frame = k.Frame,
                Value = new LingoPointDTO { X = k.Value.X, Y = k.Value.Y },
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            PositionOptions = ToDto(animProps.Position.Options),

            Rotation = animProps.Rotation.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            RotationOptions = ToDto(animProps.Rotation.Options),

            Skew = animProps.Skew.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            SkewOptions = ToDto(animProps.Skew.Options),

            ForegroundColor = animProps.ForegroundColor.KeyFrames.Select(k => new LingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = ToDto(k.Value),
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            ForegroundColorOptions = ToDto(animProps.ForegroundColor.Options),

            BackgroundColor = animProps.BackgroundColor.KeyFrames.Select(k => new LingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = ToDto(k.Value),
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            BackgroundColorOptions = ToDto(animProps.BackgroundColor.Options),

            Blend = animProps.Blend.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            BlendOptions = ToDto(animProps.Blend.Options)
        };
    }

    private static LingoTweenOptionsDTO ToDto(LingoTweenOptions options)
    {
        return new LingoTweenOptionsDTO
        {
            Enabled = options.Enabled,
            Curvature = options.Curvature,
            ContinuousAtEndpoints = options.ContinuousAtEndpoints,
            SpeedChange = (LingoSpeedChangeTypeDTO)options.SpeedChange,
            EaseIn = options.EaseIn,
            EaseOut = options.EaseOut
        };
    }

    private static LingoColorDTO ToDto(AColor color)
    {
        return new LingoColorDTO
        {
            Code = color.Code,
            Name = color.Name,
            R = color.R,
            G = color.G,
            B = color.B
        };
    }

    private static string SavePicture(LingoMemberBitmap picture, MovieStoreOptions options)
    {
        if (picture.ImageData == null || string.IsNullOrWhiteSpace(options.TargetDirectory) || !options.WithMedia)
            return string.Empty;

        var ext = GetPictureExtension(picture);
        var name = $"{picture.NumberInCast}_{SanitizeFileName(picture.Name)}.{ext}";
        var path = Path.Combine(options.TargetDirectory, name);
        File.WriteAllBytes(path, picture.ImageData);
        return name;
    }

    private static string SaveSound(LingoMemberSound sound, MovieStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.TargetDirectory)) return string.Empty;
        var source = !string.IsNullOrEmpty(sound.FileName) && File.Exists(sound.FileName)
            ? sound.FileName
            : sound.LinkedFilePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source))
            return string.Empty;

        var ext = GetSoundExtension(source);
        var name = $"{sound.NumberInCast}_{SanitizeFileName(sound.Name)}{ext}";
        if (!options.WithMedia) return name;
        var dest = Path.Combine(options.TargetDirectory, name);
        File.Copy(source, dest, true);
        return name;
    }

    private static string GetSoundExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
            return ".wav";
        return ext.ToLowerInvariant();
    }

    private static string GetPictureExtension(LingoMemberBitmap picture)
    {
        var format = picture.Format.ToLowerInvariant();
        if (format.Contains("png") || format.Contains("gif") || format.Contains("tiff"))
            return "png";
        return "bmp";
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static void ApplyOptions(LingoTweenOptions target, LingoTweenOptionsDTO dto)
    {
        target.Enabled = dto.Enabled;
        target.Curvature = dto.Curvature;
        target.ContinuousAtEndpoints = dto.ContinuousAtEndpoints;
        target.SpeedChange = (LingoSpeedChangeType)dto.SpeedChange;
        target.EaseIn = dto.EaseIn;
        target.EaseOut = dto.EaseOut;
    }

    private static AColor FromDto(LingoColorDTO dto)
        => new AColor(dto.Code, dto.R, dto.G, dto.B, dto.Name);
}
