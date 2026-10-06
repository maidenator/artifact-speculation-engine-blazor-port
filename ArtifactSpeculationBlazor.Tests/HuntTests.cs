using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class HuntTests
{
    private static ArtifactOutput Flower(double critDmg = 24.88) => new()
    {
        Slot = 0,
        Level = 20,
        MainStat = new MainStatEntry { Type = Stat.FlatHp, Value = 4780 },
        SubStats =
        [
            new SubstatEntry { Type = Stat.CritDMG, Value = critDmg, Rolls = 4 },
            new SubstatEntry { Type = Stat.CritRate, Value = 9.33, Rolls = 3 },
        ],
        CritValue = critDmg + 9.33 * 2,
    };

    private static HuntItem FlowerHunt() => new()
    {
        Id = "h1",
        Slot = 0,
        MainStat = Stat.FlatHp,
        Substats =
        [
            new HuntSubstat { Stat = Stat.CritDMG, MinRolls = 4 }, // 6.22*4=24.88
            new HuntSubstat { Stat = Stat.CritRate, MinRolls = 3 }, // 3.11*3=9.33
        ],
    };

    [Fact]
    public void MatchesArtifact_AcceptsExactThreshold()
    {
        Assert.True(HuntMatcher.MatchesArtifact(Flower(), FlowerHunt()));
    }

    [Fact]
    public void MatchesArtifact_RejectsWrongSlotOrMain()
    {
        var art = Flower();
        art.Slot = 2;
        Assert.False(HuntMatcher.MatchesArtifact(art, FlowerHunt()));

        art = Flower();
        art.MainStat.Type = Stat.FlatAtk;
        Assert.False(HuntMatcher.MatchesArtifact(art, FlowerHunt()));
    }

    [Fact]
    public void MatchesArtifact_RejectsMissingOrWeakSubstat()
    {
        var hunt = FlowerHunt();
        hunt.Substats.Add(new HuntSubstat { Stat = Stat.AtkPercent, MinRolls = 1 });
        Assert.False(HuntMatcher.MatchesArtifact(Flower(), hunt));

        Assert.False(HuntMatcher.MatchesArtifact(Flower(critDmg: 10), FlowerHunt()));
    }

    [Fact]
    public void MatchesArtifact_AllowsFivePercentTolerance()
    {
        // 24.88 * 0.96 = 23.88 ≥ 24.88 * 0.95 = 23.64 → pass.
        Assert.True(HuntMatcher.MatchesArtifact(Flower(critDmg: 23.9), FlowerHunt()));
        // 24.88 * 0.90 = 22.39 < 23.64 → fail.
        Assert.False(HuntMatcher.MatchesArtifact(Flower(critDmg: 22.4), FlowerHunt()));
    }

    [Fact]
    public async Task RunHunt_FindsItemWithExactResin()
    {
        // Always single drops; first artifact never matches (wrong slot),
        // second matches → found on run 2 → 40 resin.
        var arts = new Queue<ArtifactOutput>([WrongSlot(), Flower()]);
        var runner = new HuntRunner(_ => Task.FromResult(DequeueAll(arts)), () => 0.99);

        var result = await runner.RunHuntAsync([FlowerHunt()], maxBudget: 2000);

        Assert.True(result.AllFound);
        Assert.Equal(40, result.TotalResinSpent);
        Assert.Equal(40, result.ItemResults[0].ResinSpent);
        Assert.NotNull(result.ItemResults[0].Artifact);

        static ArtifactOutput WrongSlot()
        {
            var a = Flower();
            a.Slot = 4;
            return a;
        }
        static List<ArtifactOutput> DequeueAll(Queue<ArtifactOutput> q)
        {
            var list = new List<ArtifactOutput>();
            while (q.Count > 0)
                list.Add(q.Dequeue());
            return list;
        }
    }

    [Fact]
    public async Task RunHunt_RespectsBudget()
    {
        var dud = Flower();
        dud.Slot = 4;
        var runner = new HuntRunner(
            _ => Task.FromResult(new List<ArtifactOutput> { dud }),
            () => 0.99);

        var result = await runner.RunHuntAsync([FlowerHunt()], maxBudget: 100);

        Assert.False(result.AllFound);
        Assert.True(result.TotalResinSpent <= 100);
    }

    [Fact]
    public async Task RunHunt_MatchesSetOnlyWithinDomain()
    {
        var hunt = FlowerHunt();
        hunt.DomainId = "midsummer_courtyard";
        hunt.SetId = "thundering_fury";

        // Force set coin to sets[1] (draw ≥ 0.5) with single drops:
        // artifact gets thundersoother → must NOT match thundering_fury hunt.
        var runner = new HuntRunner(
            _ => Task.FromResult(new List<ArtifactOutput> { Flower() }),
            () => 0.99);

        var result = await runner.RunHuntAsync([hunt], maxBudget: 60);

        Assert.False(result.AllFound);
    }
}
