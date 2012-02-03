using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Contains methods to manipulate World of Warcraft
    /// specific vector and quaternion packing.
    /// </summary>
    public static class VectorPackingExtensions
    {
        const int PACK_COEFF_YZ = 1 << 20;
        const int PACK_COEFF_X = 1 << 21;

        /// <summary>
        /// Packs the current <see cref="Kamilla.Quaternion"/>
        /// into a <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="quat">
        /// A <see cref="Kamilla.Quaternion"/> to pack
        /// into a <see cref="System.UInt64"/>.
        /// </param>
        /// <returns>
        /// A <see cref="System.UInt64"/> that represents
        /// the packed <see cref="Kamilla.Quaternion"/>.
        /// </returns>
        public static ulong Pack(this Quaternion quat)
        {
            int w_sign = quat.W >= 0 ? 1 : -1;

            long x = ((int)(quat.X * (double)PACK_COEFF_X)) * w_sign & ((1 << 22) - 1);
            long y = ((int)(quat.Y * (double)PACK_COEFF_YZ)) * w_sign & ((1 << 21) - 1);
            long z = ((int)(quat.Z * (double)PACK_COEFF_YZ)) * w_sign & ((1 << 21) - 1);

            return (ulong)(z | (y << 21) | (x << 42));
        }

        /// <summary>
        /// Unpacks a <see cref="System.UInt64"/> into
        /// a <see cref="Kamilla.Quaternion"/>.
        /// </summary>
        /// <param name="value">
        /// A <see cref="System.UInt64"/> to unpack into
        /// a <see cref="Kamilla.Quaternion"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Kamilla.Quaternion"/>
        /// that represents the unpacked <see cref="System.UInt64"/>.
        /// </returns>
        public static Quaternion UnpackQuaternion(this ulong value)
        {
            var x = (float)(value >> 42) / (float)PACK_COEFF_X;
            var y = (float)(value << 22 >> 43) / (float)PACK_COEFF_YZ;
            var z = (float)(value << 43 >> 43) / (float)PACK_COEFF_YZ;

            var w = 1.0f - (x * x + y * y + z * z);
            w = Math.Sign(w) * (float)Math.Sqrt(Math.Abs(w));

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Unpacks a <see cref="System.UInt32"/> that was packed
        /// against a <see cref="Kamilla.Vector3"/>
        /// into the original <see cref="Kamilla.Vector3"/>.
        /// </summary>
        /// <param name="value">
        /// The packed <see cref="System.UInt32"/>.
        /// </param>
        /// <param name="against">
        /// The <see cref="Kamilla.Vector3"/>
        /// that the value was packed against.
        /// </param>
        /// <returns>
        /// The unpacked <see cref="Kamilla.Vector3"/>.
        /// </returns>
        public static Vector3 UnpackAgainst(this uint value, ref Vector3 against)
        {
            // 0x7FF = (1 << 11) - 1
            float dX = ((value >> 00) & 0x7FF) * 0.25f;
            float dY = ((value >> 11) & 0x7FF) * 0.25f;
            float dZ = ((value >> 22) & 0x3FF) * 0.25f;

            var x = against.X - dX;
            var y = against.Y - dY;
            var z = against.Z - dZ;
            return new Vector3(x, y, z);
        }
    }
}
