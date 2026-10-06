using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// C# service that wraps the JavaScript bridge to the C++ WASM artifact engine.
/// All calls go through IJSRuntime → wasmBridge.js → Emscripten → C++.
/// </summary>
public class ArtifactEngineService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private bool _initialized;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public bool IsReady => _initialized;

    public ArtifactEngineService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Initialize the WASM engine with the given seed.
    /// </summary>
    public async Task InitAsync(long seed = 1337)
    {
        await _js.InvokeVoidAsync("artifactEngine.init", seed);
        _initialized = true;
    }

    /// <summary>
    /// Set the RNG seed for the engine.
    /// </summary>
    public async Task SetSeedAsync(long seed)
    {
        await _js.InvokeVoidAsync("artifactEngine.setSeed", seed);
    }

    /// <summary>
    /// Generate a batch of artifacts.
    /// </summary>
    public async Task<List<ArtifactOutput>> GenerateBatchAsync(int count, bool upgrade = true)
    {
        var json = await _js.InvokeAsync<string>("artifactEngine.generateBatch", count, upgrade);
        return JsonSerializer.Deserialize<List<ArtifactOutput>>(json, JsonOptions) ?? [];
    }

    /// <summary>
    /// Generate a batch of artifacts with full upgrade history.
    /// </summary>
    public async Task<List<List<ArtifactOutput>>> GenerateBatchHistoryAsync(int count)
    {
        var json = await _js.InvokeAsync<string>("artifactEngine.generateBatchHistory", count);
        return JsonSerializer.Deserialize<List<List<ArtifactOutput>>>(json, JsonOptions) ?? [];
    }

    /// <summary>
    /// Run a single full-budget sample (ResinBudget mode). One draw only —
    /// the UI labels it as such; use Target trials for distributions.
    /// </summary>
    public Task<SimulationResult> RunSimulationAsync(SimulationSettings settings)
    {
        var configJson = BuildSimulationConfig(settings);
        return RunOneAsync(configJson, settings, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    /// <summary>
    /// Run N independent full-budget Target trials (review §1 HIGH) and
    /// aggregate resin-to-goal into a success rate plus p50/p90 percentiles.
    /// Each trial gets a distinct seed; cancellation is checked between trials.
    /// </summary>
    public async Task<TargetTrialResult> RunTargetTrialsAsync(
        SimulationSettings settings, int trialCount, CancellationToken ct = default)
    {
        var configJson = BuildSimulationConfig(settings);
        long seedBase = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var trials = new List<(double? Resin, SimulationResult Result)>(trialCount);
        for (int i = 0; i < trialCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var result = await RunOneAsync(configJson, settings, seedBase + i);
            trials.Add((result.TargetAchieved ? (double?)result.TotalResinSpent : null, result));
        }
        sw.Stop();

        var distribution = TargetStats.Aggregate(trials.Select(t => t.Resin).ToList());

        SimulationResult? median = null;
        if (distribution.Successes > 0)
        {
            var wins = trials.Where(t => t.Resin.HasValue).OrderBy(t => t.Resin!.Value).ToList();
            median = wins[Math.Min(wins.Count - 1, wins.Count / 2)].Result;
        }

        return new TargetTrialResult { Distribution = distribution, MedianResult = median, ElapsedMs = sw.Elapsed.TotalMilliseconds };
    }

    private async Task<SimulationResult> RunOneAsync(string configJson, SimulationSettings settings, long seed)
    {
        await SetSeedAsync(seed);
        var resultJson = await _js.InvokeAsync<string>("artifactEngine.runSimulation", configJson);
        var result = JsonSerializer.Deserialize<SimulationResult>(resultJson, JsonOptions) ?? new();

        var weights = ScoringService.WeightsFromPriority(settings.Priority ?? []);
        ScoringService.SortByScore(result.TopArtifacts, weights);
        if (result.TopArtifacts.Count > settings.TopK)
            result.TopArtifacts = result.TopArtifacts.GetRange(0, settings.TopK);

        return result;
    }

    public static string BuildSimulationConfig(SimulationSettings settings)
    {
        var weights = ScoringService.WeightsFromPriority(settings.Priority ?? []);
        var config = new Dictionary<string, object>
        {
            ["mode"] = (int)settings.Mode,
            ["resinBudget"] = settings.ResinBudget,
            ["topK"] = settings.TopK,
            ["useStrongBox"] = settings.UseStrongBox,
            ["minCritValue"] = settings.MinCritValue,
            ["substatWeights"] = weights
        };

        if (settings.Mode == SimulationMode.TargetPiece)
        {
            if (settings.TargetSlot.HasValue)
                config["targetSlot"] = settings.TargetSlot.Value;
            if (settings.TargetMainStat.HasValue)
                config["targetMainStat"] = settings.TargetMainStat.Value;
        }

        return JsonSerializer.Serialize(config);
    }

    public async ValueTask DisposeAsync()
    {
        if (_initialized)
        {
            try
            {
                await _js.InvokeVoidAsync("artifactEngine.dispose");
            }
            catch
            {
                // Silently handle disposal errors during page teardown
            }
        }
    }
}
