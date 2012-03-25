using System;

namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Represents bit mask of playable classes of World of Warcraft.
    /// 
    /// When using localized names of this enum, pass gender as the first argument.
    /// </summary>
    [Flags]
    [LocalizedNameContainer(typeof(WowStrings))]
    public enum ClassMask : uint
    {
        /// <summary>
        /// Represents the empty class mask.
        /// </summary>
        [LocalizedName("NoClasses")]
        None        = 0,

        /// <summary>
        /// Represents the Warrior class bit.
        /// </summary>
        [LocalizedName("Class1")]
        Warrior     = 1 << (Classes.Warrior - 1),

        /// <summary>
        /// Represents the Paladin class bit.
        /// </summary>
        [LocalizedName("Class2")]
        Paladin     = 1 << (Classes.Paladin - 1),

        /// <summary>
        /// Represents the Hunter class bit.
        /// </summary>
        [LocalizedName("Class3")]
        Hunter      = 1 << (Classes.Hunter - 1),

        /// <summary>
        /// Represents the Rogue class bit.
        /// </summary>
        [LocalizedName("Class4")]
        Rogue       = 1 << (Classes.Rogue - 1),

        /// <summary>
        /// Represents the Priest class bit.
        /// </summary>
        [LocalizedName("Class5")]
        Priest      = 1 << (Classes.Priest - 1),

        /// <summary>
        /// Represents the Death Knight class bit.
        /// </summary>
        [LocalizedName("Class6")]
        DeathKnight = 1 << (Classes.DeathKnight - 1),

        /// <summary>
        /// Represents the Shaman class bit.
        /// </summary>
        [LocalizedName("Class7")]
        Shaman      = 1 << (Classes.Shaman - 1),

        /// <summary>
        /// Represents the Mage class bit.
        /// </summary>
        [LocalizedName("Class8")]
        Mage        = 1 << (Classes.Mage - 1),

        /// <summary>
        /// Represents the Warlock class bit.
        /// </summary>
        [LocalizedName("Class9")]
        Warlock     = 1 << (Classes.Warlock - 1),

        /// <summary>
        /// Represents the Druid class bit.
        /// </summary>
        [LocalizedName("Class11")]
        Druid       = 1 << (Classes.Druid - 1),

        /// <summary>
        /// Represents all available World of Warcraft classes mask.
        /// </summary>
        [LocalizedName("AllClasses")]
        All = Warrior | Paladin | Hunter | Rogue | Priest | DeathKnight | Shaman | Mage | Warlock | Druid,

        [LocalizedName("AllClasses")]
        AllValues = uint.MaxValue
    }
}
