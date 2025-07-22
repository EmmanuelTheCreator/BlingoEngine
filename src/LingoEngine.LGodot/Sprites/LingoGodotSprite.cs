﻿using Godot;
using LingoEngine.Casts;
using LingoEngine.LGodot.Texts;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Texts.FrameworkCommunication;
using LingoEngine.Shapes;
using LingoEngine.LGodot.Shapes;
using LingoEngine.LGodot.Primitives;

namespace LingoEngine.LGodot.Sprites
{

    public partial class LingoGodotSprite : ILingoFrameworkSprite, IDisposable
    {
        private readonly CenterContainer _Container2D;
        private readonly Node2D _parentNode2D;
        private readonly Sprite2D _Sprite2D;
        private readonly Action<LingoGodotSprite> _showMethod;
        private readonly Action<LingoGodotSprite> _removeMethod;
        private readonly Action<LingoGodotSprite> _hideMethod;
        private readonly LingoSprite _lingoSprite;
        private bool _wasShown;

        private CanvasItemMaterial _material = new();
        private int _ink;

        internal LingoSprite LingoSprite => _lingoSprite;
        internal bool IsDirty { get; set; } = true;
        internal bool IsDirtyMember { get; set; } = true;
        private float _x;
        private float _y;
        public float X { get => _x; set { _x = value; IsDirty = true; } }
        public float Y { get => _y; 
            set { _y = value; IsDirty = true; } }
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
            var fullName = _lingoSprite.GetFullName();
            _Sprite2D.Name = fullName + ".sprite";
            _Container2D.Name = fullName;
        }

        public float Width { get; private set; }
        public float Height { get; private set; }
        private float _DesiredWidth;
        private float _DesiredHeight;
        public float SetDesiredWidth { get => _DesiredWidth; set { _DesiredWidth = value; IsDirty = true; } }
        public float SetDesiredHeight { get => _DesiredHeight; set { _DesiredHeight = value; IsDirty = true; } }

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
            _Sprite2D.ZIndex = _directToStage ? 100000 + _zIndex : _zIndex;
        }

        private void ApplyBlend()
        {
            var col = _Sprite2D.SelfModulate;
            _Sprite2D.SelfModulate = new Color(col.R, col.G, col.B, _directToStage ? 1f : _blend);
            IsDirty = true;
        }

        private void ApplyInk()
        {
            if (_lingoSprite.InkType == LingoInkType.Matte)
            {
                var inkNumber = (int)_lingoSprite.InkType;
                _Sprite2D.Material = InkShaderMaterial.Create(_lingoSprite.InkType, _lingoSprite.BackColor.ToGodotColor());
            }
            else
            {
                //CanvasItemMaterial.BlendModeEnum mode = _ink switch
                //{
                //    (int)LingoInkType.AddPin => CanvasItemMaterial.BlendModeEnum.Add,
                //    (int)LingoInkType.Add => CanvasItemMaterial.BlendModeEnum.Add,
                //    (int)LingoInkType.SubstractPin => CanvasItemMaterial.BlendModeEnum.Sub,
                //    (int)LingoInkType.Substract => CanvasItemMaterial.BlendModeEnum.Sub,
                //    (int)LingoInkType.Darken => CanvasItemMaterial.BlendModeEnum.Mul,
                //    (int)LingoInkType.Lighten => CanvasItemMaterial.BlendModeEnum.Add,
                //    _ => CanvasItemMaterial.BlendModeEnum.Mix,
                //};
                //_material.BlendMode = mode;
                //_Sprite2D.Material = _material;
            }
        }


#pragma warning disable CS8618
        public LingoGodotSprite(LingoSprite lingoSprite, Node2D parentNode, Action<LingoGodotSprite> showMethod, Action<LingoGodotSprite> hideMethod, Action<LingoGodotSprite> removeMethod)
#pragma warning restore CS8618
        {
            _parentNode2D = parentNode;
            _lingoSprite = lingoSprite;
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
                Width = 0;
                Height = 0;
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


            switch (_lingoSprite.Member)
            {
                case LingoMemberBitmap pictureMember:
                    RemoveLastChildElement();
                    UpdateMemberPicture(pictureMember.Framework<LingoGodotMemberBitmap>());
                    UpdateSizeFromTexture();
                    if (_DesiredWidth == 0) _DesiredWidth = Width;
                    if (_DesiredHeight == 0) _DesiredHeight = Height;
                    IsDirty = true;
                    return;
                case LingoMemberText textMember:
                    var godotElement = textMember.Framework<LingoGodotMemberText>();
                    UpdateNodeMember(textMember, textMember.Framework<LingoGodotMemberText>().CloneForSpriteDraw());
                    break;
                case LingoMemberField fieldMember:
                    var godotElementF = fieldMember.Framework<LingoGodotMemberField>();
                    UpdateNodeMember(fieldMember, fieldMember.Framework<LingoGodotMemberField>().CloneForSpriteDraw());
                    break;
                // all generice godot node base class members
                case LingoMemberShape shape:
                    UpdateNodeMember(_lingoSprite.Member, (Node)shape.Framework<LingoGodotMemberShape>().CloneForSpriteDraw());
                    break;
            }
            UpdateSprite2DName();
        }

        private void RemoveLastChildElement()
        {
            if (_previousChildElementNode != null) _Sprite2D.RemoveChild(_previousChildElementNode);
            _previousChildElementNode = null;
            _previousChildElement = null;
        }

        private void UpdateMemberPicture(LingoGodotMemberBitmap godotPicture)
        {
            godotPicture.Preload();

            // Set the texture using the ImageTexture from the picture member
            if (godotPicture.TextureGodot == null)
                return;
            _Sprite2D.Texture = godotPicture.TextureGodot;
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
            if (_lingoSprite.Member == null) return new LingoPoint();
            if (_lingoSprite.Member is LingoMemberBitmap member)
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
            return _lingoSprite.Member.RegPoint;
        }
    }
}
