
namespace Kamilla.Network.Protocols.Wow
{
    public enum WowPacketFlags : byte
    {
        None = 0,

        /// <summary>
        /// The long packet's header is fragmented and was not read
        /// fully when the tcp packet was originally received.
        /// 
        /// Prevents the first part of header from being decrypted
        /// when the packet goes back to the Relay to be processed.
        /// </summary>
        PartialHeader = 0x10,

        /// <summary>
        /// Indicates that the packet is interpreted
        /// as a connection hello packet.
        /// </summary>
        HelloPacket = 0x20,

        /// <summary>
        /// Indicates that the packet is a part of a compound packet.
        /// </summary>
        Compound = 0x40,
    }
}
