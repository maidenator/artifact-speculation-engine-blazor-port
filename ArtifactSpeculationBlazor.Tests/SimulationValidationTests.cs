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
    [InlineData(500, 100)]
    public void ClampTopK_BoundsRange(int input, int expected)
    {
        Assert.Equal(expected, SimulationValidation.ClampTopK(input));
    }

    [Fact]
    public void ClampMinCritValue_NeverNegative()
    {
        Assert.Equal(0, SimulationValidation.ClampMinCritValue(-5));
        Assert.Equal(25, SimulationValidation.ClampMinCritValue(25));
    }
}
