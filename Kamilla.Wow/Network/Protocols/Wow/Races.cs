using System.ComponentModel;

namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Represents playable and not playable races of World of Warcraft.
    /// 
    /// When using localized names of this enum, pass gender as the first argument.
    /// </summary>
    [LocalizedNameContainer(typeof(WowStrings))]
    public enum Races
    {
        /// <summary>
        /// Represents the Human race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race1")]
        Human = 1,

        /// <summary>
        /// Represents the Orc race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race2")]
        Orc = 2,

        /// <summary>
        /// Represents the Dwarf race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race3")]
        Dwarf = 3,

        /// <summary>
        /// Represents the Night Elf race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race4")]
        NightElf = 4,

        /// <summary>
        /// Represents the Undead race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race5")]
        Undead = 5,

        /// <summary>
        /// Represents the Tauren race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race6")]
        Tauren = 6,

        /// <summary>
        /// Represents the Gnome race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race7")]
        Gnome = 7,

        /// <summary>
        /// Represents the Troll race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race8")]
        Troll = 8,

        /// <summary>
        /// Represents the Goblin race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race9")]
        Goblin = 9,

        /// <summary>
        /// Represents the Blood Elf race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race10")]
        BloodElf = 10,

        /// <summary>
        /// Represents the Draenei race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race11")]
        Draenei = 11,

        /// <summary>
        /// Represents the Fel Orc race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race12")]
        FelOrc = 12,

        /// <summary>
        /// Represents the Naga race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race13")]
        Naga = 13,

        /// <summary>
        /// Represents the Broken race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race14")]
        Broken = 14,

        /// <summary>
        /// Represents the Skeleton race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race15")]
        Skeleton = 15,

        /// <summary>
        /// Represents the Vrykul race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race16")]
        Vrykul = 16,

        /// <summary>
        /// Represents the Tuskarr race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race17")]
        Tuskarr = 17,

        /// <summary>
        /// Represents the Forest Troll race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race18")]
        ForestTroll = 18,

        /// <summary>
        /// Represents the Taunka race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race19")]
        Taunka = 19,

        /// <summary>
        /// Represents the Northrend Skeleton race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race20")]
        NorthrendSkeleton = 20,

        /// <summary>
        /// Represents the Naga race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race21")]
        IceTroll = 21,

        /// <summary>
        /// Represents the Worgen race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race22")]
        Worgen = 22,

        /// <summary>
        /// Represents the Gilneas Human race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race23")]
        GilneasHuman = 23,

        /// <summary>
        /// Represents all Horde races. These races are playable.
        /// </summary>
        [LocalizedName("HordeRaces")]
        Horde = Orc | Troll | Goblin | BloodElf | Undead | Tauren,

        /// <summary>
        /// Represents all Alliance races. These races are playable.
        /// </summary>
        [LocalizedName("AllianceRaces")]
        Alliance = Human | Dwarf | Gnome | NightElf | Draenei | Worgen,

        /// <summary>
        /// Represents all playable races. That is, all Horde and all Alliance races.
        /// </summary>
        [LocalizedName("PlayableRaces")]
        Playable = Horde | Alliance,

        /// <summary>
        /// Represents all not playable races.
        /// </summary>
        [LocalizedName("NotPlayableRaces")]
        NotPlayable = FelOrc | Naga | Skeleton | NorthrendSkeleton | Broken
            | IceTroll | ForestTroll | Vrykul | Tuskarr | GilneasHuman | Taunka,

        /// <summary>
        /// Represents all races.
        /// </summary>
        [LocalizedName("AllRaces")]
        All = Playable | NotPlayable
    }
}
