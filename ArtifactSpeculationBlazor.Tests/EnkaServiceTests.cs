using System.Text.Json;
using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class EnkaServiceTests
{
    private const string Fixture = """
        {
          "playerInfo": {
            "nickname": "Traveler",
            "level": 60,
            "worldLevel": 8,
            "signature": "hi",
            "towerFloorIndex": 12,
            "towerLevelIndex": 3,
            "finishAchievementNum": 500
          },
          "avatarInfoList": [
            {
              "avatarId": 10000002,
              "propMap": { "4001": { "val": "90" } },
              "talentIdList": [1, 2, 3],
              "equipList": [
                {
                  "flat": { "itemType": "ITEM_WEAPON", "icon": "UI_EquipIcon_Sword_Ayaka" },
                  "weapon": { "level": 90, "affixMap": { "11441": 4 } }
                },
                {
                  "reliquary": { "level": 21 },
                  "flat": {
                    "itemType": "ITEM_RELIQUARY",
                    "rankLevel": 5,
                    "equipType": "EQUIP_BRACER",
                    "icon": "UI_RelicIcon_15019_4",
                    "reliquaryMainstat": { "mainPropId": "FIGHT_PROP_HP", "statValue": 4780 },
                    "reliquarySubstats": [
                      { "appendPropId": "FIGHT_PROP_CRITICAL_HURT", "statValue": 21.0 },
                      { "appendPropId": "FIGHT_PROP_CRITICAL", "statValue": 10.5 },
                      { "appendPropId": "FIGHT_PROP_UNKNOWN", "statValue": 99 }
                    ]
                  }
                },
                {
                  "reliquary": { "level": 20 },
                  "flat": {
                    "itemType": "ITEM_RELIQUARY",
                    "rankLevel": 4,
                    "equipType": "EQUIP_NECKLACE",
                    "icon": "UI_RelicIcon_15019_2",
                    "reliquaryMainstat": { "mainPropId": "FIGHT_PROP_ATTACK", "statValue": 311 },
                    "reliquarySubstats": []
                  }
                }
              ]
            }
          ]
        }
        """;

    private static readonly Dictionary<string, EnkaService.CharacterInfo> Map = new()
    {
        ["10000002"] = new() { Name = "Kamisato Ayaka", Icon = "UI_AvatarIcon_Ayaka" },
    };

    [Theory]
    [InlineData("123456789", true)]
    [InlineData("1234567890", true)]
    [InlineData("123", false)]
    [InlineData("12345678901", false)]
    [InlineData("abcdefghi", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidUid_EnforcesNineToTenDigits(string? uid, bool expected)
    {
        Assert.Equal(expected, EnkaService.IsValidUid(uid));
    }

    [Fact]
    public void Parse_ReturnsNullWithoutPlayerInfo()
    {
        using var doc = JsonDocument.Parse("{}");
        Assert.Null(EnkaService.Parse(doc.RootElement, Map));
    }

    [Fact]
    public void Parse_MapsProfileCharacterWeaponAndRelics()
    {
        using var doc = JsonDocument.Parse(Fixture);
        var parsed = EnkaService.Parse(doc.RootElement, Map);

        Assert.NotNull(parsed);
        var (profile, characters) = parsed.Value;
        Assert.Equal("Traveler", profile.Nickname);
        Assert.Equal(60, profile.Level);
        Assert.Equal(8, profile.WorldLevel);
        Assert.Equal(12, profile.AbyssFloor);

        var ayaka = Assert.Single(characters);
        Assert.Equal("Kamisato Ayaka", ayaka.Name);
        Assert.Equal(90, ayaka.Level);
        Assert.Equal(3, ayaka.Constellation);
        Assert.Contains("UI_AvatarIcon_Ayaka", ayaka.IconUrl);
        Assert.Equal(90, ayaka.Weapon.Level);
        Assert.Equal(5, ayaka.Weapon.Refinement);

        // Only the 5-star relic is parsed; the 4-star is skipped.
        var flower = Assert.Single(ayaka.Artifacts);
        Assert.Equal(0, flower.Slot);
        Assert.Equal(20, flower.Level);
        Assert.Equal(Stat.FlatHp, flower.MainStat.Type);
        Assert.Equal(4780, flower.MainStat.Value);
        // Unknown substat skipped; CV = 21.0 + 10.5 * 2.
        Assert.Equal(2, flower.SubStats.Count);
        Assert.Equal(42.0, flower.CritValue, precision: 10);
    }

    [Fact]
    public void Parse_HandlesTravelerAndMissingMap()
    {
        using var doc = JsonDocument.Parse("""{"playerInfo": {"nickname": "x"}, "avatarInfoList": [{"avatarId": 10000005, "equipList": []}]}""");
        var parsed = EnkaService.Parse(doc.RootElement, new());

        Assert.NotNull(parsed);
        Assert.Equal("Aether", parsed.Value.Characters[0].Name);
    }
}
