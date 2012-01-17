
namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Defines World of Warcraft expansion sets.
    /// </summary>
    [LocalizedNameContainer(typeof(WowStrings))]
    public enum Expansions
    {
        /// <summary>
        /// Represents the original, classic World of Warcraft.
        /// </summary>
        [LocalizedName("Expac0")]
        Original = 0,

        /// <summary>
        /// Represents the first expansion set to the World of Warcraft, The Burning Crusade.
        /// </summary>
        [LocalizedName("Expac1")]
        TheBurningCrusade = 1,

        /// <summary>
        /// Represents the second expansion set to the World of Warcraft, Wrath of the Lich King.
        /// </summary>
        [LocalizedName("Expac2")]
        WrathOfTheLichKing = 2,

        /// <summary>
        /// Represents the third expansion set to the World of Warcraft, Cataclysm.
        /// </summary>
        [LocalizedName("Expac3")]
        Cataclysm = 3,

        /// <summary>
        /// Represents the fourth expansion set to the World of Warcraft, Mists of Pandaria.
        /// </summary>
        [LocalizedName("Expac4")]
        MistsOfPandaria = 4,
    }
}
