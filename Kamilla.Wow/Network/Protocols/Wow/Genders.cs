
namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Represents World of Warcraft genders.
    /// </summary>
    [LocalizedNameContainer(typeof(WowStrings))]
    public enum Genders
    {
        /// <summary>
        /// Represents male gender.
        /// </summary>
        [LocalizedName("Gender0")]
        Male = 0,

        /// <summary>
        /// Represents female gender.
        /// </summary>
        [LocalizedName("Gender1")]
        Female = 1,

        /// <summary>
        /// Represents an unknown or undefined gender.
        /// </summary>
        [LocalizedName("Gender2")]
        None = 2,
    }
}
