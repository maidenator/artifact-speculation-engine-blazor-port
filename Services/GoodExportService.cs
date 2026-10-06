using System.Text.Json.Serialization;
using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// GOOD-format export (https://frzyc.github.io/genshin-optimizer/...).
/// Ported from React <c>src/utils/good.ts</c>, but maps <c>SetId</c> to its
/// GOOD key instead of always writing GladiatorsFinale (React P2 bug).
/// Unknown/null sets fall back to GladiatorsFinale.
/// </summary>
public static class GoodExportService
{
    public const string DefaultSetKey = "GladiatorsFinale";

    private static readonly Dictionary<int, string> StatToGood = new()
    {
        [Stat.CritDMG] = "critDMG_",
        [Stat.CritRate] = "critRate_",
        [Stat.ElementalMastery] = "eleMas",
        [Stat.EnergyRecharge] = "enerRech_",
        [Stat.AtkPercent] = "atk_",
        [Stat.FlatAtk] = "atk",
        [Stat.HpPercent] = "hp_",
        [Stat.FlatHp] = "hp",
        [Stat.DefPercent] = "def_",
        [Stat.FlatDef] = "def",
        [Stat.HealingBonus] = "heal_",
        [Stat.PyroDMG] = "pyro_dmg_",
        [Stat.HydroDMG] = "hydro_dmg_",
        [Stat.ElectroDMG] = "electro_dmg_",
        [Stat.CryoDMG] = "cryo_dmg_",
        [Stat.AnemoDMG] = "anemo_dmg_",
        [Stat.GeoDMG] = "geo_dmg_",
        [Stat.DendroDMG] = "dendro_dmg_",
        [Stat.PhysicalDMG] = "physical_dmg_",
    };

    private static readonly Dictionary<int, string> SlotToGood = new()
    {
        [0] = "flower",
        [1] = "plume",
        [2] = "sands",
        [3] = "goblet",
        [4] = "circlet",
    };

    /// <summary>
    /// Explicit GOOD set keys. Falls back to PascalCase conversion for any
    /// set missing here (newer sets — verify against the GOOD spec).
    /// </summary>
    private static readonly Dictionary<string, string> SetToGood = new()
    {
        ["thundering_fury"] = "ThunderingFury",
        ["thundersoother"] = "Thundersoother",
        ["archaic_petra"] = "ArchaicPetra",
        ["retracing_bolide"] = "RetracingBolide",
        ["viridescent_venerer"] = "ViridescentVenerer",
        ["maiden_beloved"] = "MaidenBeloved",
        ["crimson_witch_of_flames"] = "CrimsonWitchOfFlames",
        ["lavawalker"] = "Lavawalker",
        ["bloodstained_chivalry"] = "BloodstainedChivalry",
        ["noblesse_oblige"] = "NoblesseOblige",
        ["blizzard_strayer"] = "BlizzardStrayer",
        ["heart_of_depth"] = "HeartOfDepth",
        ["tenacity_of_the_millelith"] = "TenacityOfTheMillelith",
        ["pale_flame"] = "PaleFlame",
        ["shimenawas_reminiscence"] = "ShimenawasReminiscence",
        ["emblem_of_severed_fate"] = "EmblemOfSeveredFate",
        ["husk_of_opulent_dreams"] = "HuskOfOpulentDreams",
        ["ocean_hued_clam"] = "OceanHuedClam",
        ["vermillion_hereafter"] = "VermillionHereafter",
        ["echoes_of_an_offering"] = "EchoesOfAnOffering",
        ["deepwood_memories"] = "DeepwoodMemories",
        ["gilded_dreams"] = "GildedDreams",
        ["desert_pavilion_chronicle"] = "DesertPavilionChronicle",
        ["flower_of_paradise_lost"] = "FlowerOfParadiseLost",
        ["nymphs_dream"] = "NymphsDream",
        ["vourukashas_glow"] = "VourukashasGlow",
        ["marechaussee_hunter"] = "MarechausseeHunter",
        ["golden_troupe"] = "GoldenTroupe",
        ["nighttime_whispers"] = "NighttimeWhispersInTheEchoingWoods",
        ["song_of_days_past"] = "SongOfDaysPast",
        ["fragment_of_harmonic_whimsy"] = "FragmentOfHarmonicWhimsy",
        ["unfinished_reverie"] = "UnfinishedReverie",
        ["scroll_of_the_hero_of_cinder_city"] = "ScrollOfTheHeroOfCinderCity",
        ["obsidian_codex"] = "ObsidianCodex",
        ["finale_of_the_deep_galleries"] = "FinaleOfTheDeepGalleries",
        ["long_nights_oath"] = "LongNightsOath",
    };

    public static string MapStat(int stat) =>
        StatToGood.TryGetValue(stat, out var key) ? key : "hp";

    public static string MapSlot(int slot) =>
        SlotToGood.TryGetValue(slot, out var key) ? key : "flower";

    public static string MapSet(string? setId)
    {
        if (!string.IsNullOrEmpty(setId))
        {
            if (SetToGood.TryGetValue(setId, out var key))
                return key;
            // PascalCase fallback for sets missing above (verify vs GOOD spec).
            return string.Concat(setId.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
        }
        return DefaultSetKey;
    }

    public static GoodArtifact ToGoodArtifact(ArtifactOutput art, string location = "", bool locked = false) => new()
    {
        SetKey = MapSet(art.SetId),
        SlotKey = MapSlot(art.Slot),
        Level = art.Level,
        Rarity = 5,
        MainStatKey = MapStat(art.MainStat.Type),
        Location = location,
        Lock = locked,
        Substats = art.SubStats
            .Select(s => new GoodSubstat { Key = MapStat(s.Type), Value = s.Value })
            .ToList(),
    };

    public static GoodDocument ToGoodDocument(IEnumerable<ArtifactOutput> artifacts) => new()
    {
        Format = "GOOD",
        Version = 3,
        Source = "ArtifactSpeculationEngineBlazor",
        Artifacts = artifacts.Select(a => ToGoodArtifact(a)).ToList(),
    };

    public class GoodDocument
    {
        [JsonPropertyName("format")] public string Format { get; set; } = "GOOD";
        [JsonPropertyName("version")] public int Version { get; set; } = 3;
        [JsonPropertyName("source")] public string Source { get; set; } = "";
        [JsonPropertyName("artifacts")] public List<GoodArtifact> Artifacts { get; set; } = [];
    }

    public class GoodArtifact
    {
        [JsonPropertyName("setKey")] public string SetKey { get; set; } = "";
        [JsonPropertyName("slotKey")] public string SlotKey { get; set; } = "";
        [JsonPropertyName("level")] public int Level { get; set; }
        [JsonPropertyName("rarity")] public int Rarity { get; set; } = 5;
        [JsonPropertyName("mainStatKey")] public string MainStatKey { get; set; } = "";
        [JsonPropertyName("location")] public string Location { get; set; } = "";
        [JsonPropertyName("lock")] public bool Lock { get; set; }
        [JsonPropertyName("substats")] public List<GoodSubstat> Substats { get; set; } = [];
    }

    public class GoodSubstat
    {
        [JsonPropertyName("key")] public string Key { get; set; } = "";
        [JsonPropertyName("value")] public double Value { get; set; }
    }
}
