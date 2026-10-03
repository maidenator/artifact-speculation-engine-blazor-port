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
    /// Run a full simulation with the given settings.
    /// </summary>
    public async Task<SimulationResult> RunSimulationAsync(SimulationSettings settings)
    {
        var configJson = BuildSimulationConfig(settings);
        await SetSeedAsync(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        var resultJson = await _js.InvokeAsync<string>("artifactEngine.runSimulation", configJson);
        return JsonSerializer.Deserialize<SimulationResult>(resultJson, JsonOptions) ?? new();
    }

    private static string BuildSimulationConfig(SimulationSettings settings)
    {
        var config = new Dictionary<string, object>
        {
            ["mode"] = (int)settings.Mode,
            ["resinBudget"] = settings.ResinBudget,
            ["topK"] = settings.TopK,
            ["useStrongBox"] = settings.UseStrongBox,
            ["minCritValue"] = settings.MinCritValue,
            ["substatWeights"] = Array.Empty<object>()
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
