using Microsoft.AspNetCore.Components;
using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Components;

public partial class HuntModal : ComponentBase
{
    [Parameter] public EventCallback<HuntItem> OnAdd { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private int _selectedSlot; // default Flower
    private int? _selectedMainStat = Stat.FlatHp;
    private Dictionary<int, int> _substatRolls = new();

    private string? _domainId;
    private string? _setId;
    private string? _error;

    private int[] MainOptions => GameData.MainStatsForSlot(_selectedSlot);
    private List<ArtifactSet> AvailableSets => GameData.AvailableSets(_domainId);
    private int TotalRolls => _substatRolls.Values.Sum();
    private int MaxTotalRolls => 5 + _substatRolls.Count;

    private void SelectSlot(int slot)
    {
        _selectedSlot = slot;
        var options = GameData.MainStatsForSlot(slot);
        _selectedMainStat = options.Length > 0 ? options[0] : null;

        // Flower/Feather fixed mains can't appear as substats; drop conflicts.
        if (_selectedMainStat.HasValue)
            _substatRolls.Remove(_selectedMainStat.Value);
        _error = null;
    }

    private void SelectMainStat(int stat)
    {
        _selectedMainStat = stat;
        _substatRolls.Remove(stat);
        _error = null;
    }

    private void ToggleSubstat(int stat)
    {
        if (_substatRolls.ContainsKey(stat))
        {
            _substatRolls.Remove(stat);
        }
        else if (_substatRolls.Count < HuntListService.MaxSubstats && _selectedMainStat != stat)
        {
            _substatRolls[stat] = 1;
        }
        _error = null;
    }

    private void ChangeRolls(int stat, int delta)
    {
        if (!_substatRolls.TryGetValue(stat, out int current))
            return;

        int next = current + delta;
        if (next < 1 || next > HuntListService.MaxRollsPerStat)
            return;
        if (delta > 0 && TotalRolls >= MaxTotalRolls)
            return;

        _substatRolls[stat] = next;
        _error = null;
    }

    private void SelectDomain(string? domainId)
    {
        _domainId = string.IsNullOrEmpty(domainId) ? null : domainId;
        // Changing domain resets a set from another domain.
        if (!string.IsNullOrEmpty(_setId) && !string.IsNullOrEmpty(_domainId)
            && !GameData.SetBelongsToDomain(_setId, _domainId))
            _setId = null;
        _error = null;
    }

    private async Task AddToHuntList()
    {
        if (!_selectedMainStat.HasValue)
        {
            _error = "Pick a main stat.";
            return;
        }

        var item = new HuntItem
        {
            Id = Guid.NewGuid().ToString(),
            Slot = _selectedSlot,
            MainStat = _selectedMainStat.Value,
            Substats = _substatRolls.Select(kv => new HuntSubstat { Stat = kv.Key, MinRolls = kv.Value }).ToList(),
            DomainId = _domainId,
            SetId = _setId,
        };

        if (!HuntListService.Validate(item, out string error))
        {
            _error = error;
            return;
        }

        await OnAdd.InvokeAsync(item);
    }
}
