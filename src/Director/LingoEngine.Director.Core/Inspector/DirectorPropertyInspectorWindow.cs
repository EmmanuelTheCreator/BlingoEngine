using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Sounds;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Windowing.Commands;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using LingoEngine.Tools;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.Shapes;
using Microsoft.Extensions.Logging;

namespace LingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>, IHasSpriteSelectedEvent, IHasMemberSelectedEvent
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
        private LingoGfxLabel? _sprite;
        private LingoGfxLabel? _member;
        private LingoGfxLabel? _cast;
        private LingoPlayer _player;
        private ILingoCommandManager? _commandManager;
        private LingoGfxTabContainer _tabs;
        private DirectorMemberThumbnail? _thumb;
        private LingoGfxPanel? _header;
        private ILingoFrameworkFactory _factory;
        private IDirectorIconManager _iconManager;
        public ILingoFrameworkFactory Factory => _factory;
        private LingoGfxPanel _headerPanel;
        private IDirectorEventMediator _mediator;
        private readonly IDirectorBehaviorDescriptionManager _descriptionManager;
        private readonly ILogger<DirectorPropertyInspectorWindow> _logger;
        private readonly DirectorStageGuides _guides;
        private LingoGfxWrapPanel _behaviorPanel;
        private float _lastWidh;
        private float _lastHeight;
        private Dictionary<string, LingoSpriteBehavior> _behaviors = new();
        private LingoGfxItemList _behaviorList;
        public event Action<LingoSpriteBehavior>? BehaviorSelected;

        public LingoGfxPanel HeaderPanel => _headerPanel;
        public LingoGfxTabContainer Tabs => _tabs;
        public string SpriteText { get => _sprite?.Text ?? string.Empty; set { if (_sprite != null) _sprite.Text = value; } }
        public string MemberText { get => _member?.Text ?? string.Empty; set { if (_member != null) _member.Text = value; } }
        public string CastText { get => _cast?.Text ?? string.Empty; set { if (_cast != null) _cast.Text = value; } }

        public record HeaderElements(LingoGfxPanel Panel, LingoGfxWrapPanel Header, DirectorMemberThumbnail Thumbnail);

        public DirectorPropertyInspectorWindow(LingoPlayer player, ILingoCommandManager commandManager, ILingoFrameworkFactory factory, IDirectorIconManager iconManager, IDirectorEventMediator mediator, IDirectorBehaviorDescriptionManager descriptionManager, DirectorStageGuides guides, ILogger<DirectorPropertyInspectorWindow> logger)
        {
            _player = player;
            _commandManager = commandManager;
            _factory = factory;
            _iconManager = iconManager;
            _mediator = mediator;
            _descriptionManager = descriptionManager;
            _guides = guides;
            _logger = logger;
            _mediator.Subscribe(this);
        }
        public override void Dispose()
        {
            base.Dispose();
            _mediator.Unsubscribe(this);
        }
        public void Init(IDirFrameworkWindow frameworkWindow, float width, float height, int titleBarHeight)
        {
            base.Init(frameworkWindow);
            _lastHeight = height;
            _lastWidh = width;
            CreateHeaderElements();
            _tabs = _factory.CreateTabContainer("InspectorTabs");
            CreateBehaviorPanel();
            
            AddMovieTab(_player.ActiveMovie);
        }

      
        private LingoGfxPanel CreateHeaderElements()
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

            _sprite = container.SetLabelAt(_factory, "SpriteLabel", 0, 0);
            _member = container.SetLabelAt(_factory, "MemberLabel", 0, 13);
            _cast = container.SetLabelAt(_factory, "MemberLabel", 0, 26);


            var header = _factory.CreatePanel("HeaderPanel");
            header.AddItem(thumbPanel);
            header.AddItem(container);


            _headerPanel = _factory.CreatePanel("RootHeaderPanel");
            _headerPanel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _headerPanel.AddItem(header);
            _headerPanel.Height = HeaderHeight;
            _header = header;
            return _headerPanel;
        }

        

       


        public void SpriteSelected(ILingoSpriteBase sprite) => ShowObject(sprite);
        public void MemberSelected(ILingoMember member) => ShowObject(member);

        public void OnResizing(float width, float height)
        {
            _lastWidh = width;
            _lastHeight = height;
            if (_tabs == null || _header == null)
                return;

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
            var lastSelectedTab = Enum.Parse<PropetyTabNames>(_tabs.SelectedTabName);
            _tabs.ClearTabs();
            ILingoMember? member = null;
            if (obj is LingoSprite2D sp)
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
            else if (obj is ILingoMember m)
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
                case LingoSprite2D sp2:
                    AddSpriteTab(sp2);
                    AddGuidesTab(_guides);
                    if (sp2.Member != null)
                        AddMemberTabs(sp2.Member);
                    if (_player.ActiveMovie != null)
                        AddMovieTab(_player.ActiveMovie);
                    break;
                case ILingoMember member2:
                    AddMemberTabs(member2);
                    AddCastTab(member2.Cast);
                    break;
                case ILingoCast cast:AddCastTab(cast);break;

                default:
                    //AddTab(obj.GetType().Name, obj);
                    break;
            }

            try
            {
                if (_tabs.GetChildren().Any(x => x.Name == lastSelectedTab.ToString()))
                    _tabs.SelectTabByName(lastSelectedTab.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting tab:" + lastSelectedTab+":"+ex.Message);
            }
        }

      

        private void AddMemberTabs(ILingoMember member)
        {
            AddMemberTab(member);
            switch (member)
            {
                case LingoMemberText text:AddTextTab(text);break;
                case LingoMemberField text:AddTextTab(text);break;
                case LingoMemberBitmap pic:AddBitmapTab(pic);break;
                case LingoMemberSound sound:AddSoundTab(sound);break;
                case LingoMemberShape shape:AddShapeTab(shape);break;
                case LingoMemberFilmLoop film:AddTab(PropetyTabNames.FilmLoop, film);break;
            }
        }

        private void AddSpriteTab(LingoSprite2D sprite)
        {
            CreateBehaviorPanel();
            var wrapContainer = AddTab(PropetyTabNames.Sprite);
            var containerIcons = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "SpriteDetailIcons");
            var container = _factory.CreatePanel("SpriteDetailPanel");
            

            containerIcons.Margin = new LingoMargin(5, 5, 5, 5);
            containerIcons.Compose(_factory)
                .AddStateButton("SpriteLock", sprite, _iconManager.Get(DirectorIcon.Lock),c => c.Lock)
                .AddStateButton("SpriteFlipH", sprite, _iconManager.Get(DirectorIcon.FlipHorizontal), c => c.FlipH,"")
                .AddStateButton("SpriteFlipV", sprite, _iconManager.Get(DirectorIcon.FlipVertical), c => c.FlipV)
                ;

            container.Compose(_factory)
                   .Columns(4)
                   .AddTextInput("SpriteName", "Name:", sprite, s => s.Name, inputSpan: 3)
                   .Columns(8)
                   .AddNumericInput("SpriteLocH", "X:", sprite, s => s.LocH)
                   .AddNumericInput("SpriteLocV", "Y:", sprite, s => s.LocV)
                   .AddNumericInput("SpriteLocZ", "Z:", sprite, s => s.LocZ, inputSpan: 3)
                   .AddNumericInput("SpriteLeft", "L:", sprite, s => s.Left)
                   .AddNumericInput("SpriteTop", "T:", sprite, s => s.Top)
                   .AddNumericInput("SpriteRight", "R:", sprite, s => s.Right)
                   .AddNumericInput("SpriteBottom", "B:", sprite, s => s.Bottom)
                   .AddNumericInput("SpriteWidth", "W:", sprite, s => s.Width)
                   .AddNumericInput("SpriteHeight", "H:", sprite, s => s.Height, inputSpan: 5)
                   .AddEnumInput<LingoSprite2D,LingoInkType>("SpriteInk", "Ink:", sprite, s => s.Ink, inputSpan: 6)
                   .AddNumericInput("SpriteBlend", "%", sprite, s => s.Blend, showLabel: false)
                   .AddNumericInput("SpriteBeginFrame", "StartFrame:", sprite, s => s.BeginFrame, labelSpan: 3)
                   .AddNumericInput("SpriteEndFrame", "End:", sprite, s => s.EndFrame, inputSpan: 1, labelSpan: 3)
                   .AddNumericInput("SpriteRotation", "Rotation:", sprite, s => s.Rotation, labelSpan: 3)
                   .AddNumericInput("SpriteSkew", "Skew:", sprite, s => s.Skew, inputSpan: 1, labelSpan: 3)
                   .AddColorPicker("SpriteForeColor", "Foreground:", sprite, s => s.ForeColor, inputSpan: 1, labelSpan: 3)
                   .AddColorPicker("SpriteBackColor", "Background:", sprite, s => s.BackColor, inputSpan: 1, labelSpan: 3)
                   .Finalize();
                   ;
            var index = 0;
            _behaviors = sprite.Behaviors.ToDictionary(b =>
            {
                index++;
                return index + "." + b.Name;
            });
            _behaviorList.ClearItems();
            foreach (var item in _behaviors)
                _behaviorList.AddItem(item.Key, item.Value.Name);
            

            wrapContainer
                .AddItem(containerIcons)
                .AddHLine(_factory, "SpriteSplitterIconHLine", _lastWidh - 10, 5)
                .AddItem(container)
                .AddHLine(_factory, "SpriteSplitterIconHLine", _lastWidh - 10, 5)
                .AddItem(_behaviorPanel)
                ;
        }

        private void CreateBehaviorPanel()
        {
            _behaviorPanel = _factory.CreateWrapPanel(LingoOrientation.Vertical, "InspectorBehaviors");

            _behaviorList = _factory.CreateItemList("BehaviorList", x =>
            {
                if (x != null && _behaviors.TryGetValue(x, out var behavior))
                    BehaviorSelected?.Invoke(behavior);
            });
            _behaviorList.Height = 45;
            _behaviorList.Width = _lastWidh - 15;
            _behaviorList.Margin = new LingoMargin(5,0,0,0);
            _behaviorPanel.AddItem(_behaviorList);

        }


        public LingoGfxWindow? BuildBehaviorPopup(LingoSpriteBehavior behavior)
            => _descriptionManager.BuildBehaviorPopup(behavior,() =>
            {
                _behaviorList.SelectedIndex = -1;
            });

        private void AddMemberTab(ILingoMember member)
        {
            var wrapContainer = AddTab(PropetyTabNames.Member);
            var container = _factory.CreatePanel("MemberDetailPanel");
            wrapContainer
                .AddItem(container)
                ;

            container.Compose(_factory)
                   .Columns(4)
                   .AddTextInput("MemberName", "Name:", member, s => s.Name, inputSpan: 3)
                   .Columns(4)
                   .AddLabel("MemberSize","Size: ",2)
                   .AddLabel("MemberSizeV", CommonExtensions.BytesToShortString(member.Size),2)
                   .AddLabel("MemberCreationDate","Created: ",2)
                   .AddLabel("MemberCreationDateV",member.CreationDate.ToString("dd/MM/yyyy HH:mm"),2)
                   .AddLabel("MemberModifyDate","Modified: ",2)
                   .AddLabel("MemberModifyDateV",member.ModifiedDate.ToString("dd/MM/yyyy HH:mm"),2)
                   .Columns(4)
                   .AddTextInput("MemberFileName", "FileName:", member, s => s.FileName, inputSpan: 3)
                   .Columns(4)
                   .AddTextInput("MemberComments", "Comments:", member, s => s.Comments, inputSpan: 3)
                   .Finalize()
                   ;
        } 
        private void AddBitmapTab(LingoMemberBitmap member)
        {
            var wrapContainer = AddTab(PropetyTabNames.Bitmap);
            var container = _factory.CreatePanel("MemberDetailPanel");
            wrapContainer
                .AddItem(container)
                ;

            container.Compose(_factory)
                   .Columns(4)
                   .AddLabel("BitmapSize","Dimensions: ",2)
                   .AddLabel("BitmapSizeV", member.Width + " x " + member.Height,2) 
                   .AddCheckBox("BitmapHighLight", "Hightlight: ",member,x => x.Hilite,2,true,2)
                   //.AddLabel("BitmapBitDepth", "BitDepth: ", 2)
                   //.AddLabel("BitmapBitDepthV", member.ColorDepth,2)
                   .Columns(8)
                   .AddNumericInput("BitmapRegPointX", "RegPoint X:", member, s => s.RegPoint.X, inputSpan: 1, labelSpan: 3)
                   .AddNumericInput("BitmapRegPointY", "Y:", member, s => s.RegPoint.Y, inputSpan: 4, labelSpan: 1)
                   .Finalize()
                   ;
        }

        private void AddSoundTab(LingoMemberSound member)
        {
            var soundChannel = _player.Sound.Channel(1);
            if (soundChannel == null) return;
            var wrap = AddTab(PropetyTabNames.Sound);
            var btnPanel = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "SoundButtons");
            var playBtn = _factory.CreateButton("SoundPlay", "Play");
            var stopBtn = _factory.CreateButton("SoundStop", "Stop");
            playBtn.Pressed += () => soundChannel.Play(member);
            stopBtn.Pressed += () => soundChannel.Stop();
            btnPanel.AddItem(playBtn);
            btnPanel.AddItem(stopBtn);
            var panel = _factory.CreatePanel("SoundPanel");
            wrap.AddItem(btnPanel);
            wrap.AddItem(panel);

            string duration = TimeSpan.FromSeconds(member.Length).ToString(@"hh\:mm\:ss\.fff");
            panel.Compose(_factory)
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

        private void AddMovieTab(ILingoMovie? movie)
        {
            var wrap = AddTab(PropetyTabNames.Movie);

            var rowSize = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "MovieStageSizeRow");
            rowSize.Margin = new LingoMargin(5, 5, 5, 0);
            rowSize.Compose(_factory)
                .AddLabel("Stage size:")
                .AddNumericInput("MovieStageWidth", _player.Stage, m => m.Width, 40)
                .AddLabel("x")
                .AddNumericInput("MovieStageHeight", _player.Stage, m => m.Height, 40)
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
                var rowChannels = _factory.CreatePanel("MovieChannelsRow");
                rowChannels.Compose(_factory)
                    .Columns(4)
                    .AddNumericInput("MovieChannels", "Channels:", movie, m => m.MaxSpriteChannelCount)
                    .AddColorPicker("StageBgColor", "Color", _player.Stage, m => m.BackgroundColor)
                    .Columns(2)
                    .AddButton("MovieApplyBtn", "Apply", () =>
                    {
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

        private void AddCastTab(ILingoCast cast)
        {
            var wrap = AddTab(PropetyTabNames.Cast);
            var rowChannels = _factory.CreatePanel("CastRow");
            rowChannels.Margin = new LingoMargin(5, 5, 0, 0);
            rowChannels.Compose(_factory)
                   .Columns(8)
                   .NextRow()
                   .AddNumericInput("CastNumber", "Number:", cast, m => m.Number, 1, true, false, 2, c => c.Enabled = false)
                   .AddTextInput("CastName", "Name:", cast, m => m.Name, 3, 2)
                   .Finalize();
            ;
            wrap.AddItem(rowChannels);
        }
        private void AddTextTab(ILingoMemberTextBase textMember)
        {
            var wrap = AddTab(PropetyTabNames.Text);
            var rowChannels = _factory.CreatePanel("TextRow");
            rowChannels.Margin = new LingoMargin(5, 5, 0, 0);
            rowChannels.Compose(_factory)
                   .NextRow()
                   .Columns(8)
                   .AddNumericInput("TextWidth", "W:", textMember, s => s.Width)
                   .AddNumericInput("TextHeight", "H:", textMember, s => s.Height, inputSpan: 5)

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
         
        private void AddShapeTab(LingoMemberShape member)
        {
            var wrap = AddTab(PropetyTabNames.Shape);
            var rowChannels = _factory.CreatePanel("ShapeRow");
            rowChannels.Margin = new LingoMargin(5, 5, 0, 0);
            var composer = rowChannels.Compose(_factory)
                   .NextRow()
                   .Columns(8)
                   .AddEnumInput<LingoMemberShape, LingoShapeType>("ShapeType", "Shape:", member, s => s.ShapeTypeInt, inputSpan: 6, labelSpan: 2)
                   .AddCheckBox("ShapeClosed", "Filled:", member, s => s.Filled, inputSpan: 1, true)
                   
                   .NextRow()
                   .AddNumericInput("ShapeWidth", "W:", member, s => s.Width)
                   .AddNumericInput("ShapeHeight", "H:", member, s => s.Height, inputSpan: 5)
                   ;
            if (member.ShapeType == LingoShapeType.Rectangle || member.ShapeType == LingoShapeType.Oval)
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

        private void AddGuidesTab(DirectorStageGuides guides)
        {
            var wrap = AddTab(PropetyTabNames.Guides);
            var guidesPanel = _factory.CreatePanel("GuidesPanel");
            guidesPanel.Margin = new LingoMargin(5, 5, 0, 0);
            guidesPanel.Compose(_factory)
                   .Columns(4)
                   .AddColorPicker("GuideColor", "Color:", guides, g => g.GuidesColor)
                   .AddCheckBox("GuideVisible", "Visible:", guides, g => g.GuidesVisible)
                   .AddCheckBox("GuideSnap", "SnapTo:", guides, g => g.GuidesSnap)
                   .AddCheckBox("GuideLock", "Lock:", guides, g => g.GuidesLocked)
                   .Finalize();
            wrap.AddItem(guidesPanel);

            var gridPanel = _factory.CreatePanel("GridPanel");
            gridPanel.Margin = new LingoMargin(5, 5, 0, 0);
            gridPanel.Compose(_factory)
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
                   .AddNumericInput("GridWidth", "W:", guides, g => g.GridWidth)
                   .AddNumericInput("GridHeight", "H:", guides, g => g.GridHeight)
                   .Finalize();
            wrap.AddItem(gridPanel);
        }


        private LingoGfxWrapPanel AddTab(PropetyTabNames tabName)
        {
            var name = tabName.ToString();
            var scroller = _factory.CreateScrollContainer(name + "Scroll");
            LingoGfxWrapPanel container = _factory.CreateWrapPanel(LingoOrientation.Vertical, name + "Container");
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
            LingoGfxWrapPanel container = _factory.CreateWrapPanel(LingoOrientation.Vertical, name + "Container");

            if (_commandManager != null && (obj is LingoMemberBitmap || obj is ILingoMemberTextBase))
            {
                var editBtn = _factory.CreateButton("EditButton", "Edit");
                editBtn.Pressed += () =>
                {
                    string code = obj switch
                    {
                        LingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                        ILingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                        _ => string.Empty
                    };
                    if (!string.IsNullOrEmpty(code))
                        _commandManager.Handle(new OpenWindowCommand(code));
                };
                container.AddItem(editBtn);
            }

            // TODO: behavior list
            //if (obj as LingoSprite sprite)
            //    ShowBehavior(sprite)

            scroller.AddItem(container);
            var tabItem = _factory.CreateTabItem(name, name);
            tabItem.Content = scroller;
            _tabs.AddTab(tabItem);
        }



      


        public override void OpenWindow()
        {
            base.OpenWindow();
        }

  


       
    }
}
