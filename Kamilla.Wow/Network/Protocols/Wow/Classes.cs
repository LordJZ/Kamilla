
namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Represents playable classes of World of Warcraft.
    /// 
    /// When using localized names of this enum, pass gender as the first argument.
    /// </summary>
    [LocalizedNameContainer(typeof(WowStrings))]
    public enum Classes
    {
        /// <summary>
        /// Represents the Warrior class.
        /// </summary>
        [LocalizedName("Class1")]
        Warrior     = 1,

        /// <summary>
        /// Represents the Paladin class.
        /// </summary>
        [LocalizedName("Class2")]
        Paladin     = 2,

        /// <summary>
        /// Represents the Hunter class.
        /// </summary>
        [LocalizedName("Class3")]
        Hunter      = 3,

        /// <summary>
        /// Represents the Rogue class.
        /// </summary>
        [LocalizedName("Class4")]
        Rogue       = 4,

        /// <summary>
        /// Represents the Priest class.
        /// </summary>
        [LocalizedName("Class5")]
        Priest      = 5,

        /// <summary>
        /// Represents the Death Knight class.
        /// </summary>
        [LocalizedName("Class6")]
        DeathKnight = 6,

        /// <summary>
        /// Represents the Shaman class.
        /// </summary>
        [LocalizedName("Class7")]
        Shaman      = 7,

        /// <summary>
        /// Represents the Mage class.
        /// </summary>
        [LocalizedName("Class8")]
        Mage        = 8,

        /// <summary>
        /// Represents the Warlock class.
        /// </summary>
        [LocalizedName("Class9")]
        Warlock     = 9,

        /// <summary>
        /// Represents the Druid class.
        /// </summary>
        [LocalizedName("Class11")]
        Druid       = 11,
    }
}
