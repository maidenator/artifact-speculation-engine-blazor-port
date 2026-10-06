using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Pure scoring logic ported from the React frontend (<c>src/utils/scoring.ts</c>).
/// Ranked priority list → weights: first pick counts most, last pick least.
/// </summary>
public static class ScoringService
{
    /// <summary>
    /// Ordered stat ids from a weight preset, highest weight first.
    /// Mirrors <c>priorityFrom</c>.
    /// </summary>
    public static int[] PriorityFrom(Dictionary<int, double> partial)
    {
        return partial
            .Where(kv => kv.Value > 0)
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToArray();
    }

    /// <summary>
    /// Ranked list → weights. Mirrors <c>weightsFromPriority</c>.
    /// </summary>
    public static List<SubstatWeight> WeightsFromPriority(int[] order)
    {
        var weights = new List<SubstatWeight>(order.Length);
        for (int i = 0; i < order.Length; i++)
            weights.Add(new SubstatWeight(order[i], (double)(order.Length - i) / order.Length));
        return weights;
    }

    /// <summary>
    /// Weighted substat score. Falls back to crit value when no weights.
    /// Mirrors <c>scoreArtifact</c>.
    /// </summary>
    public static double ScoreArtifact(ArtifactOutput art, List<SubstatWeight> substatWeights)
    {
        if (substatWeights.Count == 0)
            return art.CritValue;

        double total = 0;
        foreach (var sub in art.SubStats)
        {
            var match = substatWeights.Find(w => w.Stat == sub.Type);
            if (match is not null)
                total += sub.Value * match.Weight;
        }
        return total;
    }

    /// <summary>
    /// Roll Value: every counted substat adds value / max roll as a percent.
    /// 100% = one max roll. Counts crit stats when priority is empty.
    /// Mirrors <c>rollValue</c>.
    /// </summary>
    public static double RollValue(ArtifactOutput art, int[] priority)
    {
        int[] counted = priority.Length > 0 ? priority : [Stat.CritDMG, Stat.CritRate];
        var countedSet = new HashSet<int>(counted);
        double total = 0;
        foreach (var sub in art.SubStats)
        {
            if (countedSet.Contains(sub.Type) && ArtifactData.MaxSubstatRoll.TryGetValue(sub.Type, out var max) && max > 0)
                total += (sub.Value / max) * 100;
        }
        return total;
    }

    public static string RvTier(double rv) =>
        rv >= 700 ? "cv-max" : rv >= 600 ? "cv-top" : rv >= 500 ? "cv-high" : rv >= 400 ? "cv-mid" : "cv-low";

    /// <summary>
    /// Find which tier (low/mid/high/max) a single roll delta is closest to.
    /// Mirrors <c>getRollTier</c>.
    /// </summary>
    public static string GetRollTier(int statType, double delta)
    {
        if (!ArtifactData.RollValuesRounded.TryGetValue(statType, out var values))
            return "min";

        int bestIdx = 0;
        double minDiff = double.MaxValue;
        for (int i = 0; i < values.Length; i++)
        {
            double diff = Math.Abs(values[i] - delta);
            if (diff < minDiff)
            {
                minDiff = diff;
                bestIdx = i;
            }
        }
        return ArtifactData.TierNames[bestIdx];
    }

    /// <summary>
    /// Estimate roll count from a total value via average-tier heuristic.
    /// Mirrors <c>inferRollCount</c>.
    /// </summary>
    public static int InferRollCount(int statType, double totalValue)
    {
        if (!ArtifactData.RollValuesRounded.TryGetValue(statType, out var values))
            return 1;
        double avg = (values[0] + values[^1]) / 2;
        return Math.Max(1, (int)Math.Round(totalValue / avg));
    }

    /// <summary>
    /// Sort artifacts by score descending (client-side parity with React's re-sort).
    /// </summary>
    public static void SortByScore(List<ArtifactOutput> artifacts, List<SubstatWeight> weights)
    {
        artifacts.Sort((a, b) => ScoreArtifact(b, weights).CompareTo(ScoreArtifact(a, weights)));
    }
}
