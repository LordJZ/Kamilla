using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla
{
    public struct Quaternion
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
    }
}
