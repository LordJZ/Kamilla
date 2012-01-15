using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Defines variables required to handle a packet dump reading class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NetworkLogAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the file extension of the underlying dump reader format.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Gets or sets header chars which a packet dump of the selected format should contain.
        /// 
        /// When this property is set, the char array is converted into byte array, and HeaderBytes property is set.
        /// </summary>
        public char[] HeaderChars
        {
            get { throw new Exception(); }
            set { HeaderBytes = value.Select(c => (byte)c).ToArray(); }
        }

        /// <summary>
        /// Gets or sets header string which a packet dump of the selected format should contain.
        /// 
        /// When this property is set, the string is converted into byte array, and HeaderBytes property is set.
        /// </summary>
        public string HeaderString
        {
            get { throw new Exception(); }
            set { HeaderBytes = Encoding.UTF8.GetBytes(value); }
        }

        /// <summary>
        /// Gets or sets the header bytes which a packet dump of the selected format should contain.
        /// </summary>
        public byte[] HeaderBytes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Kamilla.Network.Logging.NetworkLogFlags"/> of the underlying packet dump reading class.
        /// </summary>
        public NetworkLogFlags Flags { get; set; }
    }
}
