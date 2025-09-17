using Godot;
using System;
using System.Numerics;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.LGodot.Movies;
using BlingoEngine.LGodot.Sounds;
using BlingoEngine.LGodot.Texts;
using BlingoEngine.LGodot.Shapes;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using BlingoEngine.LGodot.Stages;
using BlingoEngine.Members;
using BlingoEngine.Casts;
using BlingoEngine.Shapes;
using BlingoEngine.Bitmaps;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using AbstUI.Styles;
using Microsoft.Extensions.Logging;
using BlingoEngine.LGodot.Bitmaps;
using BlingoEngine.LGodot.Scripts;
using BlingoEngine.Scripts;
using BlingoEngine.FilmLoops;
using BlingoEngine.LGodot.FilmLoops;
using BlingoEngine.Medias;
using BlingoEngine.LGodot.Medias;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.LGodot.Styles;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.LGodot.Components;
using BlingoEngine.Transitions.TransitionLibrary;

namespace BlingoEngine.LGodot.Core
{
    public class GodotFactory : IBlingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly IBlingoServiceProvider _serviceProvider;
        private readonly BlingoGodotRootNode _blingoRootNode;
        private Node _rootNode;

        private readonly GodotComponentFactory _gfxFactory;

        public GodotFactory(IBlingoServiceProvider serviceProvider, BlingoGodotRootNode rootNode)
        {
            _blingoRootNode = rootNode;
            _rootNode = rootNode.RootNode;
            _serviceProvider = serviceProvider;
            var fontManager = _serviceProvider.GetRequiredService<IAbstFontManager>();
            var styleManager = _serviceProvider.GetRequiredService<IAbstStyleManager>();
            var godotStyleManager = _serviceProvider.GetRequiredService<IAbstGodotStyleManager>();
            _gfxFactory = _serviceProvider.GetRequiredService<GodotComponentFactory>(); // new GodotGfxFactory(styleManager, fontManager, godotStyleManager, _blingoRootNode);
        }

        public IAbstComponentFactory GfxFactory => _gfxFactory;

        public IAbstComponentFactory ComponentFactory => _gfxFactory;


        #region Sound

        /// <inheritdoc/>
        public BlingoSound CreateSound(IBlingoCastLibsContainer castLibsContainer)
        {
            var blingoSound = new BlingoGodotSound();
            var soundChannel = new BlingoSound(blingoSound, castLibsContainer, this);
            blingoSound.Init(soundChannel);
            return soundChannel;
        }
        /// <inheritdoc/>
        public BlingoSoundChannel CreateSoundChannel(int number)
        {
            var blingoSoundChannel = new BlingoGodotSoundChannel(number, _rootNode);
            var soundChannel = new BlingoSoundChannel(blingoSoundChannel, number);
            blingoSoundChannel.Init(soundChannel);
            _disposables.Add(blingoSoundChannel);
            return soundChannel;
        }
        #endregion


        #region Members
        /// <inheritdoc/>
        public T CreateMember<T>(IBlingoCast cast, int numberInCast, string name = "") where T : BlingoMember
        {

            return typeof(T) switch
            {
                Type t when t == typeof(BlingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoFilmLoopMember) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoMemberShape) => (CreateMemberShape(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoMemberQuickTimeMedia) => (CreateMemberQuickTimeMedia(cast, numberInCast, name) as T)!,
                Type t when t == typeof(BlingoMemberRealMedia) => (CreateMemberRealMedia(cast, numberInCast, name) as T)!,
            };
        }
        /// <inheritdoc/>
        public BlingoMemberSound CreateMemberSound(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var blingoMemberSound = new BlingoGodotMemberSound();
            var memberSound = new BlingoMemberSound(blingoMemberSound, (BlingoCast)cast, numberInCast, name, fileName ?? "");
            blingoMemberSound.Init(memberSound);
            _disposables.Add(blingoMemberSound);
            return memberSound;
        }
        /// <inheritdoc/>
        public BlingoFilmLoopMember CreateMemberFilmLoop(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var impl = new BlingoGodotFilmLoopMember();
            var member = new BlingoFilmLoopMember(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            impl.Init(member);
            _disposables.Add(impl);
            return member;
        }
        /// <inheritdoc/>
        public BlingoMemberShape CreateMemberShape(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var impl = new BlingoGodotMemberShape();
            var member = new BlingoMemberShape((BlingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
            _disposables.Add(impl);
            return member;
        }
        /// <inheritdoc/>
        public BlingoMemberQuickTimeMedia CreateMemberQuickTimeMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var impl = new BlingoGodotMemberMedia();
            var member = new BlingoMemberQuickTimeMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            impl.Init(member);
            _disposables.Add(impl);
            return member;
        }
        /// <inheritdoc/>
        public BlingoMemberRealMedia CreateMemberRealMedia(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var impl = new BlingoGodotMemberMedia();
            var member = new BlingoMemberRealMedia(impl, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            impl.Init(member);
            _disposables.Add(impl);
            return member;
        }
        /// <inheritdoc/>
        public BlingoMemberBitmap CreateMemberBitmap(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var godotInstance = new BlingoGodotMemberBitmap(_serviceProvider.GetRequiredService<ILogger<BlingoGodotMemberBitmap>>());
            var blingoInstance = new BlingoMemberBitmap((BlingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(blingoInstance);
            _disposables.Add(godotInstance);
            return blingoInstance;
        }
        /// <inheritdoc/>
        public BlingoMemberField CreateMemberField(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var godotInstance = new BlingoGodotMemberField(_serviceProvider.GetRequiredService<IAbstFontManager>(), _serviceProvider.GetRequiredService<ILogger<BlingoGodotMemberField>>());
            var blingoInstance = new BlingoMemberField((BlingoCast)cast, godotInstance, numberInCast, ComponentFactory, name, fileName ?? "", regPoint);
            godotInstance.Init(blingoInstance);
            _disposables.Add(godotInstance);
            return blingoInstance;
        }
        /// <inheritdoc/>
        public BlingoMemberText CreateMemberText(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default)
        {
            var godotInstance = new BlingoGodotMemberText(_serviceProvider.GetRequiredService<IAbstFontManager>(), _serviceProvider.GetRequiredService<ILogger<BlingoGodotMemberText>>());
            var blingoInstance = new BlingoMemberText((BlingoCast)cast, godotInstance, numberInCast, ComponentFactory, name, fileName ?? "", regPoint);
            godotInstance.Init(blingoInstance);
            _disposables.Add(godotInstance);
            return blingoInstance;
        }
        /// <inheritdoc/>
        public BlingoMember CreateScript(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null,
            APoint regPoint = default)
        {
            var godotInstance = new BlingoFrameworkMemberScript();
            var blingoInstance = new BlingoMemberScript(godotInstance, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return blingoInstance;
        }
        /// <inheritdoc/>
        public BlingoMember CreateEmpty(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null,
            APoint regPoint = default)
        {
            var godotInstance = new BlingoFrameworkMemberEmpty();
            var blingoInstance = new BlingoMember(godotInstance, BlingoMemberType.Empty, (BlingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return blingoInstance;
        }
        #endregion


        /// <inheritdoc/>
        public BlingoStage CreateStage(BlingoPlayer blingoPlayer)
        {
            var stageContainer = (BlingoGodotStageContainer)_serviceProvider.GetRequiredService<IBlingoFrameworkStageContainer>();
            var godotInstance = new BlingoGodotStage(blingoPlayer);
            var blingoInstance = new BlingoStage(godotInstance);
            stageContainer.SetStage(godotInstance);
            godotInstance.Init(blingoInstance, blingoPlayer);
            _disposables.Add(godotInstance);
            return blingoInstance;
        }
        /// <inheritdoc/>
        public BlingoMovie AddMovie(BlingoStage stage, BlingoMovie blingoMovie)
        {
            var godotStage = stage.Framework<BlingoGodotStage>();
            var godotInstance = new BlingoGodotMovie(godotStage, blingoMovie, m => _disposables.Remove(m));
            blingoMovie.Init(godotInstance);
            _disposables.Add(godotInstance);

            return blingoMovie;
        }

        /// <summary>
        /// Dependant on movie, because the behaviors are scoped and movie related.
        /// </summary>
        public BlingoSprite2D CreateSprite2D(IBlingoMovie movie, Action<BlingoSprite2D> onRemoveMe)
        {
            var movieTyped = (BlingoMovie)movie;
            var blingoSprite = new BlingoSprite2D(((BlingoMovie)movie).GetEnvironment(), movie);
            blingoSprite.SetOnRemoveMe(onRemoveMe);
            movieTyped.Framework<BlingoGodotMovie>().CreateSprite(blingoSprite);
            return blingoSprite;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        /// <inheritdoc/>
        public BlingoStageMouse CreateMouse(BlingoStage stage)
        {
            BlingoStageMouse? mouse = null;
            var godotInstance = _blingoRootNode.GetStageMouseNode(() => mouse!);
            mouse = new BlingoStageMouse(stage, (IBlingoFrameworkMouse)godotInstance);
            return mouse;
        }

        /// <inheritdoc/>
        public BlingoKey CreateKey()
        {
            BlingoKey? key = null;
            var impl = new BlingoGodotKey(_rootNode, new Lazy<AbstKey>(() => key!));
            key = new BlingoKey(impl);
            impl.SetKeyObj(key);
            return key;
        }


        #region Gfx elements
        public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
            => _gfxFactory.CreateGfxCanvas(name, width, height);

        public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
            => _gfxFactory.CreateWrapPanel(orientation, name);

        public AbstPanel CreatePanel(string name)
            => _gfxFactory.CreatePanel(name);

        public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
            => _gfxFactory.CreateLayoutWrapper(content, x, y);

        public AbstTabContainer CreateTabContainer(string name)
            => _gfxFactory.CreateTabContainer(name);

        public AbstTabItem CreateTabItem(string name, string title)
            => _gfxFactory.CreateTabItem(name, title);

        public AbstScrollContainer CreateScrollContainer(string name)
            => _gfxFactory.CreateScrollContainer(name);

        public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
            => _gfxFactory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);

        public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
            => _gfxFactory.CreateInputSliderInt(orientation, name, min, max, step, onChange);

        public AbstInputSlider<TValue> CreateInputSlider<TValue>(string name, AOrientation orientation, NullableNum<TValue> min, NullableNum<TValue> max, NullableNum<TValue> step, Action<TValue>? onChange = null)
            where TValue : struct, System.Numerics.INumber<TValue>, IConvertible
            => _gfxFactory.CreateInputSlider(name, orientation, min, max, step, onChange);

        public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
            => _gfxFactory.CreateInputText(name, maxLength, onChange, multiLine);

        public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
            => _gfxFactory.CreateInputNumberFloat(name, min, max, onChange);

        public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
            => _gfxFactory.CreateInputNumberInt(name, min, max, onChange);

        public AbstInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
            where TValue : System.Numerics.INumber<TValue>
            => _gfxFactory.CreateInputNumber(name, min, max, onChange);

        public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
            => _gfxFactory.CreateSpinBox(name, min, max, onChange);

        public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
            => _gfxFactory.CreateInputCheckbox(name, onChange);

        public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
            => _gfxFactory.CreateInputCombobox(name, onChange);

        public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
            => _gfxFactory.CreateItemList(name, onChange);

        public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
            => _gfxFactory.CreateColorPicker(name, onChange);

        public AbstLabel CreateLabel(string name, string text = "")
            => _gfxFactory.CreateLabel(name, text);

        public AbstButton CreateButton(string name, string text = "")
            => _gfxFactory.CreateButton(name, text);

        public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
            => _gfxFactory.CreateStateButton(name, texture, text, onChange);

        public AbstMenu CreateMenu(string name)
            => _gfxFactory.CreateMenu(name);

        public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
            => _gfxFactory.CreateMenuItem(name, shortcut);

        public AbstMenu CreateContextMenu(object window)
            => _gfxFactory.CreateContextMenu(window);

        public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
            => _gfxFactory.CreateHorizontalLineSeparator(name);

        public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
            => _gfxFactory.CreateVerticalLineSeparator(name);



        #endregion


    }
}

