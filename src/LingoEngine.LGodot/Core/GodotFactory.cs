using Godot;
using System;
using System.Numerics;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Movies;
using LingoEngine.LGodot.Sounds;
using LingoEngine.LGodot.Texts;
using LingoEngine.LGodot.Shapes;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.LGodot.Stages;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Shapes;
using LingoEngine.Gfx;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Styles;
using Microsoft.Extensions.Logging;
using LingoEngine.LGodot.Styles;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.LGodot.Scripts;
using LingoEngine.Scripts;
using LingoEngine.FilmLoops;
using LingoEngine.LGodot.FilmLoops;

namespace LingoEngine.LGodot.Core
{
    public class GodotFactory : ILingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly ILingoServiceProvider _serviceProvider;
        private readonly LingoGodotRootNode _lingoRootNode;
        private Node _rootNode;

        private readonly GodotGfxFactory _gfxFactory;

        public GodotFactory(ILingoServiceProvider serviceProvider, LingoGodotRootNode rootNode)
        {
            _lingoRootNode = rootNode;
            _rootNode = rootNode.RootNode;
            _serviceProvider = serviceProvider;
            var fontManager = _serviceProvider.GetRequiredService<ILingoFontManager>();
            var styleManager = _serviceProvider.GetRequiredService<ILingoGodotStyleManager>();
            _gfxFactory = new GodotGfxFactory(fontManager, styleManager, _lingoRootNode);
        }

        public ILingoGfxFactory GfxFactory => _gfxFactory;

        public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => lingoMovie.GetServiceProvider().GetRequiredService<T>();
        public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => lingoMovie.GetServiceProvider().GetRequiredService<T>();

        #region Sound

        /// <inheritdoc/>
        public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer)
        {
            var lingoSound = new LingoGodotSound();
            var soundChannel = new LingoSound(lingoSound, castLibsContainer, this);
            lingoSound.Init(soundChannel);
            return soundChannel;
        }
        /// <inheritdoc/>
        public LingoSoundChannel CreateSoundChannel(int number)
        {
            var lingoSoundChannel = new LingoGodotSoundChannel(number, _rootNode);
            var soundChannel = new LingoSoundChannel(lingoSoundChannel, number);
            lingoSoundChannel.Init(soundChannel);
            _disposables.Add(lingoSoundChannel);
            return soundChannel;
        }
        #endregion


        #region Members
        /// <inheritdoc/>
        public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember
        {

            return typeof(T) switch
            {
                Type t when t == typeof(LingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberShape) => (CreateMemberShape(cast, numberInCast, name) as T)!,
            };
        }
        /// <inheritdoc/>
        public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var lingoMemberSound = new LingoGodotMemberSound();
            var memberSound = new LingoMemberSound(lingoMemberSound, (LingoCast)cast, numberInCast, name, fileName ?? "");
            lingoMemberSound.Init(memberSound);
            _disposables.Add(lingoMemberSound);
            return memberSound;
        }
        /// <inheritdoc/>
        public LingoFilmLoopMember CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var impl = new LingoGodotFilmLoopMember();
            var member = new LingoFilmLoopMember(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            impl.Init(member);
            _disposables.Add(impl);
            return member;
        }
        /// <inheritdoc/>
        public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var impl = new LingoGodotMemberShape();
            var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
            _disposables.Add(impl);
            return member;
        }
        /// <inheritdoc/>
        public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberBitmap(_serviceProvider.GetRequiredService<ILogger<LingoGodotMemberBitmap>>());
            var lingoInstance = new LingoMemberBitmap((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        /// <inheritdoc/>
        public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberField(_serviceProvider.GetRequiredService<ILingoFontManager>(), _serviceProvider.GetRequiredService<ILogger<LingoGodotMemberField>>());
            var lingoInstance = new LingoMemberField((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        /// <inheritdoc/>
        public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberText(_serviceProvider.GetRequiredService<ILingoFontManager>(), _serviceProvider.GetRequiredService<ILogger<LingoGodotMemberText>>());
            var lingoInstance = new LingoMemberText((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        /// <inheritdoc/>
        public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default)
        {
            var godotInstance = new LingoFrameworkMemberScript();
            var lingoInstance = new LingoMemberScript(godotInstance, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return lingoInstance;
        }
        /// <inheritdoc/>
        public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default)
        {
            var godotInstance = new LingoFrameworkMemberEmpty();
            var lingoInstance = new LingoMember(godotInstance, LingoMemberType.Empty, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return lingoInstance;
        }
        #endregion


        /// <inheritdoc/>
        public LingoStage CreateStage(LingoPlayer lingoPlayer)
        {
            var stageContainer = (LingoGodotStageContainer)_serviceProvider.GetRequiredService<ILingoFrameworkStageContainer>();
            var godotInstance = new LingoGodotStage(lingoPlayer);
            var lingoInstance = new LingoStage(godotInstance);
            stageContainer.SetStage(godotInstance);
            godotInstance.Init(lingoInstance, lingoPlayer);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        /// <inheritdoc/>
        public LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie)
        {
            var godotStage = stage.Framework<LingoGodotStage>();
            var godotInstance = new LingoGodotMovie(godotStage, lingoMovie, m => _disposables.Remove(m));
            lingoMovie.Init(godotInstance);
            _disposables.Add(godotInstance);

            return lingoMovie;
        }

        /// <summary>
        /// Dependant on movie, because the behaviors are scoped and movie related.
        /// </summary>
        public LingoSprite2D CreateSprite2D(ILingoMovie movie, Action<LingoSprite2D> onRemoveMe)
        {
            var movieTyped = (LingoMovie)movie;
            var lingoSprite = new LingoSprite2D(((LingoMovie)movie).GetEnvironment(), movie);
            lingoSprite.SetOnRemoveMe(onRemoveMe);
            movieTyped.Framework<LingoGodotMovie>().CreateSprite(lingoSprite);
            return lingoSprite;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        /// <inheritdoc/>
        public LingoStageMouse CreateMouse(LingoStage stage)
        {
            LingoStageMouse? mouse = null;
            var godotInstance = _lingoRootNode.GetStageMouseNode(() => mouse!);
            mouse = new LingoStageMouse(stage, godotInstance);
            return mouse;
        }

        /// <inheritdoc/>
        public LingoKey CreateKey()
        {
            LingoKey? key = null;
            var impl = new LingoGodotKey(_rootNode, new Lazy<LingoKey>(() => key!));
            key = new LingoKey(impl);
            impl.SetLingoKey(key);
            return key;
        }


        #region Gfx elements
        public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
            => _gfxFactory.CreateGfxCanvas(name, width, height);

        public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
            => _gfxFactory.CreateWrapPanel(orientation, name);

        public LingoGfxPanel CreatePanel(string name)
            => _gfxFactory.CreatePanel(name);

        public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y)
            => _gfxFactory.CreateLayoutWrapper(content, x, y);

        public LingoGfxTabContainer CreateTabContainer(string name)
            => _gfxFactory.CreateTabContainer(name);

        public LingoGfxTabItem CreateTabItem(string name, string title)
            => _gfxFactory.CreateTabItem(name, title);

        public LingoGfxScrollContainer CreateScrollContainer(string name)
            => _gfxFactory.CreateScrollContainer(name);

        public LingoGfxInputSlider<float> CreateInputSliderFloat(LingoOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
            => _gfxFactory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);

        public LingoGfxInputSlider<int> CreateInputSliderInt(LingoOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
            => _gfxFactory.CreateInputSliderInt(orientation, name, min, max, step, onChange);

        public LingoGfxInputSlider<TValue> CreateInputSlider<TValue>(string name, LingoOrientation orientation, NullableNum<TValue> min, NullableNum<TValue> max, NullableNum<TValue> step, Action<TValue>? onChange = null)
            where TValue : struct, System.Numerics.INumber<TValue>, IConvertible
            => _gfxFactory.CreateInputSlider(name, orientation, min, max, step, onChange);

        public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
            => _gfxFactory.CreateInputText(name, maxLength, onChange);

        public LingoGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
            => _gfxFactory.CreateInputNumberFloat(name, min, max, onChange);

        public LingoGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
            => _gfxFactory.CreateInputNumberInt(name, min, max, onChange);

        public LingoGfxInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
            where TValue : System.Numerics.INumber<TValue>
            => _gfxFactory.CreateInputNumber(name, min, max, onChange);

        public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
            => _gfxFactory.CreateSpinBox(name, min, max, onChange);

        public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
            => _gfxFactory.CreateInputCheckbox(name, onChange);

        public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
            => _gfxFactory.CreateInputCombobox(name, onChange);

        public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
            => _gfxFactory.CreateItemList(name, onChange);

        public LingoGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null)
            => _gfxFactory.CreateColorPicker(name, onChange);

        public LingoGfxLabel CreateLabel(string name, string text = "")
            => _gfxFactory.CreateLabel(name, text);

        public LingoGfxButton CreateButton(string name, string text = "")
            => _gfxFactory.CreateButton(name, text);

        public LingoGfxStateButton CreateStateButton(string name, ILingoTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
            => _gfxFactory.CreateStateButton(name, texture, text, onChange);

        public LingoGfxMenu CreateMenu(string name)
            => _gfxFactory.CreateMenu(name);

        public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
            => _gfxFactory.CreateMenuItem(name, shortcut);

        public LingoGfxMenu CreateContextMenu(object window)
            => _gfxFactory.CreateContextMenu(window);

        public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
            => _gfxFactory.CreateHorizontalLineSeparator(name);

        public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
            => _gfxFactory.CreateVerticalLineSeparator(name);

        public LingoGfxWindow CreateWindow(string name, string title = "")
            => _gfxFactory.CreateWindow(name, title);

        #endregion


    }
}
