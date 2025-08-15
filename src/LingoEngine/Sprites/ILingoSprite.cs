﻿using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Sprites
{
    /// <summary>
    /// Represents a base sprite in the score.
    /// Mirrors Lingo’s sprite object functionality.
    /// </summary>
    public interface ILingoSpriteBase
    {
        /// <summary>
        /// The frame number at which the sprite appears. Read/write.
        /// </summary>
        int BeginFrame { get; set; }
        /// <summary>
        /// The frame number at which the sprite stops displaying. Read/write.
        /// </summary>
        int EndFrame { get; set; }
        /// <summary>
        /// Returns or sets the name of the sprite.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The unique index number of the sprite in the score. Read-only.
        /// </summary>
        int SpriteNum { get; }

        bool Puppet { get; set; }

        bool Lock { get; set; }
    }

    /// <summary>
    /// Represents a sprite in the score with visual, timing, and behavioral properties.
    /// Mirrors Lingo’s sprite object functionality.
    /// </summary>
    public interface ILingoSprite : ILingoSpriteBase
    {
       

        /// <summary>
        /// Background color for the sprite. Read/write.
        /// </summary>
        AColor BackColor { get; set; }

        /// <summary>
        /// Specifies the blend percentage (0–100) of the sprite’s visibility. Read/write.
        /// </summary>
        float Blend { get; set; }

        /// <summary>
        /// Reference to the cast associated with this sprite. Read-only.
        /// </summary>
        LingoCast? Cast { get; }

        /// <summary>
        /// Foreground color tint of the sprite. Read/write.
        /// </summary>
        AColor Color { get; set; }

        /// <summary>
        /// Whether the sprite is editable by the user (e.g., for text input). Read/write.
        /// </summary>
        bool Editable { get; set; }

       

        /// <summary>
        /// Foreground color of the sprite, often used in text. Read/write.
        /// </summary>
        AColor ForeColor { get; set; }

        /// <summary>
        /// Whether the sprite is highlighted. Read/write.
        /// </summary>
        bool Hilite { get; set; }

        /// <summary>
        /// The ink effect applied to the sprite. Read-only.
        /// </summary>
        int Ink { get; set; }
        LingoInkType InkType { get; set; }

        /// <summary>
        /// Returns TRUE if the sprite’s cast member is linked to an external file. Read-only.
        /// </summary>
        bool Linked { get; }

        /// <summary>
        /// Returns TRUE if the sprite's media is fully loaded. Read-only.
        /// </summary>
        bool Loaded { get; }



        /// <summary>
        /// Identifies the specified cast member as a media byte array. Read/write.
        /// Use for copying or swapping media content at runtime.
        /// </summary>
        byte[] Media { get; set; }

        /// <summary>
        /// Returns TRUE if the sprite’s media is initialized and ready. Read-only.
        /// </summary>
        bool MediaReady { get; }
        float Width { get; }
        float Height { get; }

        /// <summary>
        /// Gets the cast member associated with this sprite. 
        /// </summary>
        ILingoMember? Member { get; set; }

        /// <summary>
        /// Returns or sets the user or system who last modified the sprite.
        /// </summary>
        string ModifiedBy { get; set; }

       
        /// <summary>
        /// The rectangular boundary of the sprite (top-left to bottom-right). Read/write.
        /// </summary>
        ARect Rect { get; }

        /// <summary>
        /// Specifies the registration point of a cast member. Read/write.
        /// </summary>
        APoint RegPoint { get; set; }
        APoint Loc { get; set; }
        /// <summary>
        /// Horizontal location of the sprite on the stage. Read/write.
        /// </summary>
        float LocH { get; set; }

        /// <summary>
        /// Vertical location of the sprite on the stage. Read/write.
        /// </summary>
        float LocV { get; set; }
        int LocZ { get; set; }

        /// <summary>
        /// Rotation of the sprite in degrees.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Skew angle of the sprite in degrees.
        /// </summary>
        float Skew { get; set; }

        /// <summary>
        /// Flips the sprite horizontally.
        /// </summary>
        bool FlipH { get; set; }

        /// <summary>
        /// Flips the sprite vertically.
        /// </summary>
        bool FlipV { get; set; }

        /// <summary>
        /// Top edge position of the sprite.
        /// </summary>
        float Top { get; set; }

        /// <summary>
        /// Bottom edge position of the sprite.
        /// </summary>
        float Bottom { get; set; }

        /// <summary>
        /// Left edge position of the sprite.
        /// </summary>
        float Left { get; set; }

        /// <summary>
        /// Right edge position of the sprite.
        /// </summary>
        float Right { get; set; }

        /// <summary>
        /// Cursor ID used when the mouse is over the sprite.
        /// </summary>
        int Cursor { get; set; }

        /// <summary>
        /// Constraint channel ID for this sprite.
        /// </summary>
        int Constraint { get; set; }

        /// <summary>
        /// When TRUE the sprite is rendered directly on the stage, bypassing the
        /// normal score composition. Such sprites are always drawn above regular
        /// sprites and ignore visual effects.
        /// </summary>
        bool DirectToStage { get; set; }

        /// <summary>
        /// List of script instance names or types attached to the sprite.
        /// </summary>
        List<string> ScriptInstanceList { get; }

        /// <summary>
        /// Returns the size of the media in memory, in bytes. Read-only.
        /// </summary>
        int Size { get; }

        

        /// <summary>
        /// Returns or sets a small thumbnail representation of the sprite’s media.
        /// </summary>
        byte[] Thumbnail { get; set; }

        /// <summary>
        /// Controls whether the sprite is visible on the Stage. Read/write.
        /// </summary>
        bool Visibility { get; set; }
        int MemberNum { get; }
        


        /// <summary>
        /// Changes the cast member displayed by this sprite using the cast member number.
        /// </summary>
        /// <param name="memberNumber">The index of the cast member.</param>
        ILingoSprite SetMember(int memberNumber, int? castLibNum = null);

        /// <summary>
        /// Changes the cast member displayed by this sprite using the cast member name.
        /// </summary>
        /// <param name="memberName">The name of the cast member.</param>
        ILingoSprite SetMember(string memberName, int? castLibNum = null);
        ILingoSprite SetMember(ILingoMember? member);

        ILingoSprite AddBehavior<T>(Action<T>? configure = null) where T : LingoSpriteBehavior;

        /// <summary>
        /// Sends the sprite to the back of the display order (lowest layer).
        /// </summary>
        void SendToBack();

        /// <summary>
        /// Brings the sprite to the front of the display order (topmost layer).
        /// </summary>
        void BringToFront();

        /// <summary>
        /// Moves the sprite one layer backward in the display order.
        /// </summary>
        void MoveBackward();

        /// <summary>
        /// Moves the sprite one layer forward in the display order.
        /// </summary>
        void MoveForward();

        bool Intersects(ILingoSprite other);
        bool Within(ILingoSprite other);
        (APoint topLeft, APoint topRight, APoint bottomRight, APoint bottomLeft) Quad();

    }
}
