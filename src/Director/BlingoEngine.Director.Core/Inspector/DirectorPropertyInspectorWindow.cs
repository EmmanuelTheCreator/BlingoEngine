﻿using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.Tools;
using AbstUI.Windowing;
using AbstUI.Windowing.Commands;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Inspector.Commands;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FilmLoops;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;
using BlingoEngine.Scripts;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Tempos;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace BlingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>, IHasSpriteSelectedEvent, IHasMemberSelectedEvent,
        IAbstCommandHandler<OpenBehaviorPopupCommand>
    {
        public enum PropetyTabNames
        {
            Movie,
            Sprite,
            Guides,
            Behavior,
            Member,
            Bitmap,
            Sound,
            Shape,
            Cast,
            Text,
            FilmLoop,
        }
        public const int HeaderHeight = 44;
        private AbstLabel? _sprite;
        private AbstLabel? _member;
        private AbstLabel? _cast;
        private BlingoPlayer _player;
        private IAbstCommandManager _commandManager;
        private AbstTabContainer _tabs = null!;
        private DirectorMemberThumbnail? _thumb;
        private AbstPanel? _header;
        private IDirectorIconManager _iconManager;
        private AbstPanel _headerPanel = null!;
        private AbstPanel _rootPanel = null!;
        private IDirectorEventMediator _mediator;
        private readonly IDirectorBehaviorDescriptionManager _descriptionManager;
        private readonly ILogger<DirectorPropertyInspectorWindow> _logger;
        private readonly DirectorStageGuides _guides;
        private AbstWrapPanel _behaviorPanel = null!;
        private float _lastWidh;
        private float _lastHeight;
        private Dictionary<string, BlingoSpriteBehavior> _behaviors = new();
        private AbstItemList _behaviorList = null!;
        private IAbstWindowDialogReference? _behaviorWindow;

        public AbstPanel HeaderPanel => _headerPanel;
        public AbstTabContainer Tabs => _tabs;
        public string SpriteText { get => _sprite?.Text ?? string.Empty; set { if (_sprite != null) _sprite.Text = value; } }
        public string MemberText { get => _member?.Text ?? string.Empty; set { if (_member != null) _member.Text = value; } }
        public string CastText { get => _cast?.Text ?? string.Empty; set { if (_cast != null) _cast.Text = value; } }

        public record HeaderElements(AbstPanel Panel, AbstWrapPanel Header, DirectorMemberThumbnail Thumbnail);

        public DirectorPropertyInspectorWindow(IServiceProvider serviceProvider, BlingoPlayer player, IAbstCommandManager commandManager, IBlingoFrameworkFactory factory, IDirectorIconManager iconManager, IDirectorEventMediator mediator, IDirectorBehaviorDescriptionManager descriptionManager, DirectorStageGuides guides, ILogger<DirectorPropertyInspectorWindow> logger) : base(serviceProvider, DirectorMenuCodes.PropertyInspector)
        {
            _player = player;
            _commandManager = commandManager;
            _iconManager = iconManager;
            _mediator = mediator;
            _descriptionManager = descriptionManager;
            _guides = guides;
            _logger = logger;
            _mediator.Subscribe(this);
            Width = 260;
            Height = 450;
            MinimumWidth = 260;
            MinimumHeight = 200;
            X = 1530;
            Y = 22;
            _lastWidh = Width;
            _lastHeight = Height;
            _tabs = _factory.CreateTabContainer("InspectorTabs");
            _tabs.Y = HeaderHeight;
            _rootPanel = _factory.CreatePanel("InspectorRoot");
            _rootPanel.BackgroundColor = DirectorColors.BG_WhiteMenus;
        }
        protected override void OnDispose()
        {
            base.OnDispose();
            _mediator.Unsubscribe(this);
        }
        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            Title = "Property Inspector";
            CreateHeaderElements();
            CreateBehaviorPanel();

            _headerPanel.Y = 0;
            _rootPanel.AddItem(_headerPanel);
            _rootPanel.AddItem(_tabs);
            Content = _rootPanel;

            AddMovieTab(_player.ActiveMovie);
        }


        private AbstPanel CreateHeaderElements()
        {
            var thumb = new DirectorMemberThumbnail(36, 36, _factory, _iconManager);

            var thumbPanel = _factory.CreatePanel("ThumbPanel");
            thumbPanel.X = 4;
            thumbPanel.Y = 2;
            thumbPanel.BackgroundColor = DirectorColors.Bg_Thumb;
            thumbPanel.BorderColor = DirectorColors.Border_Thumb;
            thumbPanel.BorderWidth = 1;
            thumbPanel.AddItem(thumb.Canvas);
            _thumb = thumb;

            var container = _factory.CreatePanel("InfoContainer");
            container.X = 50;

            _sprite = container.SetLabelAt("SpriteLabel", 0, 0);
            _member = container.SetLabelAt("MemberLabel", 0, 13);
            _cast = container.SetLabelAt("MemberLabel", 0, 26);


            var header = _factory.CreatePanel("HeaderPanel");
            header.BackgroundColor = DirectorColors.BG_WhiteMenus;
            header.AddItem(thumbPanel);
            header.AddItem(container);


            _headerPanel = _factory.CreatePanel("RootHeaderPanel");
            _headerPanel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _headerPanel.AddItem(header);
            _headerPanel.Height = HeaderHeight;
            _header = header;
            return _headerPanel;
        }






        public void SpriteSelected(IBlingoSpriteBase sprite) => ShowObject(sprite);
        public void MemberSelected(IBlingoMember member) => ShowObject(member);


        protected override void OnResizing(bool firstLoad, int width, int height)
        {
            base.OnResizing(firstLoad, width, height);
            _lastWidh = width;
            _lastHeight = height;
            if (_tabs == null || _header == null)
                return;

            _rootPanel.Width = width;
            _rootPanel.Height = height;
            _headerPanel.Width = width;
            _header.Width = width - 10;
            _header.Height = HeaderHeight;
            _tabs.Width = width - 10;
            _tabs.Height = height - 30 - HeaderHeight;
            _behaviorList.Width = _lastWidh - 15;
        }



        public void ShowObject(object obj)
        {
            if (_tabs == null || _thumb == null)
                return;
            PropetyTabNames lastSelectedTab = PropetyTabNames.Movie;
            if (!string.IsNullOrWhiteSpace(_tabs.SelectedTabName))
                lastSelectedTab = Enum.Parse<PropetyTabNames>(_tabs.SelectedTabName);
            _tabs.ClearTabs();
            IBlingoMember? member = null;
            if (obj is BlingoSprite2D sp)
            {
                if (lastSelectedTab == PropetyTabNames.Movie || lastSelectedTab == PropetyTabNames.Cast)
                    lastSelectedTab = PropetyTabNames.Sprite;
                member = sp.Member;
                if (member != null)
                {
                    _thumb.SetMember(member);
                    SpriteText = $"Sprite {sp.SpriteNum}: {member.Type}";
                }
            }
            else if (obj is IBlingoMember m)
            {
                member = m;
                _thumb.SetMember(member);
                SpriteText = member.Type.ToString();

                if (lastSelectedTab == PropetyTabNames.Movie || lastSelectedTab == PropetyTabNames.Sprite)
                    lastSelectedTab = PropetyTabNames.Member;
            }
            if (member != null)
            {
                MemberText = $"{member.NumberInCast}. {member.Name}";
                CastText = member.Cast.Name;
            }
            switch (obj)
            {
                case BlingoSprite2D sp2:
                    AddSpriteTab(sp2);
                    AddGuidesTab(_guides);
                    if (sp2.Member != null)
                        AddMemberTabs(sp2.Member);

                    break;
                case IBlingoMember member2:
                    AddMemberTabs(member2);
                    AddCastTab(member2.Cast);
                    break;
                case IBlingoCast cast: AddCastTab(cast); break;
                case BlingoSpriteSound sound:
                    AddSpriteTab(sound);
                    if (sound.Sound != null)
                    {
                        AddMemberTabs(sound.Sound);
                        AddSoundTab(sound.Sound);
                    }
                    break;
                case BlingoTempoSprite tempo: AddSpriteTab(tempo); break;
                case BlingoColorPaletteSprite colorPalette: AddSpriteTab(colorPalette); if (colorPalette.Member != null) AddMemberTabs(colorPalette.Member); break;
                case BlingoTransitionSprite transition: AddSpriteTab(transition); if (transition.Member != null) AddMemberTabs(transition.Member); break;
                case BlingoFrameScriptSprite frameScript: AddSpriteTab(frameScript); if (frameScript.Member != null) AddMemberTabs(frameScript.Member); break;

                default:
                    //AddTab(obj.GetType().Name, obj);
                    break;
            }
            if (obj is BlingoSprite && _player.ActiveMovie != null)
                AddMovieTab(_player.ActiveMovie);

            try
            {
                if (_tabs.GetChildren().Any(x => x.Name == lastSelectedTab.ToString()))
                    _tabs.SelectTabByName(lastSelectedTab.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting tab:" + lastSelectedTab + ":" + ex.Message);
            }
        }



        private void AddMemberTabs(IBlingoMember member)
        {
            AddMemberTab(member);
            switch (member)
            {
                case BlingoMemberText text: AddTextTab(text); break;
                case BlingoMemberField text: AddTextTab(text); break;
                case BlingoMemberBitmap pic: AddBitmapTab(pic); break;
                case BlingoMemberSound sound: AddSoundTab(sound); break;
                case BlingoMemberShape shape: AddShapeTab(shape); break;
                case BlingoFilmLoopMember film: AddTab(PropetyTabNames.FilmLoop, film); break;
            }
        }



        #region Sprite Tab
        private void AddSpriteTab(BlingoSprite sprite)
        {
            CreateBehaviorPanel();
            var wrapContainer = AddTab(PropetyTabNames.Sprite);
            var containerIcons = _factory.CreateWrapPanel(AOrientation.Horizontal, "SpriteDetailIcons");
            var container = _factory.CreatePanel("SpriteDetailPanel");
            container.BackgroundColor = DirectorColors.BG_WhiteMenus;

            containerIcons.Margin = new AMargin(5, 5, 5, 5);
            var composer0 = containerIcons.Compose()
                .AddStateButton("SpriteLock", sprite, _iconManager.Get(DirectorIcon.Lock), c => c.Lock)
                ;
            if (sprite is BlingoSprite2D sprite2D0)
                composer0
                    .AddStateButton("SpriteFlipH", sprite2D0, _iconManager.Get(DirectorIcon.FlipHorizontal), c => c.FlipH, "")
                    .AddStateButton("SpriteFlipV", sprite2D0, _iconManager.Get(DirectorIcon.FlipVertical), c => c.FlipV)
                    ;
            composer0.Finalize();

            var composer = container.Compose(_factory.ComponentFactory)
                   .Columns(4)
                   .AddTextInput("SpriteName", "Name:", sprite, s => s.Name, inputSpan: 3)
                   .Columns(8);
            if (sprite is BlingoSprite2D sprite2D)
                composer
                       .AddNumericInputFloat("SpriteLocH", "X:", sprite2D, s => s.LocH)
                       .AddNumericInputFloat("SpriteLocV", "Y:", sprite2D, s => s.LocV)
                       .AddNumericInputFloat("SpriteLocZ", "Z:", sprite2D, s => s.LocZ, inputSpan: 3)
                       .AddNumericInputFloat("SpriteLeft", "L:", sprite2D, s => s.Left)
                       .AddNumericInputFloat("SpriteTop", "T:", sprite2D, s => s.Top)
                       .AddNumericInputFloat("SpriteRight", "R:", sprite2D, s => s.Right)
                       .AddNumericInputFloat("SpriteBottom", "B:", sprite2D, s => s.Bottom)
                       .AddNumericInputFloat("SpriteWidth", "W:", sprite2D, s => s.Width)
                       .AddNumericInputFloat("SpriteHeight", "H:", sprite2D, s => s.Height, inputSpan: 5)
                       .AddEnumInput<BlingoSprite2D, BlingoInkType>("SpriteInk", "Ink:", sprite2D, s => s.Ink, inputSpan: 6)
                       .AddNumericInputFloat("SpriteBlend", "%", sprite2D, s => s.Blend, showLabel: false)
                       ;
            composer
                   .AddNumericInputInt("SpriteBeginFrame", "StartFrame:", sprite, s => s.BeginFrame, labelSpan: 3)
                   .AddNumericInputInt("SpriteEndFrame", "End:", sprite, s => s.EndFrame, inputSpan: 1, labelSpan: 3)
                   ;
            _behaviorList.ClearItems();
            _behaviors.Clear();
            if (sprite is BlingoSprite2D sprite2D1)
            {
                composer
                   .AddNumericInputFloat("SpriteRotation", "Rotation:", sprite2D1, s => s.Rotation, labelSpan: 3)
                   .AddNumericInputFloat("SpriteSkew", "Skew:", sprite2D1, s => s.Skew, inputSpan: 1, labelSpan: 3)
                   .AddColorPicker("SpriteForeColor", "Foreground:", sprite2D1, s => s.ForeColor, inputSpan: 1, labelSpan: 3)
                   .AddColorPicker("SpriteBackColor", "Background:", sprite2D1, s => s.BackColor, inputSpan: 1, labelSpan: 3)

                ;
                var index = 0;
                _behaviors = sprite2D1.Behaviors.ToDictionary(b =>
                {
                    index++;
                    return $"{index}.{b.Name} {(b.ScriptMember != null ? $"{b.ScriptMember.CastLibNum},{b.ScriptMember.NumberInCast}" : "")}";
                });
            }
            if (sprite is BlingoFrameScriptSprite frameScript && frameScript.Behavior != null)
                _behaviors.Add("1." + frameScript.Behavior.Name + $"{(frameScript.Member != null ? $"{frameScript.Member.CastLibNum},{frameScript.Member.NumberInCast}" : "")}", frameScript.Behavior);

            foreach (var item in _behaviors)
                _behaviorList.AddItem(item.Key, item.Value.Name);


            composer.Finalize();
            wrapContainer
                .AddItem(containerIcons)
                .AddHLine("SpriteSplitterIconHLine", _lastWidh - 10, 5)
                .AddItem(container)
                .AddHLine("SpriteSplitterIconHLine", _lastWidh - 10, 5)
                .AddItem(_behaviorPanel)
                ;
        }

        private void CreateBehaviorPanel()
        {
            _behaviorPanel = _factory.CreateWrapPanel(AOrientation.Vertical, "InspectorBehaviors");

            _behaviorList = _factory.CreateItemList("BehaviorList", x =>
            {
                if (x != null && _behaviors.TryGetValue(x, out var behavior))
                    _commandManager.Handle(new OpenBehaviorPopupCommand(behavior));
            });
            _behaviorList.Height = 45;
            _behaviorList.Width = _lastWidh - 15;
            _behaviorList.Margin = new AMargin(5, 0, 0, 0);
            _behaviorPanel.AddItem(_behaviorList);

        }


        public IAbstWindowDialogReference? BuildBehaviorPopup(BlingoSpriteBehavior behavior)
            => _descriptionManager.BuildBehaviorPopup(behavior, () =>
            {
                _behaviorList.SelectedIndex = -1;
            });

        public void ShowBehaviorPopup(IAbstWindowDialogReference window)
        {
            _behaviorWindow?.Dialog?.Dispose();
            _behaviorWindow = window;
            //window.PopupCentered();
        }

        public bool CanExecute(OpenBehaviorPopupCommand command) => true;

        public bool Handle(OpenBehaviorPopupCommand command)
        {
            var win = BuildBehaviorPopup(command.Behavior);
            if (win == null) return true;
            ShowBehaviorPopup(win);
            return true;
        }

        #endregion



        #region Memeber/Bitmap

        private void AddMemberTab(IBlingoMember member)
        {
            var wrapContainer = AddTab(PropetyTabNames.Member);
            var container = _factory.CreatePanel("MemberDetailPanel");
            wrapContainer
                .AddItem(container)
                ;

            container.Compose(_factory.ComponentFactory)
                   .Columns(4)
                   .AddTextInput("MemberName", "Name:", member, s => s.Name, inputSpan: 3)
                   .Columns(4)
                   .AddLabel("MemberSize", "Size: ", 2)
                   .AddLabel("MemberSizeV", CommonExtensions.BytesToShortString(member.Size), 2)
                   .AddLabel("MemberCreationDate", "Created: ", 2)
                   .AddLabel("MemberCreationDateV", member.CreationDate.ToString("dd/MM/yyyy HH:mm"), 2)
                   .AddLabel("MemberModifyDate", "Modified: ", 2)
                   .AddLabel("MemberModifyDateV", member.ModifiedDate.ToString("dd/MM/yyyy HH:mm"), 2)
                   .Columns(4)
                   .AddTextInput("MemberFileName", "FileName:", member, s => s.FileName, inputSpan: 3)
                   .Columns(4)
                   .AddTextInput("MemberComments", "Comments:", member, s => s.Comments, inputSpan: 3)
                   .Finalize()
                   ;
        }
        private void AddBitmapTab(BlingoMemberBitmap member)
        {
            var wrapContainer = AddTab(PropetyTabNames.Bitmap);
            var container = _factory.CreatePanel("MemberDetailPanel");
            wrapContainer
                .AddItem(container)
                ;

            container.Compose(_factory.ComponentFactory)
                   .Columns(4)
                   .AddLabel("BitmapSize", "Dimensions: ", 2)
                   .AddLabel("BitmapSizeV", member.Width + " x " + member.Height, 2)
                   .AddCheckBox("BitmapHighLight", "Hightlight: ", member, x => x.Hilite, 2, true, 2)
                   //.AddLabel("BitmapBitDepth", "BitDepth: ", 2)
                   //.AddLabel("BitmapBitDepthV", member.ColorDepth,2)
                   .Columns(8)
                   .AddNumericInputFloat("BitmapRegPointX", "RegPoint X:", member, s => s.RegPoint.X, inputSpan: 1, labelSpan: 3)
                   .AddNumericInputFloat("BitmapRegPointY", "Y:", member, s => s.RegPoint.Y, inputSpan: 4, labelSpan: 1)
                   .Finalize()
                   ;
        }

        #endregion



        #region Sound

        private void AddSoundTab(BlingoMemberSound member)
        {
            var soundChannel = _player.Sound.Channel(1);
            if (soundChannel == null) return;
            var wrap = AddTab(PropetyTabNames.Sound);
            var btnPanel = _factory.CreateWrapPanel(AOrientation.Horizontal, "SoundButtons");
            var playBtn = _factory.CreateButton("SoundPlay", "Play");
            var stopBtn = _factory.CreateButton("SoundStop", "Stop");
            playBtn.Pressed += () => soundChannel.Play(member);
            stopBtn.Pressed += () => soundChannel.Stop();
            btnPanel.AddItem(playBtn);
            btnPanel.AddItem(stopBtn);
            AbstPanel panel = _factory.CreatePanel("SoundPanel");
            wrap.AddItem(btnPanel);
            wrap.AddItem(panel);

            string duration = TimeSpan.FromSeconds(member.Length).ToString(@"hh\:mm\:ss\.fff");
            panel.Compose(_factory.ComponentFactory)
                .Columns(4)
                .AddCheckBox("SoundLoop", "Loop:", member, m => m.Loop, 1, true, 3)
                .AddLabel("SoundDuration", "Duration: ", 2)
                .AddLabel("SoundDurationV", duration, 2)
                .AddLabel("SoundSampleRate", "Sample rate: ", 2)
                .AddLabel("SoundSampleRateV", soundChannel.SampleRate + " Hz", 2)
                .AddLabel("SoundBitDepth", "Bit Depth: ", 2)
                .AddLabel("SoundBitDepthV", "16", 2)
                .AddLabel("SoundChannels", "Channels: ", 2)
                .AddLabel("SoundChannelsV", member.Stereo ? "Stereo" : "Mono", 2)
                .Finalize();
        }

        #endregion



        #region Movie

        private void AddMovieTab(IBlingoMovie? movie)
        {
            var wrap = AddTab(PropetyTabNames.Movie);

            var rowSize = _factory.CreateWrapPanel(AOrientation.Horizontal, "MovieStageSizeRow");
            rowSize.Margin = new AMargin(5, 5, 5, 0);
            rowSize.Compose()
                //.AddButton("tesxtt", "Test", () =>
                //{
                //    var kb = _factory.CreateKeyboard(Inputs.BlingoJoystickKeyboard.BlingoKeyboardLayoutType.Azerty,true);
                //    kb.Open(new BlingoPoint(50,50));
                //    kb.EnterPressed += () =>
                //    {
                //        kb.Close();
                //    };
                //    kb.Closed += () =>
                //    {
                //        var text = kb.Text;
                //    };
                //})
                //.NewLine("t")
                .AddLabel("StageSizeLbl", "Stage size:")
                .AddNumericInputFloat("MovieStageWidth", _player.Stage, m => m.Width, 40)
                .AddLabel("StageSizeLblX", "x")
                .AddNumericInputFloat("MovieStageHeight", _player.Stage, m => m.Height, 40)
                .AddCombobox("MovieResolutions", new[]
                {
                    new KeyValuePair<string,string>("640x480","640x480"),
                    new KeyValuePair<string,string>("800x600","800x600"),
                    new KeyValuePair<string,string>("1024x768","1024x768"),
                    new KeyValuePair<string,string>("1280x720","1280x720"),
                    new KeyValuePair<string,string>("1920x1080","1920x1080")
                }, 90, $"{_player.Stage.Width}x{_player.Stage.Height}", val =>
                {
                    if (!string.IsNullOrEmpty(val))
                    {
                        var p = val.Split('x');
                        if (p.Length == 2 && int.TryParse(p[0], out var w) && int.TryParse(p[1], out var h))
                        {
                            _player.Stage.Width = w;
                            _player.Stage.Height = h;
                        }
                    }
                })
               .Finalize()
                ;
            wrap.AddItem(rowSize);

            if (movie != null)
            {
                // We create settings to not directly remove all sprites when changing the number, but only when pressed apply to not loose sprites to fast.
                var settings = new DirMovieUISettings(movie);
                var rowChannels = _factory.CreatePanel("MovieChannelsRow");
                rowChannels.BackgroundColor = DirectorColors.BG_WhiteMenus;
                rowChannels.Compose(_factory.ComponentFactory)
                    .Columns(4)
                    .AddNumericInputInt("MovieChannels", "Channels:", settings, m => settings.MaxSpriteChannelCount)
                    .AddColorPicker("StageBgColor", "Color", _player.Stage, m => m.BackgroundColor)
                    .Columns(2)
                    .AddButton("MovieApplyBtn", "Apply", () =>
                    {
                        settings.Apply();
                        _mediator.Raise(DirectorEventType.StagePropertiesChanged);
                        _mediator.Raise(DirectorEventType.CastPropertiesChanged);
                    })
                    .NextRow()
                    //    .Finalize()
                    //    ;
                    //wrap.AddItem(rowChannels);

                    //wrap.AddHLine(_factory,"HSplitterMovie");

                    //var rowAbouts = _factory.CreatePanel("MovieAbouts");
                    //rowAbouts.Compose(_factory)
                    .Columns(1)
                    .AddLabel("MovieAboutL", "About:")
                    .AddTextInput("MovieAbout", "About", movie, m => m.About, 1, 0)
                    .AddLabel("CopyrightL", "Copyright:")
                    .AddTextInput("MovieCopyright", "Copyright", movie, m => m.Copyright, 1, 0)
                    .Finalize()
                   ;
                wrap.AddItem(rowChannels);
            }
        }
        private class DirMovieUISettings
        {
            private readonly IBlingoMovie _movie;

            public DirMovieUISettings(IBlingoMovie movie)
            {
                MaxSpriteChannelCount = movie.MaxSpriteChannelCount;
                _movie = movie;
            }
            public void Apply()
            {
                _movie.MaxSpriteChannelCount = MaxSpriteChannelCount;
            }

            public int MaxSpriteChannelCount { get; set; }
        }


        #endregion

        #region CAST

        private void AddCastTab(IBlingoCast cast)
        {
            var wrap = AddTab(PropetyTabNames.Cast);
            var rowChannels = _factory.CreatePanel("CastRow");
            rowChannels.BackgroundColor = DirectorColors.BG_WhiteMenus;
            rowChannels.Margin = new AMargin(5, 5, 0, 0);
            rowChannels.Compose(_factory.ComponentFactory)
                   .Columns(8)
                   .NextRow()
                   .AddNumericInputInt("CastNumber", "Number:", cast, m => m.Number, 1, true, false, 2, c => c.Enabled = false)
                   .AddTextInput("CastName", "Name:", cast, m => m.Name, 3, 2)
                   .Finalize();
            ;
            wrap.AddItem(rowChannels);
        }
        #endregion


        #region TEXT
        private void AddTextTab(IBlingoMemberTextBase textMember)
        {
            var wrap = AddTab(PropetyTabNames.Text);
            var rowChannels = _factory.CreatePanel("TextRow");
            rowChannels.BackgroundColor = DirectorColors.BG_WhiteMenus;
            rowChannels.Margin = new AMargin(5, 5, 0, 0);
            rowChannels.Compose(_factory.ComponentFactory)
                   .NextRow()
                   .Columns(8)
                   .AddNumericInputFloat("TextWidth", "W:", textMember, s => s.Width)
                   .AddNumericInputFloat("TextHeight", "H:", textMember, s => s.Height, inputSpan: 5)

                   .NextRow()
                   .Columns(2)
                   .AddButton("EditText", "Edit", () =>
                   {
                       _commandManager?.Handle(new OpenWindowCommand(DirectorMenuCodes.TextEditWindow));
                   })
                   .Finalize();
            ;
            wrap.AddItem(rowChannels);
        }
        #endregion


        #region Shape

        private void AddShapeTab(BlingoMemberShape member)
        {
            var wrap = AddTab(PropetyTabNames.Shape);
            var rowChannels = _factory.CreatePanel("ShapeRow");
            rowChannels.Margin = new AMargin(5, 5, 0, 0);
            var composer = rowChannels.Compose(_factory.ComponentFactory)
                   .NextRow()
                   .Columns(8)
                   .AddEnumInput<BlingoMemberShape, BlingoShapeType>("ShapeType", "Shape:", member, s => s.ShapeTypeInt, inputSpan: 6, labelSpan: 2)
                   .AddCheckBox("ShapeClosed", "Filled:", member, s => s.Filled, inputSpan: 1, true)

                   .NextRow()
                   .AddNumericInputInt("ShapeWidth", "W:", member, s => s.Width)
                   .AddNumericInputInt("ShapeHeight", "H:", member, s => s.Height, inputSpan: 5)
                   ;
            if (member.ShapeType == BlingoShapeType.Rectangle || member.ShapeType == BlingoShapeType.Oval)
            {
                composer
                    .NextRow()
                       .AddButton("EditShape", "Edit", () =>
                       {
                           _commandManager?.Handle(new OpenWindowCommand(DirectorMenuCodes.ShapeEditWindow));
                       }, 6)
                       .NextRow()
                       ;
            }
            composer
                   .Finalize();
            ;
            wrap.AddItem(rowChannels);
        }

        #endregion


        #region Guides
        private void AddGuidesTab(DirectorStageGuides guides)
        {
            var wrap = AddTab(PropetyTabNames.Guides);
            var guidesPanel = _factory.CreatePanel("GuidesPanel");
            guidesPanel.Margin = new AMargin(5, 5, 0, 0);
            guidesPanel.Compose(_factory.ComponentFactory)
                   .Columns(4)
                   .AddColorPicker("GuideColor", "Color:", guides, g => g.GuidesColor)
                   .AddCheckBox("GuideVisible", "Visible:", guides, g => g.GuidesVisible)
                   .AddCheckBox("GuideSnap", "SnapTo:", guides, g => g.GuidesSnap)
                   .AddCheckBox("GuideLock", "Lock:", guides, g => g.GuidesLocked)
                   .Finalize();
            wrap.AddItem(guidesPanel);

            var gridPanel = _factory.CreatePanel("GridPanel");
            gridPanel.Margin = new AMargin(5, 5, 0, 0);
            gridPanel.Compose(_factory.ComponentFactory)
                   .Columns(4)
                   .AddColorPicker("GridColor", "Color:", guides, g => g.GridColor)
                   .AddCheckBox("GridVisible", "Visible:", guides, g => g.GridVisible)
                   .AddCheckBox("GridSnap", "SnapTo:", guides, g => g.GridSnap)
                   .NextRow()
                   .AddButton("AddVerticalGuide", "Add vertical", () => guides.AddVertical(0), 2)
                   .AddButton("AddHorizontalGuide", "Add horizontal", () => guides.AddHorizontal(0), 2)
                   .NextRow()
                   .AddButton("RemoveGuides", "Remove all", () => guides.RemoveAll(), 4)
                   .NextRow()
                   .AddNumericInputFloat("GridWidth", "W:", guides, g => g.GridWidth)
                   .AddNumericInputFloat("GridHeight", "H:", guides, g => g.GridHeight)
                   .Finalize();
            wrap.AddItem(gridPanel);
        }

        #endregion


        #region Common: AddTab

        private AbstWrapPanel AddTab(PropetyTabNames tabName)
        {
            var name = tabName.ToString();
            var scroller = _factory.CreateScrollContainer(name + "Scroll");
            AbstWrapPanel container = _factory.CreateWrapPanel(AOrientation.Vertical, name + "Container");
            scroller.AddItem(container);
            var tabItem = _factory.CreateTabItem(name, name);
            tabItem.Content = scroller;
            _tabs.AddTab(tabItem);
            return container;
        }


        private void AddTab(PropetyTabNames tabName, object obj)
        {
            if (_tabs == null)
                return;
            var name = tabName.ToString();
            var scroller = _factory.CreateScrollContainer(name + "Scroll");
            AbstWrapPanel container = _factory.CreateWrapPanel(AOrientation.Vertical, name + "Container");

            if (_commandManager != null && (obj is BlingoMemberBitmap || obj is IBlingoMemberTextBase))
            {
                var editBtn = _factory.CreateButton("EditButton", "Edit");
                editBtn.Pressed += () =>
                {
                    string code = obj switch
                    {
                        BlingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                        IBlingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                        _ => string.Empty
                    };
                    if (!string.IsNullOrEmpty(code))
                        _commandManager.Handle(new OpenWindowCommand(code));
                };
                container.AddItem(editBtn);
            }

            // TODO: behavior list
            //if (obj as BlingoSprite sprite)
            //    ShowBehavior(sprite)

            scroller.AddItem(container);
            var tabItem = _factory.CreateTabItem(name, name);
            tabItem.Content = scroller;
            _tabs.AddTab(tabItem);
        }


        #endregion


    }
}

