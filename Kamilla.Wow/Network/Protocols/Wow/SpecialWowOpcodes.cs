
namespace Kamilla.Network.Protocols.Wow
{
    public class SpecialWowOpcodes : SpecialOpcodes
    {
        public const uint HelloOpcode = uint.MaxValue - 2;
        public const uint SetSubZone = uint.MaxValue - 3;
        public const uint SetZone = uint.MaxValue - 4;
    }
}
