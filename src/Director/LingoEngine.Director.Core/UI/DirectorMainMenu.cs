using LingoEngine.Core;
using LingoEngine.Director.Core.Projects;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.Events;
using System;
using System.Collections.Generic;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.Icons;
using AbstUI.Commands;
using LingoEngine.Director.Core.Compilers.Commands;
using LingoEngine.Net.RNetContracts;
using AbstUI.Primitives;
using LingoEngine.Director.Core.Tools.Commands;
using AbstUI.Windowing;
using AbstUI.Inputs;
using AbstUI.Tools;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using LingoEngine.Director.Core.Importer.Commands;
using LingoEngine.Director.Core.Remote.Commands;
using LingoEngine.Net.RNetProjectHost;
using LingoEngine.Net.RNetProjectClient;
using System;
using System.ComponentModel;

namespace LingoEngine.Director.Core.UI
{
    /// <summary>
    /// Framework independent implementation of the Director main menu.
    /// </summary>
    public class DirectorMainMenu : DirectorWindow<IDirFrameworkMainMenuWindow>
    {
        private readonly AbstWrapPanel _menuBar;
        private readonly AbstWrapPanel _iconBar;
        private readonly AbstMenu _fileMenu;
        private readonly AbstMenu _editMenu;
        private readonly AbstMenu _insertMenu;
        private readonly AbstMenu _modifyMenu;
        private readonly AbstMenu _controlMenu;
        private readonly AbstMenu _windowMenu;
        private readonly AbstMenu _remoteMenu;
        private readonly AbstButton _fileButton;
        private readonly AbstButton _editButton;
        private readonly AbstButton _insertButton;
        private AbstButton _ModifyButton;
        private readonly AbstButton _ControlButton;
        private readonly AbstButton _windowButton;
        private readonly AbstButton _remoteButton;
        private AbstStateButton _playButton;
        private readonly IAbstWindowManager _windowManager;
        private readonly DirectorProjectManager _projectManager;
        private readonly LingoPlayer _player;
        private readonly IAbstShortCutManager _shortCutManager;
        private readonly IHistoryManager _historyManager;
        private readonly IAbstCommandManager _commandManager;
        private readonly IRNetProjectServer _server;
        private readonly ILingoRNetProjectClient _client;
        private readonly ILingoFrameworkFactory _factory;
        private readonly List<ShortCutInfo> _shortCuts = new();
        private AbstMenuItem _undoItem;
        private AbstMenuItem _redoItem;
        private AbstMenuItem _hostItem = null!;
        private AbstMenuItem _clientItem = null!;
        private ILingoMovie? _lingoMovie;
        private List<AbstButton> _topMenuButtons = new List<AbstButton>();
        private List<AbstMenu> _topMenus = new List<AbstMenu>();
        private bool _playPauseState;

        public AbstWrapPanel MenuBar => _menuBar;
        public AbstWrapPanel IconBar => _iconBar;

        public bool PlayPauseState
        {
            get => _playPauseState;
            set
            {
                var hasChanged = _playPauseState != value;
                _playPauseState = value;
                if (hasChanged)
                    SetPlayState(value);
            }
        }

        private class ShortCutInfo
        {
            public AbstShortCutMap Map { get; init; } = null!;
            public string Key { get; init; } = string.Empty;
            public bool Ctrl { get; init; }
            public bool Alt { get; init; }
            public bool Shift { get; init; }
            public bool Meta { get; init; }
        }

        public DirectorMainMenu(IServiceProvider serviceProvider, IAbstWindowManager windowManager, DirectorProjectManager projectManager, LingoPlayer player, IAbstShortCutManager shortCutManager,
            IHistoryManager historyManager, IDirectorIconManager directorIconManager, IAbstCommandManager commandManager, ILingoFrameworkFactory factory,
            IRNetProjectServer server, ILingoRNetProjectClient client) : base(serviceProvider, DirectorMenuCodes.MainMenu)
        {
            _windowManager = windowManager;
            _projectManager = projectManager;
            _player = player;
            _shortCutManager = shortCutManager;
            _historyManager = historyManager;
            _commandManager = commandManager;
            _server = server;
            _client = client;
            _factory = factory;

            _server.ConnectionStatusChanged += OnServerStateChanged;
            _client.ConnectionStatusChanged += OnClientStateChanged;

            _menuBar = factory.CreateWrapPanel(AOrientation.Horizontal, "MenuBar");
            _iconBar = factory.CreateWrapPanel(AOrientation.Horizontal, "IconBar");
            _iconBar.Height = 20;

            _fileMenu = factory.CreateMenu("FileMenu");
            _editMenu = factory.CreateMenu("EditMenu");
            _insertMenu = factory.CreateMenu("InsertMenu");
            _modifyMenu = factory.CreateMenu("ModifyMenu");
            _controlMenu = factory.CreateMenu("ControlMenu");
            _windowMenu = factory.CreateMenu("WindowMenu");
            _remoteMenu = factory.CreateMenu("RemoteMenu");
            // menu buttons
            _fileButton = factory.CreateButton("FileButton", "File");
            _editButton = factory.CreateButton("EditButton", "Edit");
            _insertButton = factory.CreateButton("InsertButton", "Insert");
            _ModifyButton = factory.CreateButton("ModifyButton", "Modify");
            _ControlButton = factory.CreateButton("ControlButton", "Control");
            _windowButton = factory.CreateButton("WindowButton", "Window");
            _remoteButton = factory.CreateButton("RemoteButton", "Remote");

            // icon buttons
            _iconBar
                .ComposeForToolBar()
                .AddButton("CompileButton", "", () => _commandManager.Handle(new CompileProjectCommand()), c => c.IconTexture = directorIconManager.Get(DirectorIcon.Script))
                .AddVLine()
                .AddButton("RewindButton", "", DoRewind, c => c.IconTexture = directorIconManager.Get(DirectorIcon.Rewind))
                .AddStateButton("RewindButton", this, directorIconManager.Get(DirectorIcon.Stop), p => p.PlayPauseState,
                    c =>
                    {
                        c.TextureOff = directorIconManager.Get(DirectorIcon.Play);
                        _playButton = c;
                    })
                .AddVLine()
                .AddButton("Show" + DirectorMenuCodes.StageWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.StageWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowStage))
                .AddButton("Show" + DirectorMenuCodes.CastWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.CastWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowCast))
                .AddButton("Show" + DirectorMenuCodes.ScoreWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.ScoreWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowScore))
                .AddButton("Show" + DirectorMenuCodes.PropertyInspector, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.PropertyInspector), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowProperty))
                .AddVLine()
                .AddButton("Show" + DirectorMenuCodes.PictureEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.PictureEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowPaint))
                .AddButton("Show" + DirectorMenuCodes.ShapeEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.ShapeEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowPath))
                .AddButton("Show" + DirectorMenuCodes.TextEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.TextEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowText))

            //.AddVLine("VLine4", 16, 2)
            //.AddButton("Show"+ DirectorMenuCodes.TextEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.TextEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowText))
            ;

            _topMenus.Add(_fileMenu);
            _topMenus.Add(_editMenu);
            _topMenus.Add(_insertMenu);
            _topMenus.Add(_modifyMenu);
            _topMenus.Add(_controlMenu);
            _topMenus.Add(_windowMenu);
            _topMenus.Add(_remoteMenu);

            _topMenuButtons.Add(_fileButton);
            _topMenuButtons.Add(_editButton);
            _topMenuButtons.Add(_insertButton);
            _topMenuButtons.Add(_ModifyButton);
            _topMenuButtons.Add(_ControlButton);
            _topMenuButtons.Add(_windowButton);
            _topMenuButtons.Add(_remoteButton);


            CallOnAllTopMenuButtons(x =>
            {
                _menuBar.AddItem(x);
            });

            _fileButton.Pressed += () => ShowMenu(_fileMenu, _fileButton);
            _editButton.Pressed += () => { UpdateUndoRedoState(); ShowMenu(_editMenu, _editButton); };
            _insertButton.Pressed += () => ShowMenu(_insertMenu, _insertButton);
            _ModifyButton.Pressed += () => ShowMenu(_modifyMenu, _ModifyButton);
            _ControlButton.Pressed += () => ShowMenu(_controlMenu, _ControlButton);
            _windowButton.Pressed += () => ShowMenu(_windowMenu, _windowButton);
            _remoteButton.Pressed += () => ShowMenu(_remoteMenu, _remoteButton);

            ComposeMenu(factory);

            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded += OnShortCutAdded;
            _shortCutManager.ShortCutRemoved += OnShortCutRemoved;
            _player.Key.Subscribe(this);

            UpdatePlayButton();
            foreach (var sc in _shortCutManager.GetShortCuts())
                _shortCuts.Add(ParseShortCut(sc));
        }



        public void CallOnAllTopMenuButtons(Action<AbstButton> btnAction)
        {
            foreach (var item in _topMenuButtons)
                btnAction(item);
        }
        public void CallOnAllTopMenus(Action<AbstMenu> action)
        {
            foreach (var item in _topMenus)
                action(item);
        }

        private void ComposeMenu(ILingoFrameworkFactory factory)
        {
            CreateFileMenu(factory);
            CreateEditMenu(factory);
            CreateInsertMenu(factory);
            CreateModifyMenu(factory);
            CreateControlMenu(factory);
            CreateWindowMenu(factory);
            CreateRemoteMenu(factory);
        }

        private void CreateFileMenu(ILingoFrameworkFactory factory)
        {
            // File Menu
            var load = factory.CreateMenuItem("Load");
            load.Activated += () => _projectManager.LoadMovie();
            _fileMenu.AddItem(load);

            var save = factory.CreateMenuItem("Save");
            save.Activated += () => _projectManager.SaveMovie();
            _fileMenu.AddItem(save);

            var ie = factory.CreateMenuItem("Import/Export");
            ie.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ImportExportWindow);
            _fileMenu.AddItem(ie);

            var importer = factory.CreateMenuItem("Lingo code importer");
            importer.Activated += () => _commandManager.Handle(new OpenLingoCodeImporterCommand());
            _fileMenu.AddItem(importer);

            var quit = factory.CreateMenuItem("Quit");
            quit.Activated += () => Environment.Exit(0);
            _fileMenu.AddItem(quit);
        }

        private void CreateEditMenu(ILingoFrameworkFactory factory)
        {
            // Edit Menu
            _undoItem = factory.CreateMenuItem("Undo\tCTRL+Z");
            _undoItem.Activated += () => _historyManager.Undo();
            _editMenu.AddItem(_undoItem);

            _redoItem = factory.CreateMenuItem("Redo\tCTRL+Y");
            _redoItem.Activated += () => _historyManager.Redo();
            _editMenu.AddItem(_redoItem);

            var projectSettings = factory.CreateMenuItem("Project Settings");
            projectSettings.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            _editMenu.AddItem(projectSettings);

            var cConverter = factory.CreateMenuItem("Lingo to # Converter");
            cConverter.Activated += () => _commandManager.Handle(new OpenLingoCSharpConverterCommand());
            _editMenu.AddItem(cConverter);
        }



        private void CreateInsertMenu(ILingoFrameworkFactory factory)
        {
            var menuItem = factory.CreateMenuItem("Menu TODO");
            menuItem.Activated += () => { };
            _insertMenu.AddItem(menuItem); ;
        }
        private void CreateModifyMenu(ILingoFrameworkFactory factory)
        {
            var menuItem = factory.CreateMenuItem("Menu TODO");
            menuItem.Activated += () => { };
            _modifyMenu.AddItem(menuItem);
        }
        private void CreateControlMenu(ILingoFrameworkFactory factory)
        {
            var menuItem = factory.CreateMenuItem("Menu TODO");
            menuItem.Activated += () => { };
            _controlMenu.AddItem(menuItem);
        }
        private void CreateWindowMenu(ILingoFrameworkFactory factory)
        {
            // Window Menu
            var stage = factory.CreateMenuItem("Stage  \tCTRL+1");
            stage.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.StageWindow);
            _windowMenu.AddItem(stage);

            var cast = factory.CreateMenuItem("Cast  \tCTRL+3");
            cast.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.CastWindow);
            _windowMenu.AddItem(cast);

            var score = factory.CreateMenuItem("Score  \tCTRL+4");
            score.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow);
            _windowMenu.AddItem(score);

            var text = factory.CreateMenuItem("Text \tCTRL+T");
            text.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.TextEditWindow);
            _windowMenu.AddItem(text);

            var inspector = factory.CreateMenuItem("Property Inspector  \tCTRL+ALT+S");
            inspector.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector);
            _windowMenu.AddItem(inspector);

            var tools = factory.CreateMenuItem("Tools  \tCTRL+7");
            tools.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow);
            _windowMenu.AddItem(tools);

            var binary = factory.CreateMenuItem("Binary Viewer");
            binary.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.BinaryViewerWindow);
            _windowMenu.AddItem(binary);

            var binaryV2 = factory.CreateMenuItem("Binary Viewer V2");
            binaryV2.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.BinaryViewerWindowV2);
            _windowMenu.AddItem(binaryV2);

            var paint = factory.CreateMenuItem("Paint  \tCTRL+5");
            paint.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.PictureEditWindow);
            _windowMenu.AddItem(paint);

            var behaviorInspectorWindow = factory.CreateMenuItem("Behavior Inspector \tCTRL+5");
            behaviorInspectorWindow.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.BehaviorInspectorWindow);
            _windowMenu.AddItem(behaviorInspectorWindow);
        }

        private void CreateRemoteMenu(ILingoFrameworkFactory factory)
        {
            var settings = factory.CreateMenuItem("Settings");
            settings.Activated += () => _commandManager.Handle(new OpenRNetSettingsCommand());
            _remoteMenu.AddItem(settings);

            _hostItem = factory.CreateMenuItem(_server.IsEnabled ? "Stop Host" : "Start Host");
            _hostItem.Activated += () =>
                _commandManager.Handle(_server.IsEnabled
                    ? new DisconnectRNetServerCommand()
                    : new ConnectRNetServerCommand());
            _remoteMenu.AddItem(_hostItem);

            _clientItem = factory.CreateMenuItem(_client.IsConnected ? "Stop Client" : "Start Client");
            _clientItem.Activated += () =>
                _commandManager.Handle(_client.IsConnected
                    ? new DisconnectRNetClientCommand()
                    : new ConnectRNetClientCommand());
            _remoteMenu.AddItem(_clientItem);
        }

        private void OnServerStateChanged(LingoNetConnectionState state)
        {
            _hostItem.Name = state == LingoNetConnectionState.Connected ? "Stop Host" : "Start Host";
        }

        private void OnClientStateChanged(LingoNetConnectionState state)
        {
            _clientItem.Name = state == LingoNetConnectionState.Connected ? "Stop Client" : "Start Client";
        }


        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            if (_lingoMovie != null)
            {
                _lingoMovie.PlayStateChanged -= OnPlayStateChanged;
            }
            _lingoMovie = movie;
            if (_lingoMovie != null)
            {
                _lingoMovie.PlayStateChanged += OnPlayStateChanged;
            }
            UpdatePlayButton();
        }
        private void DoRewind()
        {
            _lingoMovie?.GoTo(1);
        }

        private void OnPlayStateChanged(bool isPlaying) => UpdatePlayButton();

        private bool _playPauseFromEvent;
        private void UpdatePlayButton()
        {
            _playPauseFromEvent = true;
            PlayPauseState = _lingoMovie != null && _lingoMovie.IsPlaying;
            _playButton.IsOn = PlayPauseState;
            _playPauseFromEvent = false;
        }
        private void SetPlayState(bool state)
        {
            if (_playPauseFromEvent) return; // Prevent recursive calls from the button state change
            if (_lingoMovie == null) return;
            if (_lingoMovie.IsPlaying && state) return;
            if (_lingoMovie.IsPlaying)
                _lingoMovie.Halt();
            else
                _lingoMovie.Play();
        }

        private void OnShortCutAdded(AbstShortCutMap map)
            => _shortCuts.Add(ParseShortCut(map));

        private void OnShortCutRemoved(AbstShortCutMap map)
            => _shortCuts.RemoveAll(s => s.Map == map);

        private ShortCutInfo ParseShortCut(AbstShortCutMap map)
        {
            bool ctrl = false, alt = false, shift = false, meta = false;
            string key = string.Empty;
            var parts = map.KeyCombination.Split('+');
            foreach (var p in parts)
            {
                var token = p.Trim();
                if (token.Equals("CTRL", StringComparison.OrdinalIgnoreCase)) ctrl = true;
                else if (token.Equals("ALT", StringComparison.OrdinalIgnoreCase)) alt = true;
                else if (token.Equals("SHIFT", StringComparison.OrdinalIgnoreCase)) shift = true;
                else if (token.Equals("CMD", StringComparison.OrdinalIgnoreCase) || token.Equals("META", StringComparison.OrdinalIgnoreCase)) meta = true;
                else key = token;
            }
            return new ShortCutInfo { Map = map, Key = key.ToUpperInvariant(), Ctrl = ctrl, Alt = alt, Shift = shift, Meta = meta };
        }

        private void ShowMenu(AbstMenu menu, AbstButton button)
        {
            menu.PositionPopup(button);
            menu.Popup();
        }

        public void UpdateUndoRedoState()
        {
            _undoItem.Enabled = _historyManager.CanUndo;
            _redoItem.Enabled = _historyManager.CanRedo;
        }



        protected override void OnDispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded -= OnShortCutAdded;
            _shortCutManager.ShortCutRemoved -= OnShortCutRemoved;
            _player.Key.Unsubscribe(this);
            _server.ConnectionStatusChanged -= OnServerStateChanged;
            _client.ConnectionStatusChanged -= OnClientStateChanged;
            base.OnDispose();
        }

        protected override void OnRaiseKeyDown(AbstKeyEvent key)
        {
            var label = key.Key.ToUpperInvariant();
            bool ctrl = key.ControlDown;
            bool alt = key.OptionDown;
            bool shift = key.ShiftDown;
            bool meta = key.CommandDown;

            foreach (var sc in _shortCuts)
            {
                if (sc.Key == label && sc.Ctrl == ctrl && sc.Alt == alt && sc.Shift == shift && sc.Meta == meta)
                {
                    _shortCutManager.Execute(sc.Map.KeyCombination);
                    break;
                }
            }
        }

        protected override void OnRaiseKeyUp(AbstKeyEvent key) { }
    }
}
