using System;

namespace Kamilla.Network
{
    /// <summary>
    /// Contains extensions methods to the
    /// <see cref="Kamilla.Network.TransferDirection"/> enum.
    /// </summary>
    public static class TransferDirectionExtensions
    {
        /// <summary>
        /// Returns the opposite <see cref="Kamilla.Network.TransferDirection"/>
        /// to the specified <see cref="Kamilla.Network.TransferDirection"/>.
        /// </summary>
        /// <param name="direction">
        /// The <see cref="Kamilla.Network.TransferDirection"/> which opposite
        /// <see cref="Kamilla.Network.TransferDirection"/> should be returned.
        /// </param>
        /// <returns>
        /// The <see cref="Kamilla.Network.TransferDirection"/> opposite
        /// to the provided <see cref="Kamilla.Network.TransferDirection"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// direction value is invalid.
        /// </exception>
        public static TransferDirection Opposite(this TransferDirection direction)
        {
            if (direction == TransferDirection.ToClient)
                return TransferDirection.ToServer;

            if (direction == TransferDirection.ToServer)
                return TransferDirection.ToClient;

            throw new ArgumentException();
        }
    }
}
