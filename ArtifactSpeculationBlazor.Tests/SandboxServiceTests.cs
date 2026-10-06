using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class SandboxServiceTests
{
    private static ArtifactOutput Snapshot(params (int Type, double Value, int Rolls)[] subs) => new()
    {
        Slot = 0,
        Level = 0,
        MainStat = new MainStatEntry { Type = Stat.FlatHp, Value = 717 },
        SubStats = subs.Select(s => new SubstatEntry { Type = s.Type, Value = s.Value, Rolls = s.Rolls }).ToList(),
    };

    [Fact]
    public void ComputeDiff_DetectsNewFourthSubstat()
    {
        var current = Snapshot((Stat.CritDMG, 7.0, 1), (Stat.CritRate, 3.1, 1), (Stat.AtkPercent, 4.7, 1));
        var next = Snapshot((Stat.CritDMG, 7.0, 1), (Stat.CritRate, 3.1, 1), (Stat.AtkPercent, 4.7, 1), (Stat.EnergyRecharge, 5.2, 1));

        var diff = SandboxService.ComputeDiff(current, next);

        Assert.NotNull(diff);
        Assert.Equal(3, diff.SubstatIndex);
        Assert.Equal(5.2, diff.Delta, precision: 10);
        Assert.Equal(Stat.EnergyRecharge, diff.StatType);
    }

    [Fact]
    public void ComputeDiff_DetectsBoostedSubstat()
    {
        var current = Snapshot((Stat.CritDMG, 7.0, 1));
        var next = Snapshot((Stat.CritDMG, 13.2, 2));

        var diff = SandboxService.ComputeDiff(current, next);

        Assert.NotNull(diff);
        Assert.Equal(0, diff.SubstatIndex);
        Assert.Equal(6.2, diff.Delta, precision: 10);
    }

    [Fact]
    public void ComputeDiff_ReturnsNullWhenIdentical()
    {
        var current = Snapshot((Stat.CritDMG, 7.0, 1));
        var next = Snapshot((Stat.CritDMG, 7.0, 1));

        Assert.Null(SandboxService.ComputeDiff(current, next));
    }

    [Fact]
    public void PropagateRollTiers_StacksChronologically()
    {
        var history = new List<ArtifactOutput>
        {
            Snapshot((Stat.CritDMG, 7.0, 1)),
            Snapshot((Stat.CritDMG, 13.2, 2)),
        };

        SandboxService.PropagateRollTiers(history);

        Assert.Single(history[0].SubStats[0].RollTiers!);
        Assert.Equal(2, history[1].SubStats[0].RollTiers!.Length);
    }

    [Fact]
    public void PreviewSubstat_PeeksAtIncomingFourthStat()
    {
        var history = new List<ArtifactOutput>
        {
            Snapshot((Stat.CritDMG, 7.0, 1)),
            Snapshot((Stat.CritDMG, 7.0, 1), (Stat.CritRate, 3.1, 1)),
        };

        var preview = SandboxService.PreviewSubstat(history, 0);

        Assert.NotNull(preview);
        Assert.Equal(Stat.CritRate, preview.Type);
        Assert.Null(SandboxService.PreviewSubstat(history, 1));
    }

    [Fact]
    public void PrepareHistories_AssignsOneSetPerArtifact()
    {
        var histories = new List<List<ArtifactOutput>>
        {
            new() { Snapshot((Stat.CritDMG, 7.0, 1)), Snapshot((Stat.CritDMG, 13.2, 2)) },
            new() { Snapshot((Stat.CritRate, 3.1, 1)), Snapshot((Stat.CritRate, 6.2, 2)) },
        };

        var prepared = SandboxService.PrepareHistories(histories, "midsummer_courtyard", () => 0.99);

        // Coin 0.99 → sets[1] for both; consistent across the whole history.
        Assert.All(prepared, a => Assert.Equal("thundersoother", a.History[0].SetId));
        Assert.All(prepared, a => Assert.Equal(a.History[0].SetId, a.History[1].SetId));
        Assert.All(prepared, a => Assert.StartsWith("UI_RelicIcon_14002_", a.History[0].IconUrl));
    }
}
