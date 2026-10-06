using System.Text.Json;
using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class ScoringServiceTests
{
    [Fact]
    public void PriorityFrom_CritValuePresetMatchesDefaultSettings()
    {
        var preset = ArtifactData.WeightPresets[0];
        Assert.Equal("Crit Value", preset.Label);

        var priority = ScoringService.PriorityFrom(preset.Weights);
        Assert.Equal([Stat.CritDMG, Stat.CritRate], priority);
        Assert.Equal(priority, new SimulationSettings().Priority);
    }

    [Fact]
    public void PriorityFrom_OrdersByWeightDescendingAndDropsNonPositive()
    {
        var priority = ScoringService.PriorityFrom(new Dictionary<int, double>
        {
            [Stat.CritRate] = 2,
            [Stat.CritDMG] = 5,
            [Stat.FlatHp] = 0,
            [Stat.FlatAtk] = -1,
            [Stat.EnergyRecharge] = 3,
        });

        Assert.Equal([Stat.CritDMG, Stat.EnergyRecharge, Stat.CritRate], priority);
    }

    [Fact]
    public void WeightsFromPriority_FirstPickCountsMost()
    {
        var weights = ScoringService.WeightsFromPriority([Stat.CritDMG, Stat.CritRate]);

        Assert.Equal(2, weights.Count);
        Assert.Equal(Stat.CritDMG, weights[0].Stat);
        Assert.Equal(1.0, weights[0].Weight);
        Assert.Equal(Stat.CritRate, weights[1].Stat);
        Assert.Equal(0.5, weights[1].Weight);
    }

    [Fact]
    public void ScoreArtifact_FallsBackToCritValueWithoutWeights()
    {
        var art = new ArtifactOutput { CritValue = 42.5 };

        Assert.Equal(42.5, ScoringService.ScoreArtifact(art, []));
    }

    [Fact]
    public void ScoreArtifact_UsesWeightedSum()
    {
        var art = new ArtifactOutput
        {
            CritValue = 10,
            SubStats =
            [
                new SubstatEntry { Type = Stat.CritDMG, Value = 7.8, Rolls = 1 },
                new SubstatEntry { Type = Stat.CritRate, Value = 3.9, Rolls = 1 },
            ]
        };

        double score = ScoringService.ScoreArtifact(art,
            [new SubstatWeight(Stat.CritDMG, 1.0), new SubstatWeight(Stat.CritRate, 0.5)]);

        Assert.Equal(7.8 + 3.9 * 0.5, score, precision: 10);
    }

    [Fact]
    public void RollValue_CountsCritByDefault()
    {
        var art = new ArtifactOutput
        {
            SubStats =
            [
                new SubstatEntry { Type = Stat.CritDMG, Value = 7.8, Rolls = 1 },
                new SubstatEntry { Type = Stat.FlatHp, Value = 298.8, Rolls = 1 },
            ]
        };

        // Only the max-rolled CritDMG counts: 100%. FlatHp is not crit.
        Assert.Equal(100.0, ScoringService.RollValue(art, []), precision: 10);
    }

    [Fact]
    public void GetRollTier_PicksClosestTier()
    {
        Assert.Equal("max", ScoringService.GetRollTier(Stat.CritRate, 3.89));
        Assert.Equal("low", ScoringService.GetRollTier(Stat.CritRate, 2.72));
        Assert.Equal("min", ScoringService.GetRollTier(999, 1.0));
    }

    [Fact]
    public void RvTier_MatchesThresholds()
    {
        Assert.Equal("cv-low", ScoringService.RvTier(100));
        Assert.Equal("cv-mid", ScoringService.RvTier(400));
        Assert.Equal("cv-max", ScoringService.RvTier(700));
    }

    [Fact]
    public void BuildSimulationConfig_IncludesSubstatWeights()
    {
        var settings = new SimulationSettings
        {
            Mode = SimulationMode.ResinBudget,
            ResinBudget = 2000,
            TopK = 10,
            UseStrongBox = true,
            MinCritValue = 0,
            Priority = [Stat.CritDMG, Stat.CritRate],
        };

        var json = ArtifactEngineService.BuildSimulationConfig(settings);
        using var doc = JsonDocument.Parse(json);
        var weights = doc.RootElement.GetProperty("substatWeights");

        Assert.Equal(2, weights.GetArrayLength());
        Assert.Equal(Stat.CritDMG, weights[0].GetProperty("stat").GetInt32());
        Assert.Equal(1.0, weights[0].GetProperty("weight").GetDouble());
        Assert.Equal(Stat.CritRate, weights[1].GetProperty("stat").GetInt32());
        Assert.Equal(0.5, weights[1].GetProperty("weight").GetDouble());
    }

    [Fact]
    public void SortByScore_OrdersDescending()
    {
        var weights = ScoringService.WeightsFromPriority([Stat.CritDMG]);
        var low = new ArtifactOutput { SubStats = [new SubstatEntry { Type = Stat.CritDMG, Value = 5.4, Rolls = 1 }] };
        var high = new ArtifactOutput { SubStats = [new SubstatEntry { Type = Stat.CritDMG, Value = 7.8, Rolls = 1 }] };
        var list = new List<ArtifactOutput> { low, high };

        ScoringService.SortByScore(list, weights);

        Assert.Same(high, list[0]);
        Assert.Same(low, list[1]);
    }
}
