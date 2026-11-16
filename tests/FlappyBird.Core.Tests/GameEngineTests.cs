using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.Core.Tests;

public sealed class GameEngineTests
{
    [Fact]
    public void ReadyDoesNotAdvanceUntilFirstFlapStartsRunning()
    {
        var engine = new GameEngine(new Playfield(), new GameSettings());
        var initialY = engine.Bird.Y;
        Assert.Equal(GameState.Ready, engine.State);
        engine.Update(1);
        Assert.Equal(initialY, engine.Bird.Y);

        engine.Flap();
        Assert.Equal(GameState.Running, engine.State);
        Assert.Equal(-320, engine.Bird.VerticalVelocity);
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
}
