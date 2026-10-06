using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Pure sandbox-history logic ported from React <c>ArtifactSandbox</c>:
/// set assignment, roll-tier propagation across the +0…+20 snapshots,
/// and per-step diff computation for upgrade animations.
/// </summary>
public static class SandboxService
{
    public static readonly string[] LevelLabels = ["+0", "+4", "+8", "+12", "+16", "+20"];

    private static readonly Dictionary<int, string> SlotSuffix = new()
    {
        [0] = "4", [1] = "2", [2] = "5", [3] = "1", [4] = "3",
    };

    /// <summary>
    /// Assigns a single set per artifact (one 50/50 coin for the whole history)
    /// and builds chronological roll-tier stacks for every snapshot.
    /// </summary>
    public static List<SandboxArtifact> PrepareHistories(
        List<List<ArtifactOutput>> histories,
        string? domainId,
        Func<double>? coin = null)
    {
        coin ??= Random.Shared.NextDouble;
        var domain = string.IsNullOrEmpty(domainId)
            ? null
            : GameData.Domains.FirstOrDefault(d => d.Id == domainId);

        return histories.Select(history =>
        {
            if (domain is not null)
            {
                var chosen = coin() < 0.5 ? domain.Sets[0] : domain.Sets[1];
                foreach (var snapshot in history)
                {
                    snapshot.SetId = chosen.Id;
                    if (chosen.EnkaId.HasValue)
                    {
                        var suffix = SlotSuffix.GetValueOrDefault(snapshot.Slot, "4");
                        snapshot.IconUrl = $"UI_RelicIcon_{chosen.EnkaId}_{suffix}.png";
                    }
                }
            }

            PropagateRollTiers(history);
            return new SandboxArtifact { History = history };
        }).ToList();
    }

    /// <summary>
    /// Step 0 gets inferred tiers; each later step copies the previous tiers
    /// and appends the exact tier of the changed roll (new 4th substat or boost).
    /// </summary>
    public static void PropagateRollTiers(List<ArtifactOutput> history)
    {
        if (history.Count == 0)
            return;

        foreach (var sub in history[0].SubStats)
            sub.RollTiers = ArtifactData.InferRollTiers(sub.Type, sub.Value, sub.Rolls);

        for (int s = 1; s < history.Count; s++)
        {
            var prev = history[s - 1];
            var curr = history[s];

            for (int i = 0; i < curr.SubStats.Count; i++)
                curr.SubStats[i].RollTiers = i < prev.SubStats.Count
                    ? [.. (prev.SubStats[i].RollTiers ?? [])]
                    : [];

            var diff = ComputeDiff(prev, curr);
            if (diff is not null && curr.SubStats[diff.SubstatIndex].RollTiers is not null)
                curr.SubStats[diff.SubstatIndex].RollTiers =
                    [.. curr.SubStats[diff.SubstatIndex].RollTiers!, diff.RollTier];
        }
    }

    /// <summary>
    /// Returns which substat changed between two snapshots, or null if identical.
    /// Mirrors React <c>computeDiff</c>.
    /// </summary>
    public static SandboxDiff? ComputeDiff(ArtifactOutput current, ArtifactOutput next)
    {
        if (next.SubStats.Count > current.SubStats.Count)
        {
            var added = next.SubStats[^1];
            return new SandboxDiff(
                next.SubStats.Count - 1,
                added.Value,
                ScoringService.GetRollTier(added.Type, added.Value),
                added.Type);
        }

        for (int i = 0; i < current.SubStats.Count && i < next.SubStats.Count; i++)
        {
            if (Math.Abs(next.SubStats[i].Value - current.SubStats[i].Value) > 0.001)
            {
                double delta = next.SubStats[i].Value - current.SubStats[i].Value;
                return new SandboxDiff(
                    i,
                    delta,
                    ScoringService.GetRollTier(next.SubStats[i].Type, delta),
                    next.SubStats[i].Type);
            }
        }

        return null;
    }

    /// <summary>
    /// For 3-liner artifacts, peek at the next step for the incoming 4th substat.
    /// </summary>
    public static SubstatEntry? PreviewSubstat(List<ArtifactOutput> history, int step)
    {
        if (step < 0 || step + 1 >= history.Count)
            return null;
        var display = history[step];
        var next = history[step + 1];
        if (display.SubStats.Count < 4 && next.SubStats.Count > display.SubStats.Count)
            return next.SubStats[^1];
        return null;
    }

    public record SandboxDiff(int SubstatIndex, double Delta, string RollTier, int StatType);
}
