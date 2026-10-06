using ArtifactSpeculationBlazor.Services;

namespace ArtifactSpeculationBlazor.Tests;

public class TargetStatsTests
{
    [Fact]
    public void Aggregate_EmptySamples()
    {
        var dist = TargetStats.Aggregate([]);

        Assert.Equal(0, dist.Attempts);
        Assert.Equal(0, dist.Successes);
        Assert.Equal(0, dist.SuccessRate);
        Assert.Null(dist.P50Resin);
        Assert.Null(dist.P90Resin);
    }

    [Fact]
    public void Aggregate_AllFail()
    {
        var dist = TargetStats.Aggregate([null, null, null]);

        Assert.Equal(3, dist.Attempts);
        Assert.Equal(0, dist.Successes);
        Assert.Null(dist.P50Resin);
    }

    [Fact]
    public void Aggregate_PercentilesFollowFloorRule()
    {
        // Sorted: 100..1000 step 100 (n=10).
        // p50 -> idx floor(5.0)=5 -> 600; p90 -> idx floor(9.0)=9 -> 1000.
        var samples = Enumerable.Range(1, 10).Select(i => (double?)i * 100).ToList();

        var dist = TargetStats.Aggregate(samples);

        Assert.Equal(10, dist.Attempts);
        Assert.Equal(10, dist.Successes);
        Assert.Equal(1.0, dist.SuccessRate);
        Assert.Equal(600, dist.P50Resin);
        Assert.Equal(1000, dist.P90Resin);
    }

    [Fact]
    public void Aggregate_MixesWinsAndLosses()
    {
        var dist = TargetStats.Aggregate([100, null, 300, null]);

        Assert.Equal(4, dist.Attempts);
        Assert.Equal(2, dist.Successes);
        Assert.Equal(0.5, dist.SuccessRate);
        // Wins sorted [100,300]: p50 -> idx 1 -> 300; p90 -> idx 1 -> 300.
        Assert.Equal(300, dist.P50Resin);
        Assert.Equal(300, dist.P90Resin);
    }

    [Fact]
    public void Aggregate_SingleWin()
    {
        var dist = TargetStats.Aggregate([null, 420, null]);

        Assert.Equal(1, dist.Successes);
        Assert.Equal(420, dist.P50Resin);
        Assert.Equal(420, dist.P90Resin);
    }
}
