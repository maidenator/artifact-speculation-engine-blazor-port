using System.Diagnostics;
using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Correct hunt execution that avoids the four React P0 bugs:
/// <list type="bullet">
/// <item>checks <b>every</b> generated artifact (no top-K-20 reservoir clamp),</item>
/// <item>assigns the set with a <b>single</b> 50/50 coin per artifact (never doubled),</item>
/// <item>rejects set-without-domain items at validation time (they can never match otherwise),</item>
/// <item>records resin at the <b>exact run</b> of the find (fixed-size runs, no doubling chunks).</item>
/// </list>
/// <para>
/// Resin model mirrors the C++ engine: 20 resin per domain run, 6.5% chance of
/// a double (2-piece) drop. Strongbox is not modeled in hunts (v1 limitation,
/// shown in the UI) — hunt artifacts are examined as they drop.
/// </para>
/// </summary>
public class HuntRunner
{
    public const int ResinPerRun = 20;
    public const double DoubleDropChance = 0.065;
    public const double ResinPerDay = 180.0;
    public const int ChunkArtifacts = 200;

    private readonly Func<int, Task<List<ArtifactOutput>>> _generateBatch;
    private readonly Func<double> _drawDouble;

    /// <param name="generateBatch">Artifact source (WASM in prod, fakes in tests).</param>
    /// <param name="drawDouble">Returns [0,1) to decide double drops (injectable for tests).</param>
    public HuntRunner(Func<int, Task<List<ArtifactOutput>>> generateBatch, Func<double>? drawDouble = null)
    {
        _generateBatch = generateBatch;
        _drawDouble = drawDouble ?? Random.Shared.NextDouble;
    }

    public async Task<HuntListResult> RunHuntAsync(
        IReadOnlyList<HuntItem> huntItems,
        int maxBudget,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var itemResults = huntItems.Select(i => new HuntItemResult { HuntItemId = i.Id }).ToList();

        int totalResin = 0;
        int totalRuns = 0;
        var queue = new Queue<ArtifactOutput>();

        // Farm each domain group sequentially (you farm one domain at a time).
        // Null/empty domain = "Any" group: no set is ever assigned, so only
        // items without SetId (enforced by validation) can match there.
        var groups = huntItems
            .Select((item, index) => (item, index))
            .GroupBy(p => p.item.DomainId ?? "")
            .ToList();

        foreach (var group in groups)
        {
            string? domainId = string.IsNullOrEmpty(group.Key) ? null : group.Key;
            var domain = domainId is null ? null : GameData.Domains.FirstOrDefault(d => d.Id == domainId);
            var pending = group.Where(p => !itemResults[p.index].Found).ToList();

            while (totalResin < maxBudget && pending.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                if (totalResin + ResinPerRun > maxBudget)
                    break;

                totalResin += ResinPerRun;
                totalRuns++;
                int drops = _drawDouble() < DoubleDropChance ? 2 : 1;

                for (int d = 0; d < drops; d++)
                {
                    if (queue.Count == 0)
                    {
                        var batch = await _generateBatch(ChunkArtifacts);
                        foreach (var fetched in batch)
                            queue.Enqueue(fetched);
                        if (queue.Count == 0)
                            break;
                    }

                    var art = queue.Dequeue();

                    // Single set coin: only when farming a concrete domain.
                    if (domain is not null)
                        art.SetId = _drawDouble() < 0.5 ? domain.Sets[0].Id : domain.Sets[1].Id;

                    foreach (var (item, index) in pending.ToList())
                    {
                        if (!string.IsNullOrEmpty(item.SetId) && art.SetId != item.SetId)
                            continue;
                        if (HuntMatcher.MatchesArtifact(art, item))
                        {
                            itemResults[index].Found = true;
                            itemResults[index].ResinSpent = totalResin;
                            itemResults[index].DaysSpent = totalResin / ResinPerDay;
                            itemResults[index].Artifact = art;
                            pending.RemoveAll(p => p.index == index);
                            break; // one artifact fulfills one item
                        }
                    }
                    if (pending.Count == 0)
                        break;
                }
            }
        }

        sw.Stop();
        return new HuntListResult
        {
            AllFound = itemResults.All(r => r.Found),
            TotalResinSpent = totalResin,
            TotalDays = totalResin / ResinPerDay,
            CondensedResin = (int)Math.Ceiling(totalResin / 40.0),
            DomainRunsCompleted = totalRuns,
            StrongboxRollsCompleted = 0,
            ItemResults = itemResults,
            ElapsedMs = sw.Elapsed.TotalMilliseconds,
        };
    }
}
