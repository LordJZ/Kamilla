using System;
using System.Globalization;

namespace Kamilla
{
    /// <summary>
    /// Represents a 3-dimensional vector.
    /// </summary>
    public struct Vector3 : IEquatable<Vector3>, IFormattable
    {
        /// <summary>
        /// Represents a zero vector. This field is read-only.
        /// </summary>
        public static readonly Vector3 Zero = new Vector3();

        /// <summary>
        /// Represents a (1,1,1) vector. This field is read-only.
        /// </summary>
        public static readonly Vector3 One = new Vector3(1.0f, 1.0f, 1.0f);

        /// <summary>
        /// Represents a (-1,-1,-1) vector. This field is read-only.
        /// </summary>
        public static readonly Vector3 MinusOne = new Vector3(-1.0f, -1.0f, -1.0f);

        /// <summary>
        /// Represents a (1,0,0) vector. This field is read-only.
        /// </summary>
        public static readonly Vector3 UnitX = new Vector3(1.0f, 0.0f, 0.0f);

        /// <summary>
        /// Represents a (0,1,0) vector. This field is read-only.
        /// </summary>
        public static readonly Vector3 UnitY = new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        /// Represents a (0,0,1) vector. This field is read-only.
        /// </summary>
        public static readonly Vector3 UnitZ = new Vector3(0.0f, 0.0f, 1.0f);

        /// <summary>
        /// The X component of the <see cref="Kamilla.Vector3"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y component of the <see cref="Kamilla.Vector3"/>.
        /// </summary>
        public float Y;

        /// <summary>
        /// The Z component of the <see cref="Kamilla.Vector3"/>.
        /// </summary>
        public float Z;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Vector3"/>
        /// with the specified vector components.
        /// </summary>
        /// <param name="x">
        /// The X component of the <see cref="Kamilla.Vector3"/>.
        /// </param>
        /// <param name="y">
        /// The Y component of the <see cref="Kamilla.Vector3"/>.
        /// </param>
        /// <param name="z">
        /// The Z component of the <see cref="Kamilla.Vector3"/>.
        /// </param>
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Calculates the length of the <see cref="Kamilla.Vector3"/>.
        /// </summary>
        /// <returns>
        /// Length of the <see cref="Kamilla.Vector3"/>.
        /// </returns>
        public float GetLength()
        {
            float sqrLength = this.X * this.X + this.Y * this.Y + this.Z * this.Z;

            return (float)Math.Sqrt((double)sqrLength);
        }

        /// <summary>
        /// Multiplies a <see cref="Kamilla.Vector3"/> and a scalar value.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Kamilla.Vector3"/> used in the multiplication.
        /// </param>
        /// <param name="coeff">
        /// The scalar value used in the multiplication.
        /// </param>
        /// <returns>
        /// The <see cref="Kamilla.Vector3"/> that is a multiplication of
        /// the original <see cref="Kamilla.Vector3"/> and the scalar value.
        /// </returns>
        public static Vector3 operator *(Vector3 vector, float coeff)
        {
            return new Vector3(vector.X * coeff, vector.Y * coeff, vector.Z * coeff);
        }

        /// <summary>
        /// Sums two instances of <see cref="Kamilla.Vector3"/> structure.
        /// </summary>
        /// <param name="vector1">
        /// First instance of <see cref="Kamilla.Vector3"/> structure.
        /// </param>
        /// <param name="vector2">
        /// Second instance of <see cref="Kamilla.Vector3"/> structure.
        /// </param>
        /// <returns>
        /// Sum of two instances of <see cref="Kamilla.Vector3"/> structure.
        /// </returns>
        public static Vector3 operator +(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
        }

        public static Vector3 operator -(Vector3 vector1, Vector3 vector2)
        {
            return vector2 + (-vector2);
        }

        public static Vector3 operator -(Vector3 vector)
        {
            return new Vector3(-vector.X, -vector.Y, -vector.Z);
        }

        public bool Equals(Vector3 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public static bool operator ==(Vector3 vector1, Vector3 Vector3)
        {
            return vector1.Equals(Vector3);
        }

        public static bool operator !=(Vector3 vector1, Vector3 Vector3)
        {
            return !vector1.Equals(Vector3);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3)
                return this.Equals((Vector3)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Y.GetHashCode();
        }

        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            return this.ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (format != null)
                throw new FormatException();

            return string.Format(provider, "{{X:{0} Y:{1} Z:{2}}}", this.X, this.Y, this.Z);
        }
    }
}
