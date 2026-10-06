using System.Text.Json;
using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// In-memory hunt list with validation and JSON persistence helpers.
/// Persistence itself (localStorage) is done by the UI via JS interop;
/// this service stays pure and unit-testable.
/// Ported from React <c>useHuntList</c> + <c>HuntListItem</c> validation rules.
/// </summary>
public class HuntListService
{
    public const string StorageKey = "hunt-list";
    public const int MaxSubstats = 4;
    public const int MaxRollsPerStat = 6;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public List<HuntItem> Items { get; private set; } = [];

    public event Action? OnChange;

    public bool Add(HuntItem item)
    {
        if (!Validate(item, out _))
            return false;
        if (string.IsNullOrWhiteSpace(item.Id))
            item.Id = Guid.NewGuid().ToString();
        Items.Add(item);
        OnChange?.Invoke();
        return true;
    }

    public bool Remove(string id)
    {
        int removed = Items.RemoveAll(i => i.Id == id);
        if (removed > 0)
            OnChange?.Invoke();
        return removed > 0;
    }

    public bool Update(string id, HuntItem item)
    {
        if (!Validate(item, out _))
            return false;
        int idx = Items.FindIndex(i => i.Id == id);
        if (idx < 0)
            return false;
        item.Id = id;
        Items[idx] = item;
        OnChange?.Invoke();
        return true;
    }

    public void Clear()
    {
        Items.Clear();
        OnChange?.Invoke();
    }

    public void LoadFromJson(string? json)
    {
        Items = Deserialize(json);
        OnChange?.Invoke();
    }

    public string ToJson() => JsonSerializer.Serialize(Items, JsonOptions);

    public static List<HuntItem> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];
        try
        {
            return JsonSerializer.Deserialize<List<HuntItem>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Short-term guard for the React P0 bug: a set without a domain can never
    /// match because no set is assigned when Domain is "Any".
    /// Callers should require a domain whenever a set is picked.
    /// </summary>
    public static bool ValidateDomainSet(string? domainId, string? setId, out string error)
    {
        if (!string.IsNullOrEmpty(setId) && string.IsNullOrEmpty(domainId))
        {
            error = "Pick a domain when a set is selected.";
            return false;
        }
        if (!string.IsNullOrEmpty(setId) && !string.IsNullOrEmpty(domainId)
            && !GameData.SetBelongsToDomain(setId, domainId))
        {
            error = "Set does not belong to the selected domain.";
            return false;
        }
        error = "";
        return true;
    }

    /// <summary>
    /// Mirrors React modal rules: slot 0-4, mainStat required, ≤4 substats,
    /// each 1-6 rolls, total rolls ≤ 5 + count (a +20 artifact has 5 upgrades + initial lines).
    /// </summary>
    public static bool Validate(HuntItem item, out string error)
    {
        if (item.Slot < 0 || item.Slot > 4)
        {
            error = "Slot must be 0-4.";
            return false;
        }
        if (item.MainStat < 0)
        {
            error = "Main stat is required.";
            return false;
        }
        if (item.Substats.Count > MaxSubstats)
        {
            error = "At most 4 desired substats.";
            return false;
        }
        if (item.Substats.GroupBy(s => s.Stat).Any(g => g.Count() > 1))
        {
            error = "Duplicate substats.";
            return false;
        }
        if (item.Substats.Any(s => s.MinRolls < 1 || s.MinRolls > MaxRollsPerStat))
        {
            error = "Each substat needs 1-6 rolls.";
            return false;
        }
        int total = item.Substats.Sum(s => s.MinRolls);
        if (total > 5 + item.Substats.Count)
        {
            error = $"Total rolls ({total}) exceed achievable {5 + item.Substats.Count}.";
            return false;
        }
        return ValidateDomainSet(item.DomainId, item.SetId, out error);
    }
}
