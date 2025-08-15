using System;
using System.Numerics;

namespace AbstUI.Primitives
{
    /// <summary>
    /// Represents a 2D affine transform used by the engine.
    /// Wraps <see cref="Matrix3x2"/> to provide a framework agnostic
    /// representation similar to Godot's <c>Transform2D</c>.
    /// </summary>
    public readonly struct ATransform2D
    {
        /// <summary>Underlying matrix.</summary>
        public Matrix3x2 Matrix { get; }

        /// <summary>Creates a transform from an existing matrix.</summary>
        public ATransform2D(Matrix3x2 matrix)
        {
            Matrix = matrix;
        }

        /// <summary>Identity transform.</summary>
        public static ATransform2D Identity { get; } = new(Matrix3x2.Identity);

        /// <summary>Returns a translation transform.</summary>
        public static ATransform2D CreateTranslation(float x, float y)
            => new(Matrix3x2.CreateTranslation(x, y));

        /// <summary>Returns a scaling transform.</summary>
        public static ATransform2D CreateScale(float x, float y)
            => new(Matrix3x2.CreateScale(x, y));

        /// <summary>Returns a rotation transform in degrees.</summary>
        public static ATransform2D CreateRotation(float degrees)
            => new(Matrix3x2.CreateRotation(degrees * (MathF.PI / 180f)));

        /// <summary>
        /// Returns a skew (shear) transform, where degrees specify the
        /// shear angles along the X and Y axes.
        /// </summary>
        public static ATransform2D CreateSkew(float degreesX, float degreesY = 0)
            => new(Matrix3x2.CreateSkew(
                MathF.Tan(degreesX * (MathF.PI / 180f)),
                MathF.Tan(degreesY * (MathF.PI / 180f))));

        /// <summary>Multiplies two transforms.</summary>
        public static ATransform2D operator *(ATransform2D a, ATransform2D b)
            => new(a.Matrix * b.Matrix);

        /// <summary>Applies the transform to a point.</summary>
        public APoint TransformPoint(APoint point)
        {
            var v = Vector2.Transform(new Vector2(point.X, point.Y), Matrix);
            return new APoint(v.X, v.Y);
        }

        /// <summary>Attempts to invert the transform.</summary>
        public bool Invert(out ATransform2D inverse)
        {
            if (Matrix3x2.Invert(Matrix, out var inv))
            {
                inverse = new ATransform2D(inv);
                return true;
            }
            inverse = default;
            return false;
        }

        /// <summary>Adds a translation to this transform.</summary>
        public ATransform2D Translated(float x, float y)
            => new(Matrix * Matrix3x2.CreateTranslation(x, y));

        /// <summary>Adds a scaling to this transform.</summary>
        public ATransform2D Scaled(float x, float y)
            => new(Matrix * Matrix3x2.CreateScale(x, y));

        /// <summary>Adds a rotation in degrees to this transform.</summary>
        public ATransform2D Rotated(float degrees)
            => new(Matrix * Matrix3x2.CreateRotation(degrees * (MathF.PI / 180f)));

        /// <summary>Adds a skew to this transform.</summary>
        public ATransform2D Skewed(float degreesX, float degreesY = 0)
            => new(Matrix * Matrix3x2.CreateSkew(
                MathF.Tan(degreesX * (MathF.PI / 180f)),
                MathF.Tan(degreesY * (MathF.PI / 180f))));
    }
}

