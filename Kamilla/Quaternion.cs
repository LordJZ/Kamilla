using System;
using System.Globalization;

namespace Kamilla
{
    public struct Quaternion : IFormattable
    {
        /// <summary>
        /// The X component of the <see cref="Kamilla.Quaternion"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y component of the <see cref="Kamilla.Quaternion"/>.
        /// </summary>
        public float Y;

        /// <summary>
        /// The Z component of the <see cref="Kamilla.Quaternion"/>.
        /// </summary>
        public float Z;

        /// <summary>
        /// The W component of the <see cref="Kamilla.Quaternion"/>.
        /// </summary>
        public float W;

        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
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

            return string.Format(provider, "{{X:{0} Y:{1} Z:{2} W:{3}}}", this.X, this.Y, this.Z, this.W);
        }
    }
}
