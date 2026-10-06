using System.Text.Json.Serialization;

namespace ArtifactSpeculationBlazor.Models;

public class HuntItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Slot { get; set; }
    public int MainStat { get; set; }
    public List<HuntSubstat> Substats { get; set; } = [];
    public string? DomainId { get; set; }
    public string? SetId { get; set; }
}

public class HuntSubstat
{
    public int Stat { get; set; }
    public int MinRolls { get; set; } = 1;
}

public class HuntItemResult
{
    public string HuntItemId { get; set; } = "";
    public bool Found { get; set; }
    public int ResinSpent { get; set; }
    public double DaysSpent { get; set; }
    public ArtifactOutput? Artifact { get; set; }
}

public class HuntListResult
{
    public bool AllFound { get; set; }
    public int TotalResinSpent { get; set; }
    public double TotalDays { get; set; }
    public int CondensedResin { get; set; }
    public int DomainRunsCompleted { get; set; }
    public int StrongboxRollsCompleted { get; set; }
    public List<HuntItemResult> ItemResults { get; set; } = [];
    public double ElapsedMs { get; set; }
}

public class SubstatEntry
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("rolls")]
    public int Rolls { get; set; }

    [JsonPropertyName("rollTiers")]
    public string[]? RollTiers { get; set; }
}

public class MainStatEntry
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class ArtifactOutput
{
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("mainStat")]
    public MainStatEntry MainStat { get; set; } = new();

    [JsonPropertyName("subStats")]
    public List<SubstatEntry> SubStats { get; set; } = [];

    [JsonPropertyName("critValue")]
    public double CritValue { get; set; }

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("setId")]
    public string? SetId { get; set; }
}

public class SimulationResult
{
    [JsonPropertyName("targetAchieved")]
    public bool TargetAchieved { get; set; }

    [JsonPropertyName("totalResinSpent")]
    public int TotalResinSpent { get; set; }

    [JsonPropertyName("equivalentDays")]
    public double EquivalentDays { get; set; }

    [JsonPropertyName("domainRunsCompleted")]
    public int DomainRunsCompleted { get; set; }

    [JsonPropertyName("strongboxRollsCompleted")]
    public int StrongboxRollsCompleted { get; set; }

    [JsonPropertyName("totalFiveStarsFound")]
    public int TotalFiveStarsFound { get; set; }

    [JsonPropertyName("topArtifacts")]
    public List<ArtifactOutput> TopArtifacts { get; set; } = [];
}

public enum SimulationMode
{
    ResinBudget = 0,
    TargetPiece = 1
}

public class SimulationSettings
{
    public SimulationMode Mode { get; set; } = SimulationMode.TargetPiece;
    public int ResinBudget { get; set; } = 2000;
    public int TopK { get; set; } = 10;
    public bool UseStrongBox { get; set; } = true;
    public double MinCritValue { get; set; } = 25;
    public int? TargetSlot { get; set; }
    public int? TargetMainStat { get; set; }

    /// <summary>
    /// Default matches React: priorityFrom(WEIGHT_PRESETS[0]) = Crit DMG + Crit Rate.
    /// </summary>
    public int[] Priority { get; set; } = [Stat.CritDMG, Stat.CritRate];
}

public class SubstatWeight
{
    [JsonPropertyName("stat")]
    public int Stat { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    public SubstatWeight() { }

    public SubstatWeight(int stat, double weight)
    {
        Stat = stat;
        Weight = weight;
    }
}
