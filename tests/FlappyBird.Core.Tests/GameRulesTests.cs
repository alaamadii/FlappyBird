using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.Core.Tests;

public sealed class GameRulesTests
{
    private static GameEngine Gentle(double speed = 128, double birdWidth = 86, double pipeWidth = 64) =>
        new(new Playfield(), new GameSettings(gravity: 1, flapSpeed: 1,
            pipeSpeed: speed, birdWidth: birdWidth, pipeWidth: pipeWidth), new MiddleGapSource());

    [Theory]
    [InlineData(300)]
    [InlineData(40)]
    public void ExactContactWithEitherPipeEndsRunWithoutAdvancingPastContact(double gapTop)
    {
        var engine = Gentle();
        engine.Pipes.Pairs[0].Reset(engine.Bird.X + engine.Bird.Width, gapTop);
        var position = engine.Bird.Y;
        engine.Flap();
        engine.Update(1d / 60);
        Assert.Equal(GameState.GameOver, engine.State);
        Assert.Equal(position, engine.Bird.Y);
        Assert.Equal(0, engine.Score);
    }

    [Fact]
    public void SafeGapPassScoresOnlyAfterTrailingEdgeAndOnlyOnce()
    {
        var engine = Gentle();
        var pair = engine.Pipes.Pairs[0];
        pair.Reset(engine.Bird.X - pair.Width + 1, 190);
        engine.Flap();
        engine.Update(1d / 128);
        Assert.Equal(engine.Bird.X, pair.Right);
        Assert.Equal(0, engine.Score);
        engine.Update(1d / 128);
        Assert.Equal(GameState.Running, engine.State);
        Assert.Equal(1, engine.Score);
        Assert.True(pair.HasScored);
        engine.Update(1d / 128);
        Assert.Equal(1, engine.Score);
    }

    [Fact]
    public void RecyclingMakesTheSamePairEligibleOnItsNextPass()
    {
        var engine = Gentle();
        var pair = engine.Pipes.Pairs[0];
        pair.Reset(engine.Bird.X - pair.Width + 1, 190);
        engine.Flap();
        engine.Update(1d / 64);
        Assert.True(pair.HasScored);
        engine.Update(1);
        engine.Update(1);
        Assert.False(pair.HasScored);
        for (var step = 0; step < 600 && engine.Score < 3; step++) engine.Update(1d / 60);
        Assert.Equal(GameState.Running, engine.State);
        Assert.Equal(3, engine.Score);
        Assert.True(pair.HasScored);
    }

    [Fact]
    public void CollisionWinsOverANewPointWithinTheSameStep()
    {
        var engine = Gentle();
        var pair = engine.Pipes.Pairs[0];
        pair.Reset(engine.Bird.X - pair.Width + 0.5, 190);
        engine.Pipes.Pairs[1].Reset(engine.Bird.X + engine.Bird.Width + 0.5, 300);
        engine.Flap();
        engine.Update(1d / 60);
        Assert.Equal(GameState.GameOver, engine.State);
        Assert.Equal(0, engine.Score);
        Assert.False(pair.HasScored);
    }

    [Fact]
    public void EarnedScoreSurvivesLossAndGameOverIsFrozen()
    {
        var source = new MiddleGapSource();
        var engine = new GameEngine(new Playfield(), new GameSettings(gravity: 1, flapSpeed: 1), source);
        var pair = engine.Pipes.Pairs[0];
        pair.Reset(engine.Bird.X - pair.Width - 1, 190);
        engine.Flap();
        engine.Update(1d / 60);
        Assert.Equal(1, engine.Score);
        engine.Pipes.Pairs[1].Reset(engine.Bird.X + engine.Bird.Width, 300);
        engine.Update(1d / 60);
        Assert.Equal(GameState.GameOver, engine.State);
        var bird = (engine.Bird.Y, engine.Bird.VerticalVelocity);
        var pipes = engine.Pipes.Pairs.Select(p => (p.X, p.GapTop, p.HasScored)).ToArray();
        var calls = source.Calls;
        engine.Flap();
        engine.Update(1);
        engine.Update(0);
        Assert.Equal(bird, (engine.Bird.Y, engine.Bird.VerticalVelocity));
        Assert.Equal(pipes, engine.Pipes.Pairs.Select(p => (p.X, p.GapTop, p.HasScored)).ToArray());
        Assert.Equal(1, engine.Score);
        Assert.Equal(calls, source.Calls);
    }

    [Fact]
    public void NormalFlightHitsGroundAndStopsExactlyAtGroundTop()
    {
        var engine = new GameEngine(new Playfield(), new GameSettings(), new MiddleGapSource());
        // Keep the slower descent isolated from pipes so this still tests ground contact.
        foreach (var pair in engine.Pipes.Pairs) pair.Reset(pair.X + 2000, pair.GapTop);
        engine.Flap();
        engine.Update(1);
        engine.Update(1);
        engine.Update(1);
        Assert.Equal(GameState.GameOver, engine.State);
        Assert.Equal(560, engine.Bird.Y + engine.Bird.Height, 9);
        Assert.Equal(0, engine.Bird.VerticalVelocity);
        Assert.True(engine.Pipes.Pairs[0].X > engine.Bird.X + engine.Bird.Width);
    }

    [Theory]
    [InlineData(250.4)]
    [InlineData(900)]
    public void ExistingGroundTouchOrOverlapEndsImmediatelyWithoutNegativeTime(double gravity)
    {
        var engine = new GameEngine(new Playfield(),
            new GameSettings(gravity: gravity, maxFallSpeed: double.MaxValue), new MiddleGapSource());
        engine.Flap();
        engine.Bird.Reset();
        engine.Bird.Update(1); // Independent model deliberately supplies an already overlapping state.
        var pipeX = engine.Pipes.Pairs[0].X;
        engine.Update(1d / 60);
        Assert.Equal(GameState.GameOver, engine.State);
        Assert.Equal(512, engine.Bird.Y);
        Assert.Equal(pipeX, engine.Pipes.Pairs[0].X);
    }

    [Fact]
    public void RecyclingCannotEraseAnUnscoredPass()
    {
        var engine = Gentle();
        var departing = engine.Pipes.Pairs[0];
        departing.Reset(-departing.Width + 0.5, 190);
        engine.Flap();
        engine.Update(1d / 60);
        Assert.Equal(GameState.Running, engine.State);
        Assert.Equal(1, engine.Score);
        Assert.True(departing.X > engine.Bird.X + engine.Bird.Width);
        Assert.False(departing.HasScored); // Fresh eligibility belongs to the new cycle.
    }

    [Fact]
    public void CeilingClampingDoesNotEndTheRun()
    {
        var engine = new GameEngine(new Playfield(), new GameSettings(flapSpeed: 1000), new MiddleGapSource());
        engine.Flap();
        engine.Update(0.5);
        Assert.Equal(GameState.Running, engine.State);
        Assert.True(engine.Bird.Y >= 0);
        Assert.True(engine.Bird.VerticalVelocity >= 0);
    }

    [Fact]
    public void FastThinPipeCannotTunnelAcrossTinyBirdEvenInLargestSupportedUpdate()
    {
        var engine = Gentle(speed: 520, birdWidth: 0.01, pipeWidth: 0.01);
        engine.Pipes.Pairs[0].Reset(engine.Bird.X + engine.Bird.Width + 2, 300);
        engine.Flap();
        engine.Update(1);
        Assert.Equal(GameState.GameOver, engine.State);
        Assert.Equal(engine.Bird.X + engine.Bird.Width, engine.Pipes.Pairs[0].X, 8);
        Assert.Equal(0, engine.Score);
    }

    [Fact]
    public void RepeatedLossResetCyclesRestoreEverythingWithoutGrowingEntities()
    {
        var source = new MiddleGapSource();
        var engine = new GameEngine(new Playfield(), new GameSettings(), source);
        var initialY = engine.Bird.Y;
        var originals = engine.Pipes.Pairs.ToArray();
        for (var run = 0; run < 40; run++)
        {
            engine.Flap();
            engine.Update(1);
            engine.Update(1);
            Assert.Equal(GameState.GameOver, engine.State);
            engine.Reset();
            Assert.Equal(GameState.Ready, engine.State);
            Assert.Equal(0, engine.Score);
            Assert.Equal(initialY, engine.Bird.Y);
            Assert.Equal(0, engine.Bird.VerticalVelocity);
            Assert.Equal(520, engine.Pipes.Pairs[0].X);
            Assert.Equal(780, engine.Pipes.Pairs[1].X);
            Assert.All(engine.Pipes.Pairs, p => { Assert.False(p.HasScored); Assert.Contains(p, originals); });
        }

        Assert.Equal(82, source.Calls);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(1.00001)]
    public void InvalidElapsedIsRejectedWithoutPartialMutation(double elapsed)
    {
        var engine = Gentle();
        engine.Flap();
        var before = (engine.Bird.Y, engine.Bird.VerticalVelocity, engine.Pipes.Pairs[0].X, engine.Score);
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.Update(elapsed));
        Assert.Equal(before, (engine.Bird.Y, engine.Bird.VerticalVelocity, engine.Pipes.Pairs[0].X, engine.Score));
    }

    private sealed class MiddleGapSource : IGapSource
    {
        public int Calls { get; private set; }
        public double NextGapTop(double minimumInclusive, double maximumExclusive)
        {
            Calls++;
            return (minimumInclusive + maximumExclusive) / 2;
        }
    }
}
