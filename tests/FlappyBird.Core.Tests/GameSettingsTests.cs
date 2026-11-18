using FlappyBird.Core.Models;

namespace FlappyBird.Core.Tests;

public sealed class GameSettingsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidPhysicalValuesAreRejected(double invalid)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(gravity: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(flapSpeed: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(maxFallSpeed: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(birdWidth: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(birdHeight: invalid));
    }

    [Fact]
    public void CustomTuningDrivesBirdDimensionsAndMotion()
    {
        var settings = new GameSettings(gravity: 400, flapSpeed: 200, birdWidth: 40, birdHeight: 30);
        var bird = new Bird(new Playfield(), settings);
        bird.Flap();
        bird.Update(0.1);
        Assert.Equal(40, bird.Width);
        Assert.Equal(30, bird.Height);
        Assert.Equal(-160, bird.VerticalVelocity, 9);
    }
}
