using Godot;
using LingoEngine.Casts;
using LingoEngine.LGodot.Texts;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Tools;
using LingoEngine.Shapes;
using LingoEngine.LGodot.Shapes;
using LingoEngine.LGodot.Primitives;
using LingoEngine.FilmLoops;
using LingoEngine.LGodot.FilmLoops;

namespace LingoEngine.LGodot.Sprites
{

    public partial class LingoGodotSprite2D : ILingoFrameworkSprite, IDisposable
    {
        private readonly CenterContainer _Container2D;
        private readonly Node2D _parentNode2D;
        private readonly Sprite2D _Sprite2D;
        private readonly Action<LingoGodotSprite2D> _showMethod;
        private readonly Action<LingoGodotSprite2D> _removeMethod;
        private readonly Action<LingoGodotSprite2D> _hideMethod;
        private readonly LingoSprite2D _lingoSprite2D;
        private bool _wasShown;
        private LingoFilmLoopPlayer? _filmloopPlayer;
        private CanvasItemMaterial _material = new();
        private int _ink;

        internal LingoSprite2D LingoSprite => _lingoSprite2D;
        internal Node? ChildMemberNode => _previousChildElementNode;
        internal bool IsDirty { get; set; } = true;
        internal bool IsDirtyMember { get; set; } = true;
        private float _x;
        private float _y;
        public float X { get => _x; set { _x = value; IsDirty = true; } }
        public float Y
        {
            get => _y;
            set { _y = value; IsDirty = true; }
        }
        private int _zIndex;
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                _zIndex = value;
                ApplyZIndex();
            }
        }
        public LingoPoint RegPoint { get => (_Container2D.Position.X, _Container2D.Position.Y); set { _Container2D.Position = new Vector2(value.X, value.Y); IsDirty = true; } }

        public bool Visibility { get => _Container2D.Visible; set => _Container2D.Visible = value; }
        public ILingoCast? Cast { get; private set; }

        private float _blend = 1f;
        public float Blend
        {
            get => _blend;
            set
            {
                _blend = value;
                ApplyBlend();
            }
        }
        private string _name;
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
            _Sprite2D.Name = fullName + ".sprite";
            _Container2D.Name = fullName;
        }

        public float Width { get; private set; }
        public float Height { get; private set; }
        private float _DesiredWidth;
        private float _DesiredHeight;
        public float DesiredWidth { get => _DesiredWidth; set { _DesiredWidth = value; IsDirty = true; } }
        public float DesiredHeight { get => _DesiredHeight; set { _DesiredHeight = value; IsDirty = true; } }

        public float Rotation
        {
            get => Mathf.RadToDeg(_Sprite2D.Rotation);
            set => _Sprite2D.Rotation = Mathf.DegToRad(value);
        }
        public float Skew { get; set; }
        public bool FlipH { get => _Sprite2D.FlipH; set => _Sprite2D.FlipH = value; }
        public bool FlipV { get => _Sprite2D.FlipV; set => _Sprite2D.FlipV = value; }
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
                _ink = value;
                ApplyInk();
            }
        }

        private void ApplyZIndex()
        {
            _Container2D.ZIndex = _directToStage ? 100000 + _zIndex : _zIndex;
        }

        private void ApplyBlend()
        {
            var col = _Sprite2D.SelfModulate;
            float alpha = Mathf.Clamp(_blend / 100f, 0f, 1f);
            _Sprite2D.SelfModulate = new Color(col.R, col.G, col.B, _directToStage ? 1f : alpha);
            IsDirty = true;
        }

        private void ApplyInk()
        {
            if (_ink == 0) return;

            if (InkPreRenderer.CanHandle((LingoInkType)_ink))
            {
                _material.BlendMode = CanvasItemMaterial.BlendModeEnum.Mix;
                _Sprite2D.Material = _material;
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
                _Sprite2D.Material = InkShaderMaterial.Create(
                    _lingoSprite2D.InkType,
                    _lingoSprite2D.BackColor.ToGodotColor());
            }
            else
            {
                _material.BlendMode = mode;
                _Sprite2D.Material = _material;
            }
        }


#pragma warning disable CS8618
        public LingoGodotSprite2D(LingoSprite2D lingoSprite, Node2D parentNode, Action<LingoGodotSprite2D> showMethod, Action<LingoGodotSprite2D> hideMethod, Action<LingoGodotSprite2D> removeMethod)
#pragma warning restore CS8618
        {
            _parentNode2D = parentNode;
            _lingoSprite2D = lingoSprite;
            _showMethod = showMethod;
            _hideMethod = hideMethod;
            _removeMethod = removeMethod;
            _Sprite2D = new Sprite2D();
            _Container2D = new CenterContainer();
            _Container2D.AddChild(_Sprite2D);
            lingoSprite.Init(this);
            _zIndex = lingoSprite.SpriteNum;
            _directToStage = lingoSprite.DirectToStage;
            ApplyZIndex();
            ApplyBlend();
            _ink = lingoSprite.Ink;
            ApplyInk();
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
            var parent = _Container2D.GetParent();
            if (parent != null)
                parent.RemoveChild(_Container2D);
            _Sprite2D.Dispose();
            _Container2D.Dispose();
            _isDisposed = true;
        }

        public void Show()
        {
            if (!_wasShown)
            {
                _wasShown = true;
                _parentNode2D.AddChild(_Container2D);
                _showMethod(this);
            }
            Update();
        }
        public void Hide()
        {
            if (!_wasShown)
                return;
            _wasShown = false;
            _hideMethod(this);
            _Container2D.GetParent().RemoveChild(_Container2D);
        }

        public void SetPosition(LingoPoint lingoPoint)
        {
            _x = lingoPoint.X;
            _y = lingoPoint.Y;
            IsDirty = true;
        }

        public void MemberChanged()
        {
            _filmloopPlayer = null;
            if (LingoSprite.Member != null)
            {
                Width = LingoSprite.Member.Width;
                Height = LingoSprite.Member.Height;
            }
            IsDirtyMember = true;
        }
        private void UpdateSizeFromTexture()
        {
            if (_Sprite2D.Texture == null)
            {
                // this breaks the filmloop
                //Width = 0;
                //Height = 0;
                return;
            }
            Width = _Sprite2D.Texture.GetWidth();
            Height = _Sprite2D.Texture.GetHeight();
        }
        internal void Update()
        {
            if (IsDirtyMember)
                UpdateMember();

            if (IsDirty)
            {
                // update complex properties
                if (_Sprite2D.Texture != null)
                {
                    if (_DesiredWidth != Width || _DesiredHeight != Height)
                    {
                        UpdateSizeFromTexture();
                        Width = _DesiredWidth;
                        Height = _DesiredHeight;
                        Resize(_DesiredWidth, _DesiredHeight);
                    }
                }
                IsDirty = false;
            }
            // todo: move this 2 lines in IsDirty if test
            var offset = GetRegPointOffset();
            _Sprite2D.Position = new Vector2(_x - offset.X, _y - offset.Y);

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
                    UpdateSizeFromTexture();
                    if (_DesiredWidth == 0) _DesiredWidth = Width;
                    if (_DesiredHeight == 0) _DesiredHeight = Height;
                    IsDirty = true;
                    return;
                case LingoFilmLoopMember flm:
                    RemoveLastChildElement();
                    UpdateMemberFilmLoop(flm.Framework<LingoGodotFilmLoopMember>());
                    UpdateSizeFromTexture();
                    if (_DesiredWidth == 0) _DesiredWidth = Width;
                    if (_DesiredHeight == 0) _DesiredHeight = Height;
                    IsDirty = true;
                    return;
                case LingoMemberText textMember:
                    var godotElement = textMember.Framework<LingoGodotMemberText>();
                    UpdateNodeMember(textMember, textMember.Framework<LingoGodotMemberText>().CreateForSpriteDraw());
                    break;
                case LingoMemberField fieldMember:
                    var godotElementF = fieldMember.Framework<LingoGodotMemberField>();
                    UpdateNodeMember(fieldMember, fieldMember.Framework<LingoGodotMemberField>().CreateForSpriteDraw());
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
                _Sprite2D.RemoveChild(_previousChildElementNode);
            _previousChildElementNode = null;
            _previousChildElement = null;
        }

        private void UpdateMemberPicture(LingoGodotMemberBitmap godotPicture)
        {
            godotPicture.Preload();
            if (godotPicture.TextureGodot == null)
                return;

            if (InkPreRenderer.CanHandle(_lingoSprite2D.InkType))
                _Sprite2D.Texture = godotPicture.GetTextureForInk(_lingoSprite2D.InkType, _lingoSprite2D.BackColor);
            else
                _Sprite2D.Texture = godotPicture.TextureGodot;
        }
        private void UpdateMemberFilmLoop(LingoGodotFilmLoopMember filmLoop)
        {
            var size = filmLoop.GetBoundingBox();
            _DesiredHeight = size.Height;
            _DesiredWidth = size.Width;
            Width = size.Width;
            Height = size.Height;
            _filmloopPlayer = _lingoSprite2D.GetFilmLoopPlayer();
            if (_filmloopPlayer == null) return;
            if (_filmloopPlayer.Texture is not LingoGodotTexture2D tex)
                return;

            _Sprite2D.Texture = tex.Texture;
           
        }
        private ILingoMember? _previousChildElement;
        private Node? _previousChildElementNode;
        

        private void UpdateNodeMember(ILingoMember member, Node godotElement) // Clone required to be able to draw multiple times the same member
        {
            if (_previousChildElement == member) return;
            RemoveLastChildElement();

            member.Preload();

            _Sprite2D.AddChild(godotElement);
            _previousChildElementNode = godotElement;
            _previousChildElement = member;
            _DesiredWidth = member.Width;
            _DesiredHeight = member.Height;
        }

        public void Resize(float targetWidth, float targetHeight)
        {
            var width = _Sprite2D.Texture.GetWidth();
            var height = _Sprite2D.Texture.GetHeight();
            float scaleFactorW = targetWidth / width;
            float scaleFactorH = targetHeight / _Sprite2D.Texture.GetHeight();
            _Sprite2D.Scale = new Vector2(scaleFactorW, scaleFactorH);
        }

        private LingoPoint GetRegPointOffset()
        {
            if (_lingoSprite2D.Member == null) return new LingoPoint();
            if (_lingoSprite2D.Member is LingoMemberBitmap member)
            {
                var baseOffset = member.CenterOffsetFromRegPoint();
                if (member.Width != 0 && member.Height != 0)
                {
                    float scaleX = _Sprite2D.Scale.X;
                    float scaleY = _Sprite2D.Scale.Y;
                    return new LingoPoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return _lingoSprite2D.Member.RegPoint;
        }
    }
}
