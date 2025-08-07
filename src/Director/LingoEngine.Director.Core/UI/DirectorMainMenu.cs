using LingoEngine.Core;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using System.Collections.Generic;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.Icons;

namespace LingoEngine.Director.Core.UI
{
    /// <summary>
    /// Framework independent implementation of the Director main menu.
    /// </summary>
    public class DirectorMainMenu : DirectorWindow<IDirFrameworkMainMenuWindow>
    {
        private readonly LingoGfxWrapPanel _menuBar;
        private readonly LingoGfxWrapPanel _iconBar;
        private readonly LingoGfxMenu _fileMenu;
        private readonly LingoGfxMenu _editMenu;
        private readonly LingoGfxMenu _insertMenu;
        private readonly LingoGfxMenu _modifyMenu;
        private readonly LingoGfxMenu _controlMenu;
        private readonly LingoGfxMenu _windowMenu;
        private readonly LingoGfxButton _fileButton;
        private readonly LingoGfxButton _editButton;
        private readonly LingoGfxButton _insertButton;
        private LingoGfxButton _ModifyButton;
        private readonly LingoGfxButton _ControlButton;
        private readonly LingoGfxButton _windowButton;
        private LingoGfxStateButton _playButton;
        private readonly IDirectorWindowManager _windowManager;
        private readonly DirectorProjectManager _projectManager;
        private readonly LingoPlayer _player;
        private readonly IDirectorShortCutManager _shortCutManager;
        private readonly IHistoryManager _historyManager;
        private readonly List<ShortCutInfo> _shortCuts = new();
        private LingoGfxMenuItem _undoItem;
        private LingoGfxMenuItem _redoItem;
        private ILingoMovie? _lingoMovie;
        private List<LingoGfxButton> _topMenuButtons = new List<LingoGfxButton>();
        private List<LingoGfxMenu> _topMenus = new List<LingoGfxMenu>();
        private bool _playPauseState;

        public LingoGfxWrapPanel MenuBar => _menuBar;
        public LingoGfxWrapPanel IconBar => _iconBar;

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
            public DirectorShortCutMap Map { get; init; } = null!;
            public string Key { get; init; } = string.Empty;
            public bool Ctrl { get; init; }
            public bool Alt { get; init; }
            public bool Shift { get; init; }
            public bool Meta { get; init; }
        }

        public DirectorMainMenu(IDirectorWindowManager windowManager,DirectorProjectManager projectManager, LingoPlayer player, IDirectorShortCutManager shortCutManager,
            IHistoryManager historyManager, IDirectorIconManager directorIconManager,ILingoFrameworkFactory factory) : base(factory)
        {
            _windowManager = windowManager;
            _projectManager = projectManager;
            _player = player;
            _shortCutManager = shortCutManager;
            _historyManager = historyManager;

            _menuBar = factory.CreateWrapPanel(LingoOrientation.Horizontal, "MenuBar");
            _iconBar = factory.CreateWrapPanel(LingoOrientation.Horizontal, "IconBar");
            _iconBar.Height = 18;

            _fileMenu = factory.CreateMenu("FileMenu");
            _editMenu = factory.CreateMenu("EditMenu");
            _insertMenu = factory.CreateMenu("InsertMenu");
            _modifyMenu = factory.CreateMenu("ModifyMenu");
            _controlMenu = factory.CreateMenu("ControlMenu");
            _windowMenu = factory.CreateMenu("WindowMenu");
            // menu buttons
            _fileButton = factory.CreateButton("FileButton", "File");
            _editButton = factory.CreateButton("EditButton", "Edit");
            _insertButton = factory.CreateButton("InsertButton", "Insert");
            _ModifyButton = factory.CreateButton("ModifyButton", "Modify");
            _ControlButton = factory.CreateButton("ControlButton", "Control");
            _windowButton = factory.CreateButton("WindowButton", "Window");

            // icon buttons
            _iconBar
                .Compose()
                .AddVLine("VLine1", 16, 2)
                .AddButton("RewindButton", "", DoRewind, c => c.IconTexture = directorIconManager.Get(DirectorIcon.Rewind))
                .AddStateButton("RewindButton", this,directorIconManager.Get(DirectorIcon.Stop),p => p.PlayPauseState,"", 
                    c =>
                    {
                        c.TextureOff = directorIconManager.Get(DirectorIcon.Play);
                        _playButton = c;
                    })
                .AddVLine("VLine2", 16, 2)
            ;

            _topMenus.Add(_fileMenu);
            _topMenus.Add(_editMenu);
            _topMenus.Add(_insertMenu);
            _topMenus.Add(_modifyMenu);
            _topMenus.Add(_controlMenu);
            _topMenus.Add(_windowMenu);
            
            _topMenuButtons.Add(_fileButton);
            _topMenuButtons.Add(_editButton);
            _topMenuButtons.Add(_insertButton);
            _topMenuButtons.Add(_ModifyButton);
            _topMenuButtons.Add(_ControlButton);
            _topMenuButtons.Add(_windowButton);


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

            ComposeMenu(factory);

            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded += OnShortCutAdded;
            _shortCutManager.ShortCutRemoved += OnShortCutRemoved;
            _player.Key.Subscribe(this);

            UpdatePlayButton();
            foreach (var sc in _shortCutManager.GetShortCuts())
                _shortCuts.Add(ParseShortCut(sc));
        }

       

        public void CallOnAllTopMenuButtons(Action<LingoGfxButton> btnAction)
        {
            foreach (var item in _topMenuButtons)
                btnAction(item); 
        } public void CallOnAllTopMenus(Action<LingoGfxMenu> action)
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

        private void OnShortCutAdded(DirectorShortCutMap map)
            => _shortCuts.Add(ParseShortCut(map));

        private void OnShortCutRemoved(DirectorShortCutMap map)
            => _shortCuts.RemoveAll(s => s.Map == map);

        private ShortCutInfo ParseShortCut(DirectorShortCutMap map)
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

        private void ShowMenu(LingoGfxMenu menu, LingoGfxButton button)
        {
            menu.PositionPopup(button);
            menu.Popup();
        }

        public void UpdateUndoRedoState()
        {
            _undoItem.Enabled = _historyManager.CanUndo;
            _redoItem.Enabled = _historyManager.CanRedo;
        }

      

        public override void Dispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded -= OnShortCutAdded;
            _shortCutManager.ShortCutRemoved -= OnShortCutRemoved;
            _player.Key.Unsubscribe(this);
            base.Dispose();
        }

        protected override void OnRaiseKeyDown(LingoKey key)
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

        protected override void OnRaiseKeyUp(LingoKey key) { }
    }
}
