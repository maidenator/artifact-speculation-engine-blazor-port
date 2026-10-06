using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Tests;

public class StatIconTests
{
    [Fact]
    public void EverySubstatHasAnIcon()
    {
        foreach (var id in ArtifactData.SubstatIds)
            Assert.NotNull(ArtifactData.GetStatIcon(id));
    }

    [Fact]
    public void EveryMainStatHasAnIcon()
    {
        foreach (var id in ArtifactData.MainStatNames.Keys)
            Assert.NotNull(ArtifactData.GetStatIcon(id));
    }

    [Fact]
    public void PercentAndFlatVariantsDoNotCollide()
    {
        // These pairs broke under the old name-contains matching.
        Assert.Equal("icons/stat/hp_percent.png", ArtifactData.GetStatIcon(Stat.HpPercent));
        Assert.Equal("icons/stat/hp.png", ArtifactData.GetStatIcon(Stat.FlatHp));
        Assert.Equal("icons/stat/attack_percent.png", ArtifactData.GetStatIcon(Stat.AtkPercent));
        Assert.Equal("icons/stat/attack.png", ArtifactData.GetStatIcon(Stat.FlatAtk));
    }

    [Fact]
    public void ElementsMapToElementIcons()
    {
        Assert.Equal("icons/element/pyro.png", ArtifactData.GetStatIcon(Stat.PyroDMG));
        Assert.Equal("icons/element/physical.png", ArtifactData.GetStatIcon(Stat.PhysicalDMG));
        Assert.Equal("icons/stat/healing_bonus.png", ArtifactData.GetStatIcon(Stat.HealingBonus));
    }

    [Fact]
    public void UnknownStatHasNoIcon()
    {
        Assert.Null(ArtifactData.GetStatIcon(999));
    }
}
