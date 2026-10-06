using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class HuntListServiceTests
{
    private static HuntItem ValidItem() => new()
    {
        Id = "item-1",
        Slot = 3,
        MainStat = Stat.PyroDMG,
        Substats =
        [
            new HuntSubstat { Stat = Stat.CritDMG, MinRolls = 4 },
            new HuntSubstat { Stat = Stat.CritRate, MinRolls = 3 },
        ],
    };

    [Fact]
    public void Add_AcceptsValidItem()
    {
        var service = new HuntListService();

        Assert.True(service.Add(ValidItem()));
        Assert.Single(service.Items);
    }

    [Fact]
    public void Add_RejectsTooManySubstats()
    {
        var service = new HuntListService();
        var item = ValidItem();
        item.Substats =
        [
            new HuntSubstat { Stat = Stat.CritDMG, MinRolls = 1 },
            new HuntSubstat { Stat = Stat.CritRate, MinRolls = 1 },
            new HuntSubstat { Stat = Stat.AtkPercent, MinRolls = 1 },
            new HuntSubstat { Stat = Stat.EnergyRecharge, MinRolls = 1 },
            new HuntSubstat { Stat = Stat.ElementalMastery, MinRolls = 1 },
        ];

        Assert.False(service.Add(item));
        Assert.Empty(service.Items);
    }

    [Fact]
    public void Add_RejectsImpossibleRollTotal()
    {
        // 2 substats can hold at most 5 + 2 = 7 rolls.
        var service = new HuntListService();
        var item = ValidItem();
        item.Substats =
        [
            new HuntSubstat { Stat = Stat.CritDMG, MinRolls = 6 },
            new HuntSubstat { Stat = Stat.CritRate, MinRolls = 6 },
        ];

        Assert.False(service.Add(item));
        Assert.Empty(service.Items);
    }

    [Fact]
    public void Remove_DeletesById()
    {
        var service = new HuntListService();
        service.Add(ValidItem());

        Assert.True(service.Remove("item-1"));
        Assert.Empty(service.Items);
        Assert.False(service.Remove("missing"));
    }

    [Fact]
    public void Update_ReplacesMatchingItem()
    {
        var service = new HuntListService();
        service.Add(ValidItem());
        var updated = ValidItem();
        updated.Slot = 2;

        Assert.True(service.Update("item-1", updated));
        Assert.Equal(2, service.Items[0].Slot);
    }

    [Fact]
    public void Json_RoundTripsItems()
    {
        var service = new HuntListService();
        service.Add(ValidItem());

        var json = service.ToJson();
        var restored = new HuntListService();
        restored.LoadFromJson(json);

        Assert.Single(restored.Items);
        Assert.Equal("item-1", restored.Items[0].Id);
        Assert.Equal(2, restored.Items[0].Substats.Count);
    }

    [Fact]
    public void Deserialize_ReturnsEmptyOnInvalidJson()
    {
        Assert.Empty(HuntListService.Deserialize(null));
        Assert.Empty(HuntListService.Deserialize("not-json{"));
    }
}
