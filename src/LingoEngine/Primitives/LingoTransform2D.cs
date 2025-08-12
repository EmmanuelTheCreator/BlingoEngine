using System;
using System.Numerics;

namespace LingoEngine.Primitives
{
    /// <summary>
    /// Represents a 2D affine transform used by the engine.
    /// Wraps <see cref="Matrix3x2"/> to provide a framework agnostic
    /// representation similar to Godot's <c>Transform2D</c>.
    /// </summary>
    public readonly struct LingoTransform2D
    {
        /// <summary>Underlying matrix.</summary>
        public Matrix3x2 Matrix { get; }

        /// <summary>Creates a transform from an existing matrix.</summary>
        public LingoTransform2D(Matrix3x2 matrix)
        {
            Matrix = matrix;
        }

        /// <summary>Identity transform.</summary>
        public static LingoTransform2D Identity { get; } = new(Matrix3x2.Identity);

        /// <summary>Returns a translation transform.</summary>
        public static LingoTransform2D CreateTranslation(float x, float y)
            => new(Matrix3x2.CreateTranslation(x, y));

        /// <summary>Returns a scaling transform.</summary>
        public static LingoTransform2D CreateScale(float x, float y)
            => new(Matrix3x2.CreateScale(x, y));

        /// <summary>Returns a rotation transform in degrees.</summary>
        public static LingoTransform2D CreateRotation(float degrees)
            => new(Matrix3x2.CreateRotation(degrees * (MathF.PI / 180f)));

        /// <summary>
        /// Returns a skew (shear) transform, where degrees specify the
        /// shear angles along the X and Y axes.
        /// </summary>
        public static LingoTransform2D CreateSkew(float degreesX, float degreesY = 0)
            => new(Matrix3x2.CreateSkew(
                MathF.Tan(degreesX * (MathF.PI / 180f)),
                MathF.Tan(degreesY * (MathF.PI / 180f))));

        /// <summary>Multiplies two transforms.</summary>
        public static LingoTransform2D operator *(LingoTransform2D a, LingoTransform2D b)
            => new(a.Matrix * b.Matrix);

        /// <summary>Applies the transform to a point.</summary>
        public LingoPoint TransformPoint(LingoPoint point)
        {
            var v = Vector2.Transform(new Vector2(point.X, point.Y), Matrix);
            return new LingoPoint(v.X, v.Y);
        }

        /// <summary>Attempts to invert the transform.</summary>
        public bool Invert(out LingoTransform2D inverse)
        {
            if (Matrix3x2.Invert(Matrix, out var inv))
            {
                inverse = new LingoTransform2D(inv);
                return true;
            }
            inverse = default;
            return false;
        }

        /// <summary>Adds a translation to this transform.</summary>
        public LingoTransform2D Translated(float x, float y)
            => new(Matrix * Matrix3x2.CreateTranslation(x, y));

        /// <summary>Adds a scaling to this transform.</summary>
        public LingoTransform2D Scaled(float x, float y)
            => new(Matrix * Matrix3x2.CreateScale(x, y));

        /// <summary>Adds a rotation in degrees to this transform.</summary>
        public LingoTransform2D Rotated(float degrees)
            => new(Matrix * Matrix3x2.CreateRotation(degrees * (MathF.PI / 180f)));

        /// <summary>Adds a skew to this transform.</summary>
        public LingoTransform2D Skewed(float degreesX, float degreesY = 0)
            => new(Matrix * Matrix3x2.CreateSkew(
                MathF.Tan(degreesX * (MathF.PI / 180f)),
                MathF.Tan(degreesY * (MathF.PI / 180f))));
    }
}

