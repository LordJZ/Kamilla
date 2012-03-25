using System;
using System.ComponentModel;

namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Represents bit mask of races of World of Warcraft.
    /// 
    /// When using localized names of this enum, pass gender as the first argument.
    /// </summary>
    [Flags]
    [LocalizedNameContainer(typeof(WowStrings))]
    public enum RaceMask : uint
    {
        /// <summary>
        /// Represents the empty race mask.
        /// </summary>
        [LocalizedName("NoRaces")]
        None = 0,

        /// <summary>
        /// Represents the Human race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race1")]
        Human = 1 << (Races.Human - 1),

        /// <summary>
        /// Represents the Orc race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race2")]
        Orc = 1 << (Races.Orc - 1),

        /// <summary>
        /// Represents the Dwarf race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race3")]
        Dwarf = 1 << (Races.Dwarf - 1),

        /// <summary>
        /// Represents the Night Elf race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race4")]
        NightElf = 1 << (Races.NightElf - 1),

        /// <summary>
        /// Represents the Undead race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race5")]
        Undead = 1 << (Races.Undead - 1),

        /// <summary>
        /// Represents the Tauren race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race6")]
        Tauren = 1 << (Races.Tauren - 1),

        /// <summary>
        /// Represents the Gnome race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race7")]
        Gnome = 1 << (Races.Gnome - 1),

        /// <summary>
        /// Represents the Troll race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race8")]
        Troll = 1 << (Races.Troll - 1),

        /// <summary>
        /// Represents the Goblin race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race9")]
        Goblin = 1 << (Races.Goblin - 1),

        /// <summary>
        /// Represents the Blood Elf race. This race belongs to Horde. This race is playable.
        /// </summary>
        [LocalizedName("Race10")]
        BloodElf = 1 << (Races.BloodElf - 1),

        /// <summary>
        /// Represents the Draenei race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race11")]
        Draenei = 1 << (Races.Draenei - 1),

        /// <summary>
        /// Represents the Fel Orc race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race12")]
        FelOrc = 1 << (Races.FelOrc - 1),

        /// <summary>
        /// Represents the Naga race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race13")]
        Naga = 1 << (Races.Naga - 1),

        /// <summary>
        /// Represents the Broken race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race14")]
        Broken = 1 << (Races.Broken - 1),

        /// <summary>
        /// Represents the Skeleton race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race15")]
        Skeleton = 1 << (Races.Skeleton - 1),

        /// <summary>
        /// Represents the Vrykul race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race16")]
        Vrykul = 1 << (Races.Vrykul - 1),

        /// <summary>
        /// Represents the Tuskarr race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race17")]
        Tuskarr = 1 << (Races.Tuskarr - 1),

        /// <summary>
        /// Represents the Forest Troll race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race18")]
        ForestTroll = 1 << (Races.ForestTroll - 1),

        /// <summary>
        /// Represents the Taunka race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race19")]
        Taunka = 1 << (Races.Taunka - 1),

        /// <summary>
        /// Represents the Northrend Skeleton race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race20")]
        NorthrendSkeleton = 1 << (Races.NorthrendSkeleton - 1),

        /// <summary>
        /// Represents the Naga race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race21")]
        IceTroll = 1 << (Races.IceTroll - 1),

        /// <summary>
        /// Represents the Worgen race. This race belongs to Alliance. This race is playable.
        /// </summary>
        [LocalizedName("Race22")]
        Worgen = 1 << (Races.Worgen - 1),

        /// <summary>
        /// Represents the Gilneas Human race. This race is not playable.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [LocalizedName("Race23")]
        GilneasHuman = 1 << (Races.GilneasHuman - 1),

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
        All = Playable | NotPlayable,

        [LocalizedName("AllRaces")]
        AllValues = uint.MaxValue
    }
}
