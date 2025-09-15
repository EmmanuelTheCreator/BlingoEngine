using AbstUI.Primitives;
using LingoEngine.Animations;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.ColorPalettes;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.FilmLoops;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Tempos;
using LingoEngine.Texts;
using LingoEngine.Transitions;
using System.Text.Json;

namespace LingoEngine.IO;

public interface IJsonStateRepository
{
    LingoMovie Load(LingoProjectDTO dto, LingoPlayer player, string resourceDir);
    LingoMovie Load(LingoStageDTO stageDto, LingoMovieDTO movieDto, LingoPlayer player, string resourceDir);
    LingoMovie Load(string filePath, LingoPlayer player);

    (string JsonString, LingoMovieDTO MovieDto) Save(string filePath, LingoMovie movie, JsonStateRepository.MovieStoreOptions? options = null);


    LingoProjectDTO ToProjectDto(LingoPlayer player, JsonStateRepository.MovieStoreOptions options);

    (string JsonString, LingoMovieDTO MovieDto) Serialize(LingoMovie movie, JsonStateRepository.MovieStoreOptions options);
    string SerializeProject(LingoPlayer player, JsonStateRepository.MovieStoreOptions options);
    LingoMovieDTO Deserialize(string json);
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

    public string SerializeProject(LingoPlayer player, MovieStoreOptions options)
    {
        var dto = ToProjectDto(player, options);
        var joptions = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(dto, joptions);
        return json;
    }
    public LingoProjectDTO ToProjectDto(LingoPlayer player, MovieStoreOptions options)
    {
        LingoProjectDTO projectDTO = new LingoProjectDTO
        {
            Stage = new LingoStageDTO
            {
                Width = player.Stage.Width,
                Height = player.Stage.Height,
                BackgroundColor = player.Stage.BackgroundColor.ToDto(),
            },
        };
        if (player.ActiveMovie != null)
            projectDTO.Movies = [((LingoMovie)player.ActiveMovie).ToDto(options)];
        return projectDTO;
    }
    public (string JsonString, LingoMovieDTO MovieDto) Serialize(LingoMovie movie, MovieStoreOptions options)
    {
        LingoMovieDTO dto = movie.ToDto(options);
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
        LingoProjectDTO dto = DeserializeProject(json);

        return Load(dto, player, dir);
    }

    public LingoProjectDTO DeserializeProject(string json)
    {
        return JsonSerializer.Deserialize<LingoProjectDTO>(json) ?? throw new Exception("Invalid project file");
    }
    public LingoMovieDTO Deserialize(string json)
    {
        return JsonSerializer.Deserialize<LingoMovieDTO>(json) ?? throw new Exception("Invalid movie file");
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
        var memberMap = new Dictionary<(int CastNum, int MemberNum), LingoMember>();

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
                        if (TryResolveMember<LingoMemberSound>(memberMap, sndEntry.Member, out var sndMem2) && sndMem2 != null)
                            flMem.AddSound(sndEntry.Channel, sndEntry.StartFrame, sndMem2);
                    }
                }

                memberMap[(member.CastLibNum, member.NumberInCast)] = member;
            }
        }

        foreach (var sDto in dto.Sprite2Ds)
            BuildSpriteFromDto(sDto, movie, memberMap, mediator);

        foreach (var tempoDto in dto.TempoSprites)
            BuildTempoSpriteFromDto(tempoDto, movie);

        foreach (var paletteDto in dto.ColorPaletteSprites)
            BuildColorPaletteSpriteFromDto(paletteDto, movie, memberMap);

        foreach (var transitionDto in dto.TransitionSprites)
            BuildTransitionSpriteFromDto(transitionDto, movie, memberMap);

        foreach (var soundDto in dto.SoundSprites)
            BuildSoundSpriteFromDto(soundDto, movie, memberMap);

        return movie;
    }

    private static LingoSprite2D BuildSpriteFromDto(Lingo2DSpriteDTO sDto, LingoMovie movie, Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap, ILingoEventMediator eventMediator)
    {
        LingoSprite2D sprite;

        sprite = movie.AddSprite(sDto.SpriteNum, sDto.Name, s =>
        {
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

        if (TryResolveMember(memberMap, sDto.Member, out var mem) && mem != null)
        {
            state.Member = mem;
            sprite.SetMember(mem);
        }

        sprite.InitialState = state;
        sprite.LoadState(state);

        AddAnimator(sDto, sprite, movie, eventMediator);
        return sprite;
    }

    private static void BuildTempoSpriteFromDto(LingoTempoSpriteDTO dto, LingoMovie movie)
    {
        movie.Tempos.Add(dto.BeginFrame, sprite => TempoSpriteDtoConverter.Apply(dto, sprite));
    }

    private static void BuildColorPaletteSpriteFromDto(LingoColorPaletteSpriteDTO dto, LingoMovie movie, Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap)
    {
        var sprite = movie.ColorPalettes.Add(dto.BeginFrame);

        if (TryResolveMember<LingoColorPaletteMember>(memberMap, dto.Member, out var paletteMember) && paletteMember != null)
        {
            sprite.SetMember(paletteMember);
        }

        ColorPaletteSpriteDtoConverter.Apply(dto, sprite);
        sprite.InitialState = sprite.GetState();
    }

    private static void BuildTransitionSpriteFromDto(LingoTransitionSpriteDTO dto, LingoMovie movie, Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap)
    {
        var sprite = movie.Transitions.Add(dto.BeginFrame);

        if (TryResolveMember<LingoTransitionMember>(memberMap, dto.Member, out var transitionMember) && transitionMember != null)
        {
            sprite.SetMember(transitionMember);
        }

        TransitionSpriteDtoConverter.Apply(dto, sprite);
        sprite.InitialState = sprite.GetState();
    }

    private static void BuildSoundSpriteFromDto(LingoSpriteSoundDTO dto, LingoMovie movie, Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap)
    {
        if (!TryResolveMember<LingoMemberSound>(memberMap, dto.Member, out var soundMember) || soundMember == null)
        {
            return;
        }

        var sprite = movie.Audio.Add(dto.Channel, dto.BeginFrame, soundMember);
        SoundSpriteDtoConverter.Apply(dto, sprite);
        sprite.InitialState = sprite.GetState();
    }

    private static bool TryResolveMember(Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap, LingoMemberRefDTO? reference, out LingoMember? member)
    {
        member = null;
        if (reference == null || reference.CastLibNum <= 0 || reference.MemberNum <= 0)
        {
            return false;
        }

        if (memberMap.TryGetValue((reference.CastLibNum, reference.MemberNum), out var found))
        {
            member = found;
            return true;
        }

        return false;
    }

    private static bool TryResolveMember<TMember>(Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap, LingoMemberRefDTO? reference, out TMember? member)
        where TMember : LingoMember
    {
        member = null;
        if (!TryResolveMember(memberMap, reference, out var baseMember))
        {
            return false;
        }

        if (baseMember is TMember typed)
        {
            member = typed;
            return true;
        }

        return false;
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
            CastNum = sDto.Member.CastLibNum,
            FlipH = sDto.FlipH,
            FlipV = sDto.FlipV,
            Hilite = sDto.Hilite,
            MemberNumberInCast = sDto.Member.MemberNum,
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
    private static void AddAnimator(Lingo2DSpriteDTO sDto, LingoSprite2D sprite, ILingoSpritesPlayer spritesPlayer, ILingoEventMediator eventMediator)
    {
        if (sDto.Animator == null)
            return;
        var animatorProps = CreateAnimatorProperties(sDto.Animator, sprite);
        sprite.AddAnimator(animatorProps, eventMediator);
    }

    public static LingoSpriteAnimatorProperties CreateAnimatorProperties(LingoSpriteAnimatorDTO sDto, ILingoSprite2DLight sprite)
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
