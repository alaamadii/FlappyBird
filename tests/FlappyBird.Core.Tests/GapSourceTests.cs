using FlappyBird.Core.Services;

namespace FlappyBird.Core.Tests;

public sealed class GapSourceTests
{
    [Fact]
    public void SeededInstancesProduceTheSameBoundedSequence()
    {
        var first = new RandomGapSource(new Random(42));
        var second = new RandomGapSource(new Random(42));
        for (var sample = 0; sample < 100; sample++)
        {
            var gap = first.NextGapTop(40, 340);
            Assert.Equal(gap, second.NextGapTop(40, 340));
            Assert.InRange(gap, 40, Math.BitDecrement(340));
        }
    }

    [Fact]
    public void SourceHandlesInclusiveMinimumAndExclusiveMaximumRounding()
    {
        Assert.Equal(40, new RandomGapSource(new FixedRandom(0)).NextGapTop(40, 340));
        var nearMaximum = new RandomGapSource(new FixedRandom(Math.BitDecrement(1d))).NextGapTop(40, 340);
        Assert.True(nearMaximum < 340);
    }

    private sealed class FixedRandom(double sample) : Random
    {
        public override double NextDouble() => sample;
    }
}
