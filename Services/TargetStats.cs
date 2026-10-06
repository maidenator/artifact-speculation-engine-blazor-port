namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Multi-trial statistics for Target mode (review §1 HIGH: "report a
/// distribution, not one draw"). Each trial is an independent full-budget
/// run; resin-to-goal samples aggregate to a success rate plus percentiles.
/// Percentile rule matches the review: sorted[floor(p*n)], capped at last.
/// </summary>
public static class TargetStats
{
    public record TargetDistribution(
        int Attempts,
        int Successes,
        double SuccessRate,
        double? P50Resin,
        double? P90Resin);

    /// <param name="resinPerTrial">Resin spent per trial; null = goal not reached.</param>
    public static TargetDistribution Aggregate(IReadOnlyList<double?> resinPerTrial)
    {
        var wins = resinPerTrial
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .OrderBy(r => r)
            .ToList();

        if (wins.Count == 0)
            return new TargetDistribution(resinPerTrial.Count, 0, 0, null, null);

        return new TargetDistribution(
            resinPerTrial.Count,
            wins.Count,
            (double)wins.Count / resinPerTrial.Count,
            Percentile(wins, 0.5),
            Percentile(wins, 0.9));
    }

    private static double Percentile(List<double> sorted, double p) =>
        sorted[Math.Min(sorted.Count - 1, (int)Math.Floor(p * sorted.Count))];
}
