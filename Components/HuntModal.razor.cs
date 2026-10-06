using Microsoft.AspNetCore.Components;
using ArtifactSpeculationBlazor.Models;
using System.Collections.Generic;
using System.Linq;

namespace ArtifactSpeculationBlazor.Components;

public partial class HuntModal : ComponentBase
{
    [Parameter] public EventCallback<HuntItem> OnAdd { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private int _selectedSlot = 0; // default Flower
    private int? _selectedMainStat = Stat.FlatHp;
    private HashSet<int> _selectedSubstats = new();

    private string _domain = "Any";
    private string _set = "Any";

    private void SelectSlot(int slot)
    {
        _selectedSlot = slot;
        // Default main stats
        if (slot == 0) _selectedMainStat = Stat.FlatHp;
        else if (slot == 1) _selectedMainStat = Stat.FlatAtk;
        else _selectedMainStat = null;
        
        // Remove main stat from selected substats if it's there
        if (_selectedMainStat.HasValue && _selectedSubstats.Contains(_selectedMainStat.Value))
        {
            _selectedSubstats.Remove(_selectedMainStat.Value);
        }
    }

    private void ToggleSubstat(int stat)
    {
        if (_selectedSubstats.Contains(stat))
        {
            _selectedSubstats.Remove(stat);
        }
        else
        {
            if (_selectedSubstats.Count < 4 && _selectedMainStat != stat)
            {
                _selectedSubstats.Add(stat);
            }
        }
    }

    private async Task AddToHuntList()
    {
        var item = new HuntItem
        {
            Id = Guid.NewGuid().ToString(),
            Slot = _selectedSlot,
            MainStat = _selectedMainStat ?? Stat.FlatHp,
            Substats = DesiredSubstatsToSubstats(),
            DomainId = _domain == "Any" ? null : _domain,
            SetId = _set == "Any" ? null : _set,
        };
        await OnAdd.InvokeAsync(item);
    }

    private List<HuntSubstat> DesiredSubstatsToSubstats() =>
        _selectedSubstats.Select(stat => new HuntSubstat { Stat = stat, MinRolls = 1 }).ToList();
}
