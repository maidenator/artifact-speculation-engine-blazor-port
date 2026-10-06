using ArtifactSpeculationBlazor.Models;
using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class SimulationValidationTests
{
    [Theory]
    [InlineData(2000, 2000)]
    [InlineData(1999, 1980)]
    [InlineData(21, 20)]
    [InlineData(0, 20)]
    [InlineData(-500, 20)]
    [InlineData(10_000_001, 10_000_000)]
    public void ClampResinBudget_RoundsDownToStep(int input, int expected)
    {
        Assert.Equal(expected, SimulationValidation.ClampResinBudget(input));
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(0, 1)]
    [InlineData(500, 20)]
    public void ClampTopK_BoundsRange(int input, int expected)
    {
        Assert.Equal(expected, SimulationValidation.ClampTopK(input));
    }

    [Theory]
    [InlineData(20, 20)]
    [InlineData(0, 1)]
    [InlineData(500, 100)]
    public void ClampTrialCount_BoundsRange(int input, int expected)
    {
        Assert.Equal(expected, SimulationValidation.ClampTrialCount(input));
    }

    [Fact]
    public void ClampMinCritValue_NeverNegative()
    {
        Assert.Equal(0, SimulationValidation.ClampMinCritValue(-5));
        Assert.Equal(25, SimulationValidation.ClampMinCritValue(25));
    }

    [Fact]
    public void Normalize_DropsIncompatibleTargetMain()
    {
        var settings = new SimulationSettings { TargetSlot = 0, TargetMainStat = Stat.CritDMG };
        SimulationValidation.Normalize(settings);
        Assert.Null(settings.TargetMainStat);
        Assert.Equal(0, settings.TargetSlot);
    }

    [Fact]
    public void Normalize_KeepsCompatibleTargetMain()
    {
        var settings = new SimulationSettings { TargetSlot = 3, TargetMainStat = Stat.PyroDMG };
        SimulationValidation.Normalize(settings);
        Assert.Equal(Stat.PyroDMG, settings.TargetMainStat);
    }
}
