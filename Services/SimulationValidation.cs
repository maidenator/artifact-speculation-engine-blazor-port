using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Input validation at the UI/engine boundary.
/// Mirrors the C++-side clamping recommended in SUGGESTIONS (P0-2/P2-8):
/// resin is always a multiple of 20 within a sane range, top-K and min-CV
/// can't produce degenerate runs.
/// </summary>
public static class SimulationValidation
{
    public const int MinResin = 20;
    public const int MaxResin = 10_000_000;
    public const int ResinStep = 20;
    public const int MinTopK = 1;
    public const int MaxTopK = 100;

    public static int ClampResinBudget(int resin)
    {
        int clamped = Math.Clamp(resin, MinResin, MaxResin);
        return (clamped / ResinStep) * ResinStep;
    }

    public static int ClampTopK(int topK) => Math.Clamp(topK, MinTopK, MaxTopK);

    public static double ClampMinCritValue(double cv) => Math.Max(0, cv);

    public static void Normalize(SimulationSettings settings)
    {
        settings.ResinBudget = ClampResinBudget(settings.ResinBudget);
        settings.TopK = ClampTopK(settings.TopK);
        settings.MinCritValue = ClampMinCritValue(settings.MinCritValue);
        settings.Priority ??= [];
        // An impossible slot+main pair can never match — drop the main stat.
        if (settings.TargetSlot.HasValue && settings.TargetMainStat.HasValue
            && !GameData.MainStatsForSlot(settings.TargetSlot.Value).Contains(settings.TargetMainStat.Value))
            settings.TargetMainStat = null;
    }
}
