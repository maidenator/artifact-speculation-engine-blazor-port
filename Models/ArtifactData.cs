namespace ArtifactSpeculationBlazor.Models;

/// <summary>
/// Mirrors the stat IDs used by the C++ engine.
/// </summary>
public static class Stat
{
    public const int CritDMG = 0;
    public const int CritRate = 1;
    public const int ElementalMastery = 2;
    public const int EnergyRecharge = 3;
    public const int AtkPercent = 4;
    public const int FlatAtk = 5;
    public const int HpPercent = 6;
    public const int FlatHp = 7;
    public const int DefPercent = 8;
    public const int FlatDef = 9;
    public const int HealingBonus = 10;
    public const int PyroDMG = 11;
    public const int HydroDMG = 12;
    public const int ElectroDMG = 13;
    public const int CryoDMG = 14;
    public const int AnemoDMG = 15;
    public const int GeoDMG = 16;
    public const int DendroDMG = 17;
    public const int PhysicalDMG = 18;
}

public static class ArtifactData
{
    public static readonly string[] SlotNames = ["Flower", "Feather", "Sands", "Goblet", "Circlet"];

    public static readonly Dictionary<int, string> SlotIcons = new()
    {
        [0] = "icons/slot/flower.png",
        [1] = "icons/slot/feather.png",
        [2] = "icons/slot/sands.png",
        [3] = "icons/slot/goblet.png",
        [4] = "icons/slot/circlet.png",
    };

    public static readonly Dictionary<int, string> MainStatNames = new()
    {
        [Stat.CritDMG] = "Crit DMG",
        [Stat.CritRate] = "Crit Rate",
        [Stat.ElementalMastery] = "Elemental Mastery",
        [Stat.EnergyRecharge] = "Energy Recharge",
        [Stat.AtkPercent] = "ATK %",
        [Stat.FlatAtk] = "ATK",
        [Stat.HpPercent] = "HP %",
        [Stat.FlatHp] = "HP",
        [Stat.DefPercent] = "DEF %",
        [Stat.FlatDef] = "DEF",
        [Stat.HealingBonus] = "Healing Bonus",
        [Stat.PyroDMG] = "Pyro DMG Bonus",
        [Stat.HydroDMG] = "Hydro DMG Bonus",
        [Stat.ElectroDMG] = "Electro DMG Bonus",
        [Stat.CryoDMG] = "Cryo DMG Bonus",
        [Stat.AnemoDMG] = "Anemo DMG Bonus",
        [Stat.GeoDMG] = "Geo DMG Bonus",
        [Stat.DendroDMG] = "Dendro DMG Bonus",
        [Stat.PhysicalDMG] = "Physical DMG Bonus",
    };

    public static readonly Dictionary<int, string> SubStatNames = new()
    {
        [Stat.CritDMG] = "Crit DMG",
        [Stat.CritRate] = "Crit Rate",
        [Stat.ElementalMastery] = "Elemental Mastery",
        [Stat.EnergyRecharge] = "Energy Recharge",
        [Stat.AtkPercent] = "ATK%",
        [Stat.FlatAtk] = "ATK",
        [Stat.HpPercent] = "HP%",
        [Stat.FlatHp] = "HP",
        [Stat.DefPercent] = "DEF%",
        [Stat.FlatDef] = "DEF",
    };

    public static readonly HashSet<int> FlatStats = [Stat.ElementalMastery, Stat.FlatAtk, Stat.FlatHp, Stat.FlatDef];

    public static readonly Dictionary<int, double> MaxSubstatRoll = new()
    {
        [Stat.CritDMG] = 7.8,
        [Stat.CritRate] = 3.9,
        [Stat.ElementalMastery] = 23.3,
        [Stat.EnergyRecharge] = 6.5,
        [Stat.AtkPercent] = 5.8,
        [Stat.FlatAtk] = 19.5,
        [Stat.HpPercent] = 5.8,
        [Stat.FlatHp] = 298.8,
        [Stat.DefPercent] = 7.3,
        [Stat.FlatDef] = 23.2,
    };

    /// <summary>
    /// The 4 exact rounded roll values per stat tier (low, mid, high, max).
    /// </summary>
    public static readonly Dictionary<int, double[]> RollValuesRounded = new()
    {
        [Stat.CritDMG] = [5.4, 6.2, 7.0, 7.8],
        [Stat.CritRate] = [2.7, 3.1, 3.5, 3.9],
        [Stat.EnergyRecharge] = [4.5, 5.2, 5.8, 6.5],
        [Stat.ElementalMastery] = [16.3, 18.7, 21.0, 23.3],
        [Stat.AtkPercent] = [4.1, 4.7, 5.3, 5.8],
        [Stat.HpPercent] = [4.1, 4.7, 5.3, 5.8],
        [Stat.DefPercent] = [5.1, 5.8, 6.6, 7.3],
        [Stat.FlatAtk] = [13.6, 15.6, 17.5, 19.5],
        [Stat.FlatDef] = [16.2, 18.5, 20.8, 23.2],
        [Stat.FlatHp] = [209.1, 239.0, 268.9, 298.8],
    };

    public static readonly string[] TierNames = ["low", "mid", "high", "max"];

    public static string FormatStat(int id, double value) =>
        FlatStats.Contains(id) ? $"+{Math.Round(value):N0}" : $"+{value:F1}%";

    public static string GetSlotName(int slot) =>
        slot >= 0 && slot < SlotNames.Length ? SlotNames[slot] : "Piece";

    public static string? GetSlotIcon(int slot) =>
        SlotIcons.GetValueOrDefault(slot);

    public static string GetMainStatName(int type) =>
        MainStatNames.GetValueOrDefault(type, $"Stat {type}");

    public static string GetSubStatName(int type) =>
        SubStatNames.GetValueOrDefault(type, $"Stat {type}");

    public static string? GetStatIcon(string statName)
    {
        if (statName.Contains("Anemo")) return "icons/element/anemo.png";
        if (statName.Contains("Cryo")) return "icons/element/cryo.png";
        if (statName.Contains("Dendro")) return "icons/element/dendro.png";
        if (statName.Contains("Electro")) return "icons/element/electro.png";
        if (statName.Contains("Geo")) return "icons/element/geo.png";
        if (statName.Contains("Hydro")) return "icons/element/hydro.png";
        if (statName.Contains("Pyro")) return "icons/element/pyro.png";
        if (statName.Contains("Physical")) return "icons/element/physical.png";

        if (statName.Contains("ATK %")) return "icons/stat/attack_percent.png";
        if (statName.Contains("DEF %")) return "icons/stat/defense_percent.png";
        if (statName.Contains("HP %")) return "icons/stat/hp_percent.png";

        if (statName.Contains("ATK")) return "icons/stat/attack.png";
        if (statName.Contains("DEF")) return "icons/stat/defense.png";
        if (statName.Contains("HP")) return "icons/stat/hp.png";

        if (statName.Contains("Crit DMG")) return "icons/stat/crit_damage.png";
        if (statName.Contains("Crit Rate")) return "icons/stat/crit_rate.png";
        if (statName.Contains("Elemental Mastery")) return "icons/stat/elemental_mastery.png";
        if (statName.Contains("Energy Recharge")) return "icons/stat/energy_recharge.png";
        if (statName.Contains("Healing")) return "icons/stat/healing_bonus.png";

        return null;
    }

    public static string CvTier(double cv) =>
        cv > 50 ? "cv-max" : cv >= 40 ? "cv-top" : cv >= 30 ? "cv-high" : cv >= 20 ? "cv-mid" : "cv-low";

    /// <summary>
    /// Reverse engineer the sequence of roll tiers from a total substat value.
    /// Uses a greedy approach to produce a plausible coloring for the dots.
    /// </summary>
    public static string[] InferRollTiers(int statType, double totalValue, int rolls)
    {
        if (!RollValuesRounded.TryGetValue(statType, out var values) || rolls <= 0)
            return Enumerable.Repeat("min", Math.Max(1, rolls)).ToArray();

        var tiers = new List<string>();
        double remaining = totalValue;

        for (int r = 0; r < rolls; r++)
        {
            int rollsLeft = rolls - 1 - r;
            int bestTierIdx = 0;
            double minError = double.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                double val = values[i];
                double targetRest = remaining - val;
                double avgRequired = rollsLeft == 0 ? 0 : targetRest / rollsLeft;

                double error = 0;
                if (rollsLeft == 0)
                    error = Math.Abs(targetRest);
                else if (avgRequired < values[0])
                    error = values[0] - avgRequired;
                else if (avgRequired > values[3])
                    error = avgRequired - values[3];

                if (error < minError)
                {
                    minError = error;
                    bestTierIdx = i;
                }
            }

            tiers.Add(TierNames[bestTierIdx]);
            remaining -= values[bestTierIdx];
        }

        tiers.Sort((a, b) => Array.IndexOf(TierNames, a).CompareTo(Array.IndexOf(TierNames, b)));
        return tiers.ToArray();
    }
}
