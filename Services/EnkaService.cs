using System.Text.Json;
using System.Text.RegularExpressions;
using ArtifactSpeculationBlazor.Models;

namespace ArtifactSpeculationBlazor.Services;

/// <summary>
/// Enka.Network import ported from React <c>src/utils/enka.ts</c> +
/// <c>UIDImport.tsx</c>. Calls Enka directly (it serves CORS headers) —
/// no third-party proxy. Only 5-star reliquaries are parsed.
/// </summary>
public static partial class EnkaService
{
    public const string ApiBase = "https://enka.network/api/uid/";
    public const string ImageBase = "https://gi.yatta.moe/assets/UI/";

    [GeneratedRegex(@"^\d{9,10}$")]
    private static partial Regex UidRegex();

    public static bool IsValidUid(string? uid) =>
        !string.IsNullOrWhiteSpace(uid) && UidRegex().IsMatch(uid.Trim());

    private static readonly Dictionary<string, int> StatMap = new()
    {
        ["FIGHT_PROP_HP"] = Stat.FlatHp,
        ["FIGHT_PROP_HP_PERCENT"] = Stat.HpPercent,
        ["FIGHT_PROP_ATTACK"] = Stat.FlatAtk,
        ["FIGHT_PROP_ATTACK_PERCENT"] = Stat.AtkPercent,
        ["FIGHT_PROP_DEFENSE"] = Stat.FlatDef,
        ["FIGHT_PROP_DEFENSE_PERCENT"] = Stat.DefPercent,
        ["FIGHT_PROP_CRITICAL"] = Stat.CritRate,
        ["FIGHT_PROP_CRITICAL_HURT"] = Stat.CritDMG,
        ["FIGHT_PROP_CHARGE_EFFICIENCY"] = Stat.EnergyRecharge,
        ["FIGHT_PROP_ELEMENT_MASTERY"] = Stat.ElementalMastery,
        ["FIGHT_PROP_HEAL_ADD"] = Stat.HealingBonus,
        ["FIGHT_PROP_PHYSICAL_ADD_HURT"] = Stat.PhysicalDMG,
        ["FIGHT_PROP_FIRE_ADD_HURT"] = Stat.PyroDMG,
        ["FIGHT_PROP_ELEC_ADD_HURT"] = Stat.ElectroDMG,
        ["FIGHT_PROP_WATER_ADD_HURT"] = Stat.HydroDMG,
        ["FIGHT_PROP_WIND_ADD_HURT"] = Stat.AnemoDMG,
        ["FIGHT_PROP_ICE_ADD_HURT"] = Stat.CryoDMG,
        ["FIGHT_PROP_ROCK_ADD_HURT"] = Stat.GeoDMG,
        ["FIGHT_PROP_GRASS_ADD_HURT"] = Stat.DendroDMG,
    };

    private static readonly Dictionary<string, int> SlotMap = new()
    {
        ["EQUIP_BRACER"] = 0,
        ["EQUIP_NECKLACE"] = 1,
        ["EQUIP_SHOES"] = 2,
        ["EQUIP_RING"] = 3,
        ["EQUIP_DRESS"] = 4,
    };

    /// <summary>
    /// Pure parser: Enka JSON + character map → profile and characters.
    /// Returns null when the payload has no player info.
    /// </summary>
    public static (EnkaProfile Profile, List<EnkaCharacter> Characters)? Parse(
        JsonElement data,
        Dictionary<string, CharacterInfo> characterMap)
    {
        if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("playerInfo", out var player))
            return null;

        var profile = new EnkaProfile
        {
            Nickname = GetString(player, "nickname") ?? "Unknown",
            Level = GetInt(player, "level", 1),
            WorldLevel = GetIntOrNull(player, "worldLevel"),
            Signature = GetString(player, "signature"),
            AbyssFloor = GetIntOrNull(player, "towerFloorIndex"),
            AbyssChamber = GetIntOrNull(player, "towerLevelIndex"),
            AchievementCount = GetIntOrNull(player, "finishAchievementNum"),
        };

        var characters = new List<EnkaCharacter>();
        if (data.TryGetProperty("avatarInfoList", out var avatars) && avatars.ValueKind == JsonValueKind.Array)
        {
            foreach (var avatar in avatars.EnumerateArray())
            {
                var parsed = ParseCharacter(avatar, characterMap);
                if (parsed is not null)
                    characters.Add(parsed);
            }
        }

        return (profile, characters);
    }

    private static EnkaCharacter? ParseCharacter(JsonElement avatar, Dictionary<string, CharacterInfo> characterMap)
    {
        if (!avatar.TryGetProperty("equipList", out var equips) || equips.ValueKind != JsonValueKind.Array)
            return null;
        if (!avatar.TryGetProperty("avatarId", out var idProp) || !idProp.TryGetInt32(out int avatarId))
            return null;

        var weapon = new EnkaWeapon();
        var artifacts = new List<ArtifactOutput>();

        foreach (var equip in equips.EnumerateArray())
        {
            if (!equip.TryGetProperty("flat", out var flat))
                continue;
            string? itemType = GetString(flat, "itemType");

            if (itemType == "ITEM_WEAPON")
            {
                string? icon = GetString(flat, "icon");
                weapon = new EnkaWeapon
                {
                    Level = equip.TryGetProperty("weapon", out var w) ? GetInt(w, "level", 1) : 1,
                    Refinement = GetRefinement(equip),
                    IconUrl = icon is null ? "" : $"{ImageBase}{icon}.png",
                };
                continue;
            }

            if (itemType != "ITEM_RELIQUARY" || GetInt(flat, "rankLevel", 0) != 5)
                continue;

            if (!flat.TryGetProperty("equipType", out var equipTypeProp)
                || !SlotMap.TryGetValue(equipTypeProp.GetString() ?? "", out int slot))
                continue;

            string? mainProp = flat.TryGetProperty("reliquaryMainstat", out var main)
                && main.TryGetProperty("mainPropId", out var mainId)
                ? mainId.GetString() : null;
            if (mainProp is null || !StatMap.TryGetValue(mainProp, out int mainType))
                continue;

            var subStats = new List<SubstatEntry>();
            double cv = 0;
            if (flat.TryGetProperty("reliquarySubstats", out var subs) && subs.ValueKind == JsonValueKind.Array)
            {
                foreach (var sub in subs.EnumerateArray())
                {
                    string? propId = sub.TryGetProperty("appendPropId", out var p) ? p.GetString() : null;
                    if (propId is null || !StatMap.TryGetValue(propId, out int type))
                        continue;
                    double value = GetDouble(sub, "statValue", 0);
                    if (type == Stat.CritRate) cv += value * 2;
                    if (type == Stat.CritDMG) cv += value;
                    // Enka omits roll counts; dots are inferred from the value.
                    subStats.Add(new SubstatEntry { Type = type, Value = value, Rolls = 1 });
                }
            }

            string? relicIcon = GetString(flat, "icon");
            artifacts.Add(new ArtifactOutput
            {
                Slot = slot,
                Level = equip.TryGetProperty("reliquary", out var rel) ? GetInt(rel, "level", 1) - 1 : 0,
                MainStat = new MainStatEntry
                {
                    Type = mainType,
                    Value = flat.TryGetProperty("reliquaryMainstat", out var m) ? GetDouble(m, "statValue", 0) : 0,
                },
                SubStats = subStats,
                CritValue = cv,
                IconUrl = relicIcon is null ? null : $"{ImageBase}{relicIcon}.png",
            });
        }

        string name = avatarId.ToString();
        string iconName = $"UI_AvatarIcon_{name}";
        if (avatarId == 10000005) { name = "Aether"; iconName = "UI_AvatarIcon_PlayerBoy"; }
        else if (avatarId == 10000007) { name = "Lumine"; iconName = "UI_AvatarIcon_PlayerGirl"; }
        else if (characterMap.TryGetValue(avatarId.ToString(), out var info))
        {
            name = info.Name ?? name;
            iconName = info.Icon ?? iconName;
        }

        int level = 1;
        if (avatar.TryGetProperty("propMap", out var props)
            && props.TryGetProperty("4001", out var lv)
            && lv.TryGetProperty("val", out var val)
            && int.TryParse(val.GetString(), out int parsed))
            level = parsed;

        int constellation = 0;
        if (avatar.TryGetProperty("talentIdList", out var talents) && talents.ValueKind == JsonValueKind.Array)
            constellation = talents.GetArrayLength();

        return new EnkaCharacter
        {
            AvatarId = avatarId,
            Name = name,
            Level = level,
            IconUrl = $"{ImageBase}{iconName}.png",
            Weapon = weapon,
            Artifacts = artifacts,
            Constellation = constellation,
        };
    }

    private static int GetRefinement(JsonElement equip)
    {
        if (equip.TryGetProperty("weapon", out var w)
            && w.TryGetProperty("affixMap", out var affix)
            && affix.ValueKind == JsonValueKind.Object)
        {
            foreach (var kv in affix.EnumerateObject())
                return (int)kv.Value.GetDouble() + 1;
        }
        return 1;
    }

    private static string? GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    private static int GetInt(JsonElement el, string name, int fallback) =>
        GetIntOrNull(el, name) ?? fallback;

    private static int? GetIntOrNull(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p))
            return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetInt32(out int n) => n,
            JsonValueKind.String when int.TryParse(p.GetString(), out int s) => s,
            _ => null,
        };
    }

    private static double GetDouble(JsonElement el, string name, double fallback)
    {
        if (!el.TryGetProperty(name, out var p))
            return fallback;
        return p.ValueKind switch
        {
            JsonValueKind.Number => p.GetDouble(),
            JsonValueKind.String when double.TryParse(p.GetString(), out double s) => s,
            _ => fallback,
        };
    }

    public class CharacterInfo
    {
        public string? Name { get; set; }
        public string? Icon { get; set; }
    }
}
