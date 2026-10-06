using System.Text.Json;
using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class GoodExportTests
{
    [Fact]
    public void MapStat_CoversAllEngineStats()
    {
        Assert.Equal("critDMG_", GoodExportService.MapStat(Stat.CritDMG));
        Assert.Equal("pyro_dmg_", GoodExportService.MapStat(Stat.PyroDMG));
        Assert.Equal("hp", GoodExportService.MapStat(999));
    }

    [Fact]
    public void MapSlot_CoversAllSlots()
    {
        Assert.Equal("flower", GoodExportService.MapSlot(0));
        Assert.Equal("plume", GoodExportService.MapSlot(1));
        Assert.Equal("circlet", GoodExportService.MapSlot(4));
    }

    [Fact]
    public void MapSet_UsesKnownKeysAndDefaults()
    {
        Assert.Equal("ThunderingFury", GoodExportService.MapSet("thundering_fury"));
        Assert.Equal("NighttimeWhispersInTheEchoingWoods", GoodExportService.MapSet("nighttime_whispers"));
        Assert.Equal("GladiatorsFinale", GoodExportService.MapSet(null));
    }

    [Fact]
    public void ToGoodDocument_MapsSetIdFromArtifact()
    {
        var art = new ArtifactOutput
        {
            Slot = 3,
            Level = 20,
            SetId = "emblem_of_severed_fate",
            MainStat = new MainStatEntry { Type = Stat.EnergyRecharge, Value = 51.8 },
            SubStats = [new SubstatEntry { Type = Stat.CritDMG, Value = 21.0, Rolls = 3 }],
        };

        var doc = GoodExportService.ToGoodDocument([art]);
        var exported = doc.Artifacts[0];

        Assert.Equal("GOOD", doc.Format);
        Assert.Equal("EmblemOfSeveredFate", exported.SetKey);
        Assert.Equal("goblet", exported.SlotKey);
        Assert.Equal("enerRech_", exported.MainStatKey);
        Assert.Equal(5, exported.Rarity);
        Assert.Single(exported.Substats);
    }
}
