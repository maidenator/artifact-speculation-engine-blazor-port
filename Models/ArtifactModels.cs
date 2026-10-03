using System.Text.Json.Serialization;

namespace ArtifactSpeculationBlazor.Models;

public class HuntItem
{
    public int? Slot { get; set; }
    public int? MainStat { get; set; }
    public List<int> DesiredSubstats { get; set; } = new();
    public string Domain { get; set; } = "Any";
    public string Set { get; set; } = "Any";
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
    public int[] Priority { get; set; } = [];
}
