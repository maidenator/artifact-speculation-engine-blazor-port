using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class GameDataTests
{
    [Fact]
    public void Domains_EachHasTwoSets()
    {
        Assert.NotEmpty(GameData.Domains);
        foreach (var domain in GameData.Domains)
            Assert.Equal(2, domain.Sets.Length);
    }

    [Fact]
    public void MainStatsForSlot_MatchesOriginal()
    {
        Assert.Equal([Stat.FlatHp], GameData.MainStatsForSlot(0));
        Assert.Equal([Stat.FlatAtk], GameData.MainStatsForSlot(1));
        Assert.Contains(Stat.EnergyRecharge, GameData.MainStatsForSlot(2));
        Assert.Contains(Stat.PyroDMG, GameData.MainStatsForSlot(3));
        Assert.Contains(Stat.CritDMG, GameData.MainStatsForSlot(4));
    }

    [Fact]
    public void ValidateDomainSet_RejectsSetWithoutDomain()
    {
        Assert.False(HuntListService.ValidateDomainSet(null, "thundering_fury", out string error));
        Assert.NotEmpty(error);
        Assert.True(HuntListService.ValidateDomainSet("midsummer_courtyard", "thundering_fury", out _));
    }

    [Fact]
    public void ValidateDomainSet_RejectsForeignSet()
    {
        Assert.False(HuntListService.ValidateDomainSet("ridge_watch", "thundering_fury", out _));
    }

    [Fact]
    public void HuntItem_WithSetButNoDomain_IsInvalid()
    {
        var item = new HuntItem
        {
            Id = "x",
            Slot = 3,
            MainStat = Stat.PyroDMG,
            Substats = [new HuntSubstat { Stat = Stat.CritDMG, MinRolls = 1 }],
            DomainId = null,
            SetId = "thundering_fury",
        };

        Assert.False(HuntListService.Validate(item, out _));
    }
}
