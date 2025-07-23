﻿using Godot;
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
using System;
using System.Xml.Linq;
using LingoEngine.LGodot.Styles;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.LGodot.Scripts;
using LingoEngine.Scripts;

namespace LingoEngine.LGodot.Core
{
    public class GodotFactory : ILingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly IServiceProvider _serviceProvider;
        private Node _rootNode;

        public GodotFactory(IServiceProvider serviceProvider, LingoGodotRootNode rootNode)
        {
            _rootNode = rootNode.RootNode;
            _serviceProvider = serviceProvider;
        }

        public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => lingoMovie.GetServiceProvider().GetRequiredService<T>();
        public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => lingoMovie.GetServiceProvider().GetRequiredService<T>();

        #region Sound

        public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer)
        {
            var lingoSound = new LingoGodotSound();
            var soundChannel = new LingoSound(lingoSound, castLibsContainer, this);
            lingoSound.Init(soundChannel);
            return soundChannel;
        }
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
        public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember
        {

            return typeof(T) switch
            {
                Type t when t == typeof(LingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberFilmLoop) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberShape) => (CreateMemberShape(cast, numberInCast, name) as T)!,
            };
        }
        public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var lingoMemberSound = new LingoGodotMemberSound();
            var memberSound = new LingoMemberSound(lingoMemberSound, (LingoCast)cast, numberInCast, name, fileName ?? "");
            lingoMemberSound.Init(memberSound);
            _disposables.Add(lingoMemberSound);
            return memberSound;
        }
        public LingoMemberFilmLoop CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var impl = new LingoGodotMemberFilmLoop();
            var member = new LingoMemberFilmLoop(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            impl.Init(member);
            _disposables.Add(impl);
            return member;
        }
        public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var impl = new LingoGodotMemberShape();
            var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
            _disposables.Add(impl);
            return member;
        }
        public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberBitmap(_serviceProvider.GetRequiredService<ILogger<LingoGodotMemberBitmap>>());
            var lingoInstance = new LingoMemberBitmap((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberField(_serviceProvider.GetRequiredService<ILingoFontManager>(), _serviceProvider.GetRequiredService<ILogger<LingoGodotMemberField>>());
            var lingoInstance = new LingoMemberField((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberText(_serviceProvider.GetRequiredService<ILingoFontManager>(),_serviceProvider.GetRequiredService<ILogger<LingoGodotMemberText>>());
            var lingoInstance = new LingoMemberText((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMember CreateScript(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default)
        {
            var godotInstance = new LingoFrameworkMemberScript();
            var lingoInstance = new LingoMemberScript(godotInstance, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return lingoInstance;
        }
        public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default)
        {
            var godotInstance = new LingoFrameworkMemberEmpty();
            var lingoInstance = new LingoMember(godotInstance, LingoMemberType.Empty, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return lingoInstance;
        }
        #endregion


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
        public T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite> onRemoveMe) where T : LingoSprite
        {
            var movieTyped = (LingoMovie)movie;
            var lingoSprite = movieTyped.GetServiceProvider().GetRequiredService<T>();
            lingoSprite.SetOnRemoveMe(onRemoveMe);
            movieTyped.Framework<LingoGodotMovie>().CreateSprite(lingoSprite);
            return lingoSprite;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        public LingoMouse CreateMouse(LingoStage stage)
        {
            LingoMouse? mouse = null;
            var godotInstance = new LingoGodotMouse(_rootNode, new Lazy<LingoMouse>(() => mouse!));
            mouse = new LingoMouse(stage, godotInstance);
            return mouse;
        }

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
        {
            var canvas = new LingoGfxCanvas();
            var impl = new LingoGodotGfxCanvas(canvas, _serviceProvider.GetRequiredService<ILingoFontManager>(), width, height);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
        {
            var panel = new LingoGfxWrapPanel();
            var impl = new LingoGodotWrapPanel(panel, orientation);

            panel.Name = name;
            // Ensure the public wrapper reflects the initial orientation
            panel.Orientation = orientation;
            return panel;
        }

        public LingoGfxPanel CreatePanel(string name)
        {
            var panel = new LingoGfxPanel(this);
            var impl = new LingoGodotPanel(panel);

            panel.Name = name;
            return panel;
        }
        public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y)
        {
            if (content is ILingoGfxLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new LingoGfxLayoutWrapper(content);
            var impl = new LingoGodotLayoutWrapper(panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public LingoGfxTabContainer CreateTabContainer(string name)
        {
            var tab = new LingoGfxTabContainer();
            var impl = new LingoGodotTabContainer(tab, _serviceProvider.GetRequiredService<ILingoGodotStyleManager>());

            tab.Name = name;
            return tab;
        }
        public LingoGfxTabItem CreateTabItem(string name, string title)
        {
            var tab = new LingoGfxTabItem();
            var impl = new LingoGodotTabItem(tab);
            tab.Title = title;
            tab.Name = name;
            return tab;
        }

        public LingoGfxScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new LingoGfxScrollContainer();
            var impl = new LingoGodotScrollContainer(scroll);
            scroll.Name = name;
            return scroll;
        }

        public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
        {
            var input = new LingoGfxInputText();
            var impl = new LingoGodotInputText(input, _serviceProvider.GetRequiredService<ILingoFontManager>(), onChange);
            input.MaxLength = maxLength;
            input.Name = name;
            return input;
        }

        public LingoGfxInputNumber CreateInputNumber(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var input = new LingoGfxInputNumber();
            var impl = new LingoGodotInputNumber(input, _serviceProvider.GetRequiredService<ILingoFontManager>(), onChange);
            if (min.HasValue) input.Min = min.Value;
            if (max.HasValue) input.Max = max.Value;
            input.Name = name;
            return input;
        }

        public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new LingoGfxSpinBox();
            var impl = new LingoGodotSpinBox(spin, _serviceProvider.GetRequiredService<ILingoFontManager>(), onChange);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            return spin;
        }

        public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new LingoGfxInputCheckbox();
            var impl = new LingoGodotInputCheckbox(input, onChange);
            input.Name = name;
            return input;
        }

        public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new LingoGfxInputCombobox();
            var impl = new LingoGodotInputCombobox(input, _serviceProvider.GetRequiredService<ILingoFontManager>(), onChange);

            input.Name = name;
            return input;
        }

        public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new LingoGfxItemList();
            var impl = new LingoGodotItemList(list, onChange);
            list.Name = name;
            return list;
        }

        public LingoGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null)
        {
            var picker = new LingoGfxColorPicker();
            var impl = new LingoGodotColorPicker(picker, onChange);
            picker.Name = name;
            return picker;
        }

        public LingoGfxLabel CreateLabel(string name, string text = "")
        {
            var label = new LingoGfxLabel();
            var impl = new LingoGodotLabel(label, _serviceProvider.GetRequiredService<ILingoFontManager>());
            label.Text = text;

            label.Name = name;
            return label;
        }

        public LingoGfxButton CreateButton(string name, string text = "") //, Action? onClick = null)
        {
            var button = new LingoGfxButton();
            var impl = new LingoGodotButton(button, _serviceProvider.GetRequiredService<ILingoFontManager>());
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            return button;
        }

        public LingoGfxStateButton CreateStateButton(string name, ILingoImageTexture? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new LingoGfxStateButton();
            var impl = new LingoGodotStateButton(button, onChange);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            if (texture != null)
                button.Texture = texture;
            return button;
        }

        public LingoGfxMenu CreateMenu(string name)
        {
            var menu = new LingoGfxMenu();
            var impl = new LingoGodotMenu(menu, name);
            return menu;
        }

        public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new LingoGfxMenuItem();
            var impl = new LingoGodotMenuItem(item,name, shortcut);
            return item;
        }

        public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            var sep = new LingoGfxHorizontalLineSeparator();
            var impl = new LingoGodotHorizontalLineSeparator(sep);
            sep.Name = name;
            return sep;
        }

        public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            var sep = new LingoGfxVerticalLineSeparator();
            var impl = new LingoGodotVerticalLineSeparator(sep);
            sep.Name = name;
            return sep;
        }

        public LingoGfxWindow CreateWindow(string name, string title = "")
        {
            var win = new LingoGfxWindow();
            var impl = new LingoGodotWindow(win, _serviceProvider.GetRequiredService<ILingoGodotStyleManager>());
            win.Name = name;
            if (!string.IsNullOrWhiteSpace(title))
                win.Title = title;
            return win;
        }


        #endregion


    }
}
