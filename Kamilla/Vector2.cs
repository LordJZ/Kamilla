using System;
using System.Globalization;

namespace Kamilla
{
    /// <summary>
    /// Represents a 2-dimensional vector.
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>, IFormattable
    {
        /// <summary>
        /// Represents a zero vector. This field is read-only.
        /// </summary>
        public static readonly Vector2 Zero = new Vector2();

        /// <summary>
        /// Represents a (1,1) vector. This field is read-only.
        /// </summary>
        public static readonly Vector2 One = new Vector2(1.0f, 1.0f);

        /// <summary>
        /// Represents a (-1,-1) vector. This field is read-only.
        /// </summary>
        public static readonly Vector2 MinusOne = new Vector2(-1.0f, -1.0f);

        /// <summary>
        /// Represents a (1,0) vector. This field is read-only.
        /// </summary>
        public static readonly Vector2 UnitX = new Vector2(1.0f, 0.0f);

        /// <summary>
        /// Represents a (0,1) vector. This field is read-only.
        /// </summary>
        public static readonly Vector2 UnitY = new Vector2(0.0f, 1.0f);

        /// <summary>
        /// The X component of the <see cref="Kamilla.Vector2"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y component of the <see cref="Kamilla.Vector2"/>.
        /// </summary>
        public float Y;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Vector2"/>
        /// with the specified vector components.
        /// </summary>
        /// <param name="x">
        /// The X component of the <see cref="Kamilla.Vector2"/>.
        /// </param>
        /// <param name="y">
        /// The Y component of the <see cref="Kamilla.Vector2"/>.
        /// </param>
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Calculates the length of the <see cref="Kamilla.Vector2"/>.
        /// </summary>
        /// <returns>
        /// Length of the <see cref="Kamilla.Vector2"/>.
        /// </returns>
        public float GetLength()
        {
            float sqrLength = this.X * this.X + this.Y * this.Y;

            return (float)Math.Sqrt((double)sqrLength);
        }

        /// <summary>
        /// Multiplies a <see cref="Kamilla.Vector2"/> and a scalar value.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Kamilla.Vector2"/> used in the multiplication.
        /// </param>
        /// <param name="coeff">
        /// The scalar value used in the multiplication.
        /// </param>
        /// <returns>
        /// The <see cref="Kamilla.Vector2"/> that is a multiplication of
        /// the original <see cref="Kamilla.Vector2"/> and the scalar value.
        /// </returns>
        public static Vector2 operator *(Vector2 vector, float coeff)
        {
            return new Vector2(vector.X * coeff, vector.Y * coeff);
        }

        /// <summary>
        /// Sums two instances of <see cref="Kamilla.Vector2"/> structure.
        /// </summary>
        /// <param name="vector1">
        /// First instance of <see cref="Kamilla.Vector2"/> structure.
        /// </param>
        /// <param name="vector2">
        /// Second instance of <see cref="Kamilla.Vector2"/> structure.
        /// </param>
        /// <returns>
        /// Sum of two instances of <see cref="Kamilla.Vector2"/> structure.
        /// </returns>
        public static Vector2 operator +(Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }

        public static Vector2 operator -(Vector2 vector1, Vector2 vector2)
        {
            return vector2 + (-vector2);
        }

        public static Vector2 operator -(Vector2 vector)
        {
            return new Vector2(-vector.X, -vector.Y);
        }

        public bool Equals(Vector2 other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public static bool operator ==(Vector2 vector1, Vector2 vector2)
        {
            return vector1.Equals(vector2);
        }

        public static bool operator !=(Vector2 vector1, Vector2 vector2)
        {
            return !vector1.Equals(vector2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
                return this.Equals((Vector2)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        public override string ToString()
        {
            return this.ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            return this.ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (format != null)
                throw new FormatException();

            return string.Format(provider, "{{X:{0} Y:{1}}}", this.X, this.Y);
        }
    }
}
