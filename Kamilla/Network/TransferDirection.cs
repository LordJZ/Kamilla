
namespace Kamilla.Network
{
    /// <summary>
    /// Defines data transfer directions.
    /// </summary>
    public enum TransferDirection : byte
    {
        /// <summary>
        /// Represents Client-to-Server data transfer direction.
        /// </summary>
        ToServer,

        /// <summary>
        /// Represents Server-to-Client data transfer direction.
        /// </summary>
        ToClient
    }
}
