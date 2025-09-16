using Godot;
using LingoEngine.Casts;
using LingoEngine.LGodot.Texts;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Medias;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Tools;
using LingoEngine.Shapes;
using LingoEngine.LGodot.Shapes;
using LingoEngine.FilmLoops;
using LingoEngine.LGodot.FilmLoops;
using AbstUI.Primitives;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using LingoEngine.LGodot.Medias;

namespace LingoEngine.LGodot.Sprites
{

    public partial class LingoGodotSprite2D : ILingoFrameworkSprite, ILingoFrameworkSpriteVideo, IDisposable
    {
        private readonly CenterContainer _container2D;
        private readonly Node2D _parentNode2D;
        private readonly Sprite2D _sprite2D;
        private readonly Action<LingoGodotSprite2D> _showMethod;
        private readonly Action<LingoGodotSprite2D> _removeMethod;
        private readonly Action<LingoGodotSprite2D> _hideMethod;
        private readonly LingoSprite2D _lingoSprite2D;
        private bool _wasShown;
        private LingoFilmLoopPlayer? _filmloopPlayer;
        private VideoStreamPlayer? _videoPlayer;
        private CanvasItemMaterial _material = new();
        private int _ink;
        private AbstGodotTexture2D? _texture;
        internal LingoSprite2D LingoSprite => _lingoSprite2D;
        internal Node? ChildMemberNode => _previousChildElementNode;
        internal bool IsDirty { get; set; } = true;
        internal bool IsDirtyMember { get; set; } = true;
        private float _x;
        private float _y;
        private int _zIndex;
        private float _blend = 1f;
        private string _name = string.Empty;


        #region Properties

        public AMargin Margin { get; set; } = AMargin.Zero;

        public object FrameworkNode => _container2D;
        public float X
        {
            get => _x;
            set
            {
                if (_x == value) return;
                _x = value;
                if (_lingoSprite2D.SpriteNum == 101)
                {

                }
                SetDirty();
            }
        }
        public float Y
        {
            get => _y;
            set
            {
                if (_y == value) return;
                _y = value;
                SetDirty();
            }
        }

        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex == value) return;
                if (value > 4096) _zIndex = 4096;
                else if (value < -4096) _zIndex = -4096;
                else
                    _zIndex = value;
                ApplyZIndex();
            }
        }
        public APoint RegPoint
        {
            get => (_container2D.Position.X, _container2D.Position.Y);
            set
            {
                if (_x == value.X && _y == value.Y) return;
                _container2D.Position = new Vector2(value.X, value.Y);
                SetDirty();
            }
        }

        public bool Visibility
        {
            get => _container2D.Visible;
            set
            {
                if (_container2D.Visible == value) return;
                _container2D.Visible = value;
            }
        }
        public ILingoCast? Cast { get; private set; }


        public float Blend
        {
            get => _blend;
            set
            {
                if (_blend == value) return;
                _blend = value;
                ApplyBlend();
            }
        }

        public string Name
        {
            get => _name.ToString();
            set
            {
                _name = value;
                UpdateSprite2DName();
            }
        }

        private void UpdateSprite2DName()
        {
            var fullName = _lingoSprite2D.GetFullName();
            _sprite2D.Name = fullName + ".sprite";
            _container2D.Name = fullName;
        }

        public float Width { get; set; }
        public float Height { get; set; }
        private float _desiredWidth;
        private float _desiredHeight;
        public float DesiredWidth
        {
            get => _desiredWidth;
            set
            {
                if (_desiredWidth == value) return;
                _desiredWidth = value; SetDirty();
            }
        }
        public float DesiredHeight
        {
            get => _desiredHeight;
            set
            {
                if (_desiredHeight == value) return;
                _desiredHeight = value; SetDirty();
            }
        }

        public float Rotation
        {
            get => Mathf.RadToDeg(_sprite2D.Rotation);
            set
            {
                var val = Mathf.DegToRad(value);
                if (_sprite2D.Rotation == val) return;
                _sprite2D.Rotation = Mathf.DegToRad(value);
            }
        }
        public float Skew { get; set; }
        public bool FlipH { get => _sprite2D.FlipH; set => _sprite2D.FlipH = value; }
        public bool FlipV { get => _sprite2D.FlipV; set => _sprite2D.FlipV = value; }
        private bool _directToStage;
        public bool DirectToStage
        {
            get => _directToStage;
            set
            {
                _directToStage = value;
                ApplyZIndex();
                ApplyBlend();
            }
        }

        public int Ink
        {
            get => _ink;
            set
            {
                if (_ink == value) return;
                _ink = value;
                ApplyInk();
            }
        }

        #endregion


        public LingoGodotSprite2D(LingoSprite2D lingoSprite, Node2D parentNode, Action<LingoGodotSprite2D> showMethod, Action<LingoGodotSprite2D> hideMethod, Action<LingoGodotSprite2D> removeMethod)
        {
            _parentNode2D = parentNode;
            _lingoSprite2D = lingoSprite;
            _showMethod = showMethod;
            _hideMethod = hideMethod;
            _removeMethod = removeMethod;
            _sprite2D = new Sprite2D();
            _container2D = new CenterContainer();
            _container2D.AddChild(_sprite2D);
            lingoSprite.Init(this);
            _zIndex = lingoSprite.SpriteNum;
            _directToStage = lingoSprite.DirectToStage;
            ApplyZIndex();
            ApplyBlend();
            _ink = lingoSprite.Ink;
            ApplyInk();
        }



        private void ApplyZIndex()
        {
            _container2D.ZIndex = _directToStage ? 100000 + _zIndex : _zIndex;
        }

        private void ApplyBlend()
        {
            var col = _sprite2D.SelfModulate;
            float alpha = Mathf.Clamp(_blend / 100f, 0f, 1f);
            _sprite2D.SelfModulate = new Color(col.R, col.G, col.B, _directToStage ? 1f : alpha);
            SetDirty();
        }

        private void ApplyInk()
        {
            if (_ink == 0) return;

            if (InkPreRenderer.CanHandle((LingoInkType)_ink))
            {
                _material.BlendMode = CanvasItemMaterial.BlendModeEnum.Mix;
                _sprite2D.Material = _material;
                return;
            }

            CanvasItemMaterial.BlendModeEnum mode = _ink switch
            {
                (int)LingoInkType.AddPin => CanvasItemMaterial.BlendModeEnum.Add,
                (int)LingoInkType.Add => CanvasItemMaterial.BlendModeEnum.Add,
                (int)LingoInkType.SubstractPin => CanvasItemMaterial.BlendModeEnum.Sub,
                (int)LingoInkType.Substract => CanvasItemMaterial.BlendModeEnum.Sub,
                (int)LingoInkType.Darken => CanvasItemMaterial.BlendModeEnum.Mul,
                (int)LingoInkType.Lighten => CanvasItemMaterial.BlendModeEnum.Add,
                _ => CanvasItemMaterial.BlendModeEnum.Mix,
            };

            if (mode == CanvasItemMaterial.BlendModeEnum.Mix)
            {
                _sprite2D.Material = InkShaderMaterial.Create(
                    _lingoSprite2D.InkType,
                    _lingoSprite2D.BackColor.ToGodotColor());
            }
            else
            {
                _material.BlendMode = mode;
                _sprite2D.Material = _material;
            }
        }



        public void RemoveMe()
        {
            _removeMethod(this);
            Dispose();
        }
        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;
            var parent = _container2D.GetParent();
            if (parent != null)
                parent.RemoveChild(_container2D);
            _sprite2D.Dispose();
            _container2D.Dispose();
            _isDisposed = true;
        }

        public void Show()
        {
            if (_isDisposed) 
                return;
            if (!_wasShown)
            {
                _wasShown = true;
                _parentNode2D.AddChild(_container2D);
                _showMethod(this);
            }
            Update();
        }
        public void Hide()
        {
            if (_isDisposed) return;
            if (!_wasShown)
                return;
            _wasShown = false;
            _hideMethod(this);
            var parent = _container2D.GetParent();
            parent.RemoveChild(_container2D);
            if (parent is Node2D node2D)
                node2D.QueueRedraw();
        }

        public void SetPosition(APoint lingoPoint)
        {
            _x = lingoPoint.X;
            _y = lingoPoint.Y;
            SetDirty();
        }

        private void SetDirty()
        {
            IsDirty = true;
            _container2D.QueueRedraw();
        }

        public void MemberChanged()
        {
            _filmloopPlayer = null;
            if (LingoSprite.Member != null)
            {
                if (Width == 0 || Height == 0)
                {
                    Width = LingoSprite.Member.Width;
                    Height = LingoSprite.Member.Height;
                }
            }
            IsDirtyMember = true;
        }
        private void UpdateSizeFromTexture()
        {
            if (_texture == null)
            {
                // this breaks the filmloop
                //Width = 0;
                //Height = 0;
                return;
            }

            //if (Width == 0 || Height == 0)
            {
                Width = _texture.Width;
                Height = _texture.Height;
            }
        }
        internal void Update()
        {
            if (IsDirtyMember)
                UpdateMember();

            if (IsDirty)
            {
                // update complex properties
                if (_texture != null)
                {
                    if (_desiredWidth != Width || _desiredHeight != Height)
                    {
                        UpdateSizeFromTexture();
                        Width = _desiredWidth;
                        Height = _desiredHeight;
                        Resize(_desiredWidth, _desiredHeight);
                    }
                }
                IsDirty = false;
                //Console.WriteLine($"{_lingoSprite2D.SpriteNum}: {_lingoSprite2D.Member?.Name}: {_x}: {_y}");
            }
            // todo: move this 2 lines in IsDirty if test
            var offset = GetRegPointOffset();
            _sprite2D.Position = new Vector2(_x - offset.X, _y - offset.Y);
            
        }


        private void UpdateMember()
        {
            if (!IsDirtyMember) return;
            IsDirtyMember = false;


            switch (_lingoSprite2D.Member)
            {
                case LingoMemberBitmap pictureMember:
                    RemoveLastChildElement();
                    UpdateMemberPicture(pictureMember.Framework<LingoGodotMemberBitmap>());
                    //UpdateSizeFromTexture();
                    if (_desiredWidth == 0) _desiredWidth = Width;
                    if (_desiredHeight == 0) _desiredHeight = Height;
                    SetDirty();
                    return;
                case LingoFilmLoopMember flm:
                    RemoveLastChildElement();
                    UpdateMemberFilmLoop(flm.Framework<LingoGodotFilmLoopMember>());
                    //UpdateSizeFromTexture();
                    if (_desiredWidth == 0) _desiredWidth = Width;
                    if (_desiredHeight == 0) _desiredHeight = Height;
                    SetDirty();
                    return;
                case LingoMemberMedia mediaMember:
                    RemoveLastChildElement();
                    UpdateMemberVideo(mediaMember.Framework<LingoGodotMemberMedia>());
                    if (_desiredWidth == 0) _desiredWidth = Width;
                    if (_desiredHeight == 0) _desiredHeight = Height;
                    SetDirty();
                    return;
                case LingoMemberText textMember:
                    var godotElement = textMember.Framework<LingoGodotMemberText>();
                    UpdateNodeMember(textMember, godotElement.CreateForSpriteDraw());
                    //_lingoSprite2D.UpdateTexture(godotElement.RenderToTexture());
                    break;
                case LingoMemberField fieldMember:
                    var godotElementF = fieldMember.Framework<LingoGodotMemberField>();
                    UpdateNodeMember(fieldMember, godotElementF.CreateForSpriteDraw());
                    //_lingoSprite2D.UpdateTexture(godotElementF.RenderToTexture());
                    break;
                // all generice godot node base class members
                case LingoMemberShape shape:
                    UpdateNodeMember(_lingoSprite2D.Member, (Node)shape.Framework<LingoGodotMemberShape>().CloneForSpriteDraw());
                    break;
            }

            UpdateSprite2DName();
        }
        public void ApplyMemberChangesOnStepFrame()
        {
            //switch (_lingoSprite2D.Member)
            //{
            //    case LingoMemberBitmap pictureMember:
            //        return;
            //    case LingoMemberText textMember:
            //        break;
            //    case LingoMemberField fieldMember:
            //        //fieldMember.ApplyMemberChanges();
            //        break;
            //    // all generice godot node base class members
            //    case LingoMemberShape shape:
            //        break;
            //    case LingoFilmLoopMember filmloop:

            //        break;
            //}
        }
        private void RemoveLastChildElement()
        {
            if (_previousChildElementNode != null)
                _sprite2D.RemoveChild(_previousChildElementNode);
            _previousChildElementNode = null;
            _previousChildElement = null;
            _videoPlayer = null;
        }

        private void UpdateMemberPicture(LingoGodotMemberBitmap godotPicture)
        {
            godotPicture.Preload();
            if (godotPicture.TextureLingo == null)
                return;

            if (InkPreRenderer.CanHandle(_lingoSprite2D.InkType))
            {
                var texture1 = godotPicture.GetTextureForInk(_lingoSprite2D.InkType, _lingoSprite2D.BackColor) as AbstGodotTexture2D;
                if (texture1 != null)
                    TextureHasChanged(texture1);
            }
            else
                TextureHasChanged((AbstGodotTexture2D)godotPicture.TextureLingo);
        }
        private void TextureHasChanged(AbstGodotTexture2D tex)
        {
            if (tex.Texture == _sprite2D.Texture) return;
            if (tex.IsDisposed)
            {

            }
            _sprite2D.Texture = tex.Texture;
            _texture = tex;
            // because we dont need to clone the textures, we dont need to subscribe unsubscribe to  texture.
            _lingoSprite2D.FWTextureHasChanged(tex, false);
            if (Width == 0 || Height == 0)
            {
                Width = tex.Width;
                Height = tex.Height;
            }
        }
        public void SetTexture(IAbstTexture2D texture)
        {
            TextureHasChanged((AbstGodotTexture2D)texture);
        }
        private void UpdateMemberFilmLoop(LingoGodotFilmLoopMember filmLoop)
        {
            var size = filmLoop.GetBoundingBox();
            _desiredHeight = size.Height;
            _desiredWidth = size.Width;
            Width = size.Width;
            Height = size.Height;
            var offset = filmLoop.Offset;
            _sprite2D.Offset = new Vector2(Width / 2f - offset.X, Height / 2f - offset.Y);
            _filmloopPlayer = _lingoSprite2D.GetFilmLoopPlayer();
            if (_filmloopPlayer == null) return;
            if (_filmloopPlayer.Texture is not AbstGodotTexture2D tex)
                return;
            TextureHasChanged(tex);
        }
        private void UpdateMemberVideo(LingoGodotMemberMedia media)
        {
            media.Preload();
            var player = new VideoStreamPlayer
            {
                Stream = media.Stream
            };
            _videoPlayer = player;
            UpdateNodeMember(_lingoSprite2D.Member!, player);
        }
        private ILingoMember? _previousChildElement;
        private Node? _previousChildElementNode;


        private void UpdateNodeMember(ILingoMember member, Node godotElement) // Clone required to be able to draw multiple times the same member
        {
            if (_previousChildElement == member) return;
            RemoveLastChildElement();

            member.Preload();

            _sprite2D.AddChild(godotElement);
            _previousChildElementNode = godotElement;
            _previousChildElement = member;
            _desiredWidth = member.Width;
            _desiredHeight = member.Height;
        }

        public void Resize(float targetWidth, float targetHeight)
        {
            var width = _sprite2D.Texture.GetWidth();
            var height = _sprite2D.Texture.GetHeight();
            float scaleFactorW = targetWidth / width;
            float scaleFactorH = targetHeight / _sprite2D.Texture.GetHeight();
            _sprite2D.Scale = new Vector2(scaleFactorW, scaleFactorH);
        }

        private APoint GetRegPointOffset()
        {
            if (_lingoSprite2D.Member == null) return new APoint();
            if (_lingoSprite2D.Member is LingoMemberBitmap member)
            {
                var baseOffset = member.CenterOffsetFromRegPoint();
                if (member.Width != 0 && member.Height != 0)
                {
                    float scaleX = _sprite2D.Scale.X;
                    float scaleY = _sprite2D.Scale.Y;
                    return new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return _lingoSprite2D.Member.RegPoint;
        }


        #region Video/Media

        /// <inheritdoc/>
        public void Play()
        {
            if (_videoPlayer == null) return;
            if (_videoPlayer.Paused)
                _videoPlayer.Paused = false;
            else
                _videoPlayer?.Play();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (_videoPlayer != null)
                _videoPlayer.Paused = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_videoPlayer == null) return;
            _videoPlayer.Stop();
            _videoPlayer.StreamPosition = 0;
        }

        /// <inheritdoc/>
        public void Seek(int milliseconds)
        {
            if (_videoPlayer == null) return;
            _videoPlayer.StreamPosition = milliseconds / 1000f;
        }

        /// <inheritdoc/>
        public int Duration
        {
            get
            {
                if (_videoPlayer == null) return 0;
                return (int)(_videoPlayer.GetStreamLength() * 1000);
            }
        }

        /// <inheritdoc/>
        public int CurrentTime
        {
            get => (int)(_videoPlayer?.StreamPosition * 1000 ?? 0);
            set
            {
                if (_videoPlayer != null)
                    _videoPlayer.StreamPosition = value / 1000f;
            }
        }

        /// <inheritdoc/>
        public LingoMediaStatus MediaStatus => _videoPlayer switch
        {
            null => LingoMediaStatus.Closed,
            _ when _videoPlayer.IsPlaying() => LingoMediaStatus.Playing,
            _ => LingoMediaStatus.Paused
        };

        
        #endregion
    }
}
