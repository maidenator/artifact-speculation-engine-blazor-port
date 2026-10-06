using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Models;

/// <summary>
/// Artifact set within a domain.
/// </summary>
public class ArtifactSet
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int? EnkaId { get; set; }
}

/// <summary>
/// Farmable domain with exactly two artifact sets.
/// Ported from React <c>src/constants/domains.ts</c>.
/// </summary>
public class ArtifactDomain
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public ArtifactSet[] Sets { get; set; } = [];
}

public static class GameData
{
    public static readonly List<ArtifactDomain> Domains =
    [
        new() { Id = "midsummer_courtyard", Name = "Midsummer Courtyard", Sets = [new() { Id = "thundering_fury", Name = "Thundering Fury", EnkaId = 15005 }, new() { Id = "thundersoother", Name = "Thundersoother", EnkaId = 14002 }] },
        new() { Id = "domain_of_guyun", Name = "Domain of Guyun", Sets = [new() { Id = "archaic_petra", Name = "Archaic Petra", EnkaId = 15014 }, new() { Id = "retracing_bolide", Name = "Retracing Bolide", EnkaId = 15015 }] },
        new() { Id = "valley_of_remembrance", Name = "Valley of Remembrance", Sets = [new() { Id = "viridescent_venerer", Name = "Viridescent Venerer", EnkaId = 15002 }, new() { Id = "maiden_beloved", Name = "Maiden Beloved", EnkaId = 14004 }] },
        new() { Id = "hidden_palace_of_zhou_formula", Name = "Hidden Palace of Zhou Formula", Sets = [new() { Id = "crimson_witch_of_flames", Name = "Crimson Witch of Flames", EnkaId = 15006 }, new() { Id = "lavawalker", Name = "Lavawalker", EnkaId = 14003 }] },
        new() { Id = "clear_pool_and_mountain_cavern", Name = "Clear Pool and Mountain Cavern", Sets = [new() { Id = "bloodstained_chivalry", Name = "Bloodstained Chivalry", EnkaId = 15008 }, new() { Id = "noblesse_oblige", Name = "Noblesse Oblige", EnkaId = 15007 }] },
        new() { Id = "peak_of_vindagnyr", Name = "Peak of Vindagnyr", Sets = [new() { Id = "blizzard_strayer", Name = "Blizzard Strayer", EnkaId = 14001 }, new() { Id = "heart_of_depth", Name = "Heart of Depth", EnkaId = 15016 }] },
        new() { Id = "ridge_watch", Name = "Ridge Watch", Sets = [new() { Id = "tenacity_of_the_millelith", Name = "Tenacity of the Millelith", EnkaId = 15017 }, new() { Id = "pale_flame", Name = "Pale Flame", EnkaId = 15018 }] },
        new() { Id = "momiji_dyed_court", Name = "Momiji-Dyed Court", Sets = [new() { Id = "shimenawas_reminiscence", Name = "Shimenawa's Reminiscence", EnkaId = 15019 }, new() { Id = "emblem_of_severed_fate", Name = "Emblem of Severed Fate", EnkaId = 15020 }] },
        new() { Id = "slumbering_court", Name = "Slumbering Court", Sets = [new() { Id = "husk_of_opulent_dreams", Name = "Husk of Opulent Dreams", EnkaId = 15021 }, new() { Id = "ocean_hued_clam", Name = "Ocean-Hued Clam", EnkaId = 15022 }] },
        new() { Id = "the_lost_valley", Name = "The Lost Valley", Sets = [new() { Id = "vermillion_hereafter", Name = "Vermillion Hereafter", EnkaId = 15023 }, new() { Id = "echoes_of_an_offering", Name = "Echoes of an Offering", EnkaId = 15024 }] },
        new() { Id = "spire_of_solitary_enlightenment", Name = "Spire of Solitary Enlightenment", Sets = [new() { Id = "deepwood_memories", Name = "Deepwood Memories", EnkaId = 15025 }, new() { Id = "gilded_dreams", Name = "Gilded Dreams", EnkaId = 15026 }] },
        new() { Id = "city_of_gold", Name = "City of Gold", Sets = [new() { Id = "desert_pavilion_chronicle", Name = "Desert Pavilion Chronicle", EnkaId = 15027 }, new() { Id = "flower_of_paradise_lost", Name = "Flower of Paradise Lost", EnkaId = 15028 }] },
        new() { Id = "molten_iron_fortress", Name = "Molten Iron Fortress", Sets = [new() { Id = "nymphs_dream", Name = "Nymph's Dream", EnkaId = 15029 }, new() { Id = "vourukashas_glow", Name = "Vourukasha's Glow", EnkaId = 15030 }] },
        new() { Id = "denouement_of_sin", Name = "Denouement of Sin", Sets = [new() { Id = "marechaussee_hunter", Name = "Marechaussee Hunter", EnkaId = 15031 }, new() { Id = "golden_troupe", Name = "Golden Troupe", EnkaId = 15032 }] },
        new() { Id = "waterfall_ruin", Name = "Waterfall Ruin", Sets = [new() { Id = "nighttime_whispers", Name = "Nighttime Whispers in the Echoing Woods", EnkaId = 15034 }, new() { Id = "song_of_days_past", Name = "Song of Days Past", EnkaId = 15033 }] },
        new() { Id = "faded_theater", Name = "Faded Theater", Sets = [new() { Id = "fragment_of_harmonic_whimsy", Name = "Fragment of Harmonic Whimsy", EnkaId = 15035 }, new() { Id = "unfinished_reverie", Name = "Unfinished Reverie", EnkaId = 15036 }] },
        new() { Id = "sanctum_of_rainbow_spirits", Name = "Sanctum of Rainbow Spirits", Sets = [new() { Id = "scroll_of_the_hero_of_cinder_city", Name = "Scroll of the Hero of Cinder City", EnkaId = 15037 }, new() { Id = "obsidian_codex", Name = "Obsidian Codex", EnkaId = 15038 }] },
        new() { Id = "derelict_masonry_dock", Name = "Derelict Masonry Dock", Sets = [new() { Id = "finale_of_the_deep_galleries", Name = "Finale of the Deep Galleries", EnkaId = 15040 }, new() { Id = "long_nights_oath", Name = "Long Night's Oath", EnkaId = 15039 }] },
        new() { Id = "frostladen_machinery", Name = "Frostladen Machinery", Sets = [new() { Id = "night_of_the_skys_unveiling", Name = "Night of the Sky's Unveiling", EnkaId = 15041 }, new() { Id = "silken_moons_serenade", Name = "Silken Moon's Serenade", EnkaId = 15042 }] },
        new() { Id = "moonchilds_treasures", Name = "Moonchild's Treasures", Sets = [new() { Id = "a_day_carved_from_rising_winds", Name = "A Day Carved From Rising Winds", EnkaId = 15044 }, new() { Id = "aubade_of_morningstar_and_moon", Name = "Aubade of Morningstar and Moon", EnkaId = 15043 }] },
        new() { Id = "thorny_crown_of_the_mountain_wind", Name = "Thorny Crown of the Mountain Wind", Sets = [new() { Id = "celestial_gift", Name = "Celestial Gift", EnkaId = 15045 }, new() { Id = "disenchantment_in_deep_shadow", Name = "Disenchantment in Deep Shadow", EnkaId = 15046 }] },
        new() { Id = "inverted_glacier", Name = "Inverted Glacier", Sets = [new() { Id = "heart_of_the_furnace", Name = "Heart of the Furnace", EnkaId = 15048 }, new() { Id = "scarlet_proof", Name = "Scarlet Proof", EnkaId = 15047 }] },
    ];

    /// <summary>
    /// Valid main stats per slot. Ported from React <c>SLOT_MAIN_STATS</c>.
    /// </summary>
    public static readonly Dictionary<int, int[]> SlotMainStats = new()
    {
        [0] = [Stat.FlatHp],
        [1] = [Stat.FlatAtk],
        [2] = [Stat.HpPercent, Stat.AtkPercent, Stat.DefPercent, Stat.EnergyRecharge, Stat.ElementalMastery],
        [3] = [Stat.HpPercent, Stat.AtkPercent, Stat.DefPercent, Stat.PyroDMG, Stat.HydroDMG, Stat.ElectroDMG, Stat.CryoDMG, Stat.AnemoDMG, Stat.GeoDMG, Stat.DendroDMG, Stat.PhysicalDMG, Stat.ElementalMastery],
        [4] = [Stat.HpPercent, Stat.AtkPercent, Stat.DefPercent, Stat.CritRate, Stat.CritDMG, Stat.HealingBonus, Stat.ElementalMastery],
    };

    public static int[] MainStatsForSlot(int slot) =>
        SlotMainStats.TryGetValue(slot, out var stats) ? stats : [];

    public static List<ArtifactSet> AvailableSets(string? domainId)
    {
        if (string.IsNullOrEmpty(domainId))
            return Domains.SelectMany(d => d.Sets).ToList();
        return Domains.FirstOrDefault(d => d.Id == domainId)?.Sets.ToList() ?? [];
    }

    public static string? FindSetName(string? setId)
    {
        if (string.IsNullOrEmpty(setId))
            return null;
        return Domains.SelectMany(d => d.Sets).FirstOrDefault(s => s.Id == setId)?.Name;
    }

    public static bool SetBelongsToDomain(string setId, string domainId)
    {
        var domain = Domains.FirstOrDefault(d => d.Id == domainId);
        return domain?.Sets.Any(s => s.Id == setId) == true;
    }
}
