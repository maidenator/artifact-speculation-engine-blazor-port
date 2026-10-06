using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Pure hunt-item matching. Ported from React <c>artifactMeetsHuntItem</c>
/// (slot + main + every required substat present at avg×minRolls, 5% tolerance).
/// Set filtering is handled by <see cref="HuntRunner"/> at assignment time —
/// a set is only ever checked when its domain assigned it (single 50/50 coin,
/// never doubled, and set-without-domain items are rejected by validation).
/// </summary>
public static class HuntMatcher
{
    public const double Tolerance = 0.95;

    public static bool MatchesArtifact(ArtifactOutput art, HuntItem item)
    {
        if (art.Slot != item.Slot)
            return false;
        if (art.MainStat.Type != item.MainStat)
            return false;

        foreach (var req in item.Substats)
        {
            var sub = art.SubStats.Find(s => s.Type == req.Stat);
            if (sub is null)
                return false;
            if (!ArtifactData.AvgSubstatRoll.TryGetValue(req.Stat, out double avg))
                return false;
            if (sub.Value < avg * req.MinRolls * Tolerance)
                return false;
        }

        return true;
    }
}
