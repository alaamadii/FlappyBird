using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.Core.Tests;

public sealed class GameEngineTests
{
    [Fact]
    public void ReadyDoesNotAdvanceUntilFirstFlapStartsRunning()
    {
        var settings = new GameSettings();
        var engine = new GameEngine(new Playfield(), settings);
        var initialY = engine.Bird.Y;
        Assert.Equal(GameState.Ready, engine.State);
        engine.Update(1);
        Assert.Equal(initialY, engine.Bird.Y);

        engine.Flap();
        Assert.Equal(GameState.Running, engine.State);
        Assert.Equal(-settings.FlapSpeed, engine.Bird.VerticalVelocity);
        engine.Update(1d / 60);
        Assert.True(engine.Bird.Y < initialY);
        engine.Reset();
        Assert.Equal(GameState.Ready, engine.State);
        Assert.Equal(initialY, engine.Bird.Y);
        Assert.Equal(0, engine.Bird.VerticalVelocity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidTimeIsRejectedEvenWhileReady(double elapsed)
    {
        var engine = new GameEngine(new Playfield(), new GameSettings());
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.Update(elapsed));
    }

    [Fact]
    public void ReadyFreezesPipesAndRunningMovesThemUntilReset()
    {
        var source = new CountingGapSource();
        var engine = new GameEngine(new Playfield(), new GameSettings(), source);
        var first = engine.Pipes.Pairs[0];
        engine.Update(1);
        Assert.Equal(520, first.X);
        Assert.Equal(2, source.Calls);
        engine.Flap();
        engine.Update(0.5);
        Assert.Equal(450, first.X, 8);
        engine.Reset();
        Assert.Equal(GameState.Ready, engine.State);
        Assert.Equal(520, engine.Pipes.Pairs[0].X);
        Assert.Equal(4, source.Calls);
    }

    [Fact]
    public void UnsupportedRunningStepDoesNotMutateBirdOrPipes()
    {
        var settings = new GameSettings();
        var engine = new GameEngine(new Playfield(), settings);
        engine.Flap();
        var birdY = engine.Bird.Y;
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.Update(1.1));
        Assert.Equal(birdY, engine.Bird.Y);
        Assert.Equal(-settings.FlapSpeed, engine.Bird.VerticalVelocity);
        Assert.Equal(520, engine.Pipes.Pairs[0].X);
    }

    private sealed class CountingGapSource : IGapSource
    {
        public int Calls { get; private set; }
        public double NextGapTop(double minimumInclusive, double maximumExclusive)
        {
            Calls++;
            return minimumInclusive;
        }
    }
}
