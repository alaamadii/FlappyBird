using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.Core.Tests;

public sealed class PipeSystemTests
{
    [Fact]
    public void InitialPairsHaveValidGeometrySpacingAndAReadOnlyCollection()
    {
        var source = new SequenceGapSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(), source);
        Assert.Equal(2, pipes.Pairs.Count);
        Assert.Equal(520, pipes.Pairs[0].X);
        Assert.Equal(780, pipes.Pairs[1].X);
        Assert.Equal(260, pipes.Pairs[1].X - pipes.Pairs[0].X);
        Assert.Equal(2, source.Calls);
        Assert.Throws<NotSupportedException>(() => ((IList<PipePair>)pipes.Pairs).Clear());
        Assert.All(source.Bounds, bound => Assert.Equal((40d, 340d), bound));
    }

    [Fact]
    public void MovementUsesSecondsAndBothPairsCanBeFullyVisible()
    {
        var pipes = new PipeSystem(new Playfield(), new GameSettings(), new SequenceGapSource());
        pipes.Update(0.5);
        Assert.Equal(450, pipes.Pairs[0].X);
        pipes.Update(1);
        pipes.Update(1);
        pipes.Update(0.5);
        Assert.All(pipes.Pairs, p => Assert.True(p.X >= 0 && p.Right <= 480));
    }

    [Fact]
    public void ExactlyTouchingLeftEdgeDoesNotRecycleButLeavingDoes()
    {
        var source = new SequenceGapSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(pipeSpeed: 128, pipeSpacing: 256), source);
        var original = pipes.Pairs[0];
        for (var step = 0; step < 4; step++) pipes.Update(1);
        pipes.Update(0.5625);
        Assert.Equal(0, original.Right);
        Assert.Equal(2, source.Calls);
        var oldGap = original.GapTop;

        pipes.Update(1d / 128);

        Assert.Same(original, pipes.Pairs[1]);
        Assert.Equal(3, source.Calls);
        Assert.NotEqual(oldGap, original.GapTop);
        Assert.Equal(256, pipes.Pairs[1].X - pipes.Pairs[0].X);
    }

    [Fact]
    public void LargestSupportedUpdateRecyclesBothExpiredPairsInOrder()
    {
        var source = new SequenceGapSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(pipeSpeed: 520), source);
        var originals = pipes.Pairs.ToArray();
        pipes.Update(1);
        Assert.Equal(64, pipes.Pairs[0].Right);
        pipes.Update(1);
        Assert.Equal(4, source.Calls);
        Assert.Equal(2, pipes.Pairs.Count);
        Assert.Same(originals[0], pipes.Pairs[0]);
        Assert.Same(originals[1], pipes.Pairs[1]);
        Assert.Equal(64, pipes.Pairs[0].Right);
        Assert.Equal(260, pipes.Pairs[1].X - pipes.Pairs[0].X);
    }

    [Fact]
    public void FractionalFullCycleAtBoundaryCannotTriggerAThirdRecycle()
    {
        var source = new SequenceGapSource();
        var settings = new GameSettings(pipeSpacing: 200.1, pipeSpeed: 400.2);
        var pipes = new PipeSystem(new Playfield(), settings, source);
        var originals = pipes.Pairs.ToArray();
        originals[0].Reset(-64, 40);
        originals[1].Reset(-64 + settings.PipeSpacing, 190);

        pipes.Update(1);

        Assert.Same(originals[0], pipes.Pairs[0]);
        Assert.Same(originals[1], pipes.Pairs[1]);
        Assert.Equal(0, pipes.Pairs[0].Right);
        Assert.Equal(4, source.Calls);
        Assert.Equal(settings.PipeSpacing, pipes.Pairs[1].X - pipes.Pairs[0].X, 8);
    }

    [Fact]
    public void ManyCyclesPreserveCountInstancesSpacingAndGapBounds()
    {
        var source = new SequenceGapSource();
        var settings = new GameSettings();
        var pipes = new PipeSystem(new Playfield(), settings, source);
        var originals = pipes.Pairs.ToArray();
        for (var step = 0; step < 60000; step++)
        {
            pipes.Update(1d / 60);
            Assert.Equal(2, pipes.Pairs.Count);
            Assert.True(pipes.Pairs[0].Right >= 0);
            Assert.Equal(settings.PipeSpacing, pipes.Pairs[1].X - pipes.Pairs[0].X, 8);
            Assert.All(pipes.Pairs, p =>
            {
                Assert.Contains(p, originals);
                Assert.InRange(p.GapTop, 40, Math.BitDecrement(340));
                Assert.Equal(560, p.Lower.Bottom, 8);
            });
        }

        Assert.True(source.Calls > 500);
    }

    [Fact]
    public void ResetReusesPairsRestoresInitialPositionsAndRefreshesGaps()
    {
        var source = new SequenceGapSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(), source);
        var originals = pipes.Pairs.ToArray();
        pipes.Update(1);
        pipes.Reset();
        Assert.Equal(520, pipes.Pairs[0].X);
        Assert.Equal(780, pipes.Pairs[1].X);
        Assert.Equal(4, source.Calls);
        Assert.All(pipes.Pairs, p => Assert.Contains(p, originals));
        Assert.NotEqual(40, pipes.Pairs[0].GapTop);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(1.00001)]
    public void UnsupportedElapsedIsRejectedBeforeMovingOrGenerating(double elapsed)
    {
        var source = new SequenceGapSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(), source);
        Assert.Throws<ArgumentOutOfRangeException>(() => pipes.Update(elapsed));
        Assert.Equal(520, pipes.Pairs[0].X);
        Assert.Equal(2, source.Calls);
    }

    [Fact]
    public void ZeroTimeDoesNotMoveOrConsumeRandomness()
    {
        var source = new SequenceGapSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(), source);
        pipes.Update(0);
        Assert.Equal(520, pipes.Pairs[0].X);
        Assert.Equal(2, source.Calls);
    }

    [Fact]
    public void InvalidGapSourceIsRejected()
    {
        Assert.Throws<InvalidOperationException>(() => new PipeSystem(new Playfield(), new GameSettings(), new InvalidGapSource()));
    }

    [Fact]
    public void InvalidRecycledGapDoesNotMoveExistingPairs()
    {
        var source = new FailingAfterInitializationSource();
        var pipes = new PipeSystem(new Playfield(), new GameSettings(), source);
        for (var step = 0; step < 4; step++) pipes.Update(1);
        var before = pipes.Pairs.Select(p => (p.X, p.GapTop)).ToArray();

        Assert.Throws<InvalidOperationException>(() => pipes.Update(0.5));

        Assert.Equal(before, pipes.Pairs.Select(p => (p.X, p.GapTop)).ToArray());
    }

    [Fact]
    public void UpdatePartitionsPreservePositionsAndGenerationOrder()
    {
        var slowSource = new SequenceGapSource();
        var fastSource = new SequenceGapSource();
        var slow = new PipeSystem(new Playfield(), new GameSettings(), slowSource);
        var fast = new PipeSystem(new Playfield(), new GameSettings(), fastSource);
        for (var step = 0; step < 6; step++) slow.Update(1);
        for (var step = 0; step < 360; step++) fast.Update(1d / 60);
        for (var index = 0; index < 2; index++)
        {
            Assert.Equal(slow.Pairs[index].X, fast.Pairs[index].X, 8);
            Assert.Equal(slow.Pairs[index].GapTop, fast.Pairs[index].GapTop);
        }

        Assert.Equal(slowSource.Calls, fastSource.Calls);
    }

    private sealed class FailingAfterInitializationSource : IGapSource
    {
        private int calls;
        public double NextGapTop(double minimumInclusive, double maximumExclusive) =>
            calls++ < 2 ? minimumInclusive : maximumExclusive;
    }

    private sealed class InvalidGapSource : IGapSource
    {
        public double NextGapTop(double minimumInclusive, double maximumExclusive) => maximumExclusive;
    }

    private sealed class SequenceGapSource : IGapSource
    {
        public int Calls { get; private set; }
        public List<(double, double)> Bounds { get; } = [];
        public double NextGapTop(double minimumInclusive, double maximumExclusive)
        {
            Bounds.Add((minimumInclusive, maximumExclusive));
            double[] fractions = [0, 0.5, 0.9, 0.25];
            return minimumInclusive + (maximumExclusive - minimumInclusive) * fractions[Calls++ % fractions.Length];
        }
    }
}
