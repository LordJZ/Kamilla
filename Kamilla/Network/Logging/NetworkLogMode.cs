using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Specifies mode of a <see cref="Kamilla.Network.Logging.NetworkLog"/>.
    /// </summary>
    public enum NetworkLogMode
    {
        /// <summary>
        /// Specifies abstract mode of a <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <remarks>
        /// In abstract mode a <see cref="Kamilla.Network.Logging.NetworkLog"/> stores
        /// all passed packets, and allows one-pass data saving into the file.
        /// </remarks>
        Abstract = -1,

        /// <summary>
        /// Specifies reading mode of a <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <remarks>
        /// In reading mode a <see cref="Kamilla.Network.Logging.NetworkLog"/> loads
        /// meta data and packets from a file, and doesn't allow further data modification.
        /// </remarks>
        Reading = 0,

        /// <summary>
        /// Specifies writing mode of a <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <remarks>
        /// In writing mode a <see cref="Kamilla.Network.Logging.NetworkLog"/> saves
        /// all passed packets into a file, and doesn't allow accessing stored data.
        /// </remarks>
        Writing = 1,
    }
}
