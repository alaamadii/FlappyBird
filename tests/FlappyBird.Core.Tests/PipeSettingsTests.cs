using FlappyBird.Core.Models;

namespace FlappyBird.Core.Tests;

public sealed class PipeSettingsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AllPipeTuningRequiresFinitePositiveValues(double invalid)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(pipeSpeed: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(pipeWidth: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(pipeGapHeight: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(pipeSpacing: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(initialPipeOffset: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(gapMargin: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSettings(birdClearance: invalid));
    }

    [Fact]
    public void ImpossibleCrossFieldAndPlayfieldConfigurationsAreRejected()
    {
        var field = new Playfield();
        Assert.Throws<ArgumentException>(() => new GameSettings(pipeGapHeight: 72).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings(pipeGapHeight: 480).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings(gapMargin: 200).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings(pipeSpacing: 174).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings(pipeSpacing: 450).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings(pipeSpeed: 521).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings(initialPipeOffset: double.MaxValue).ValidateFor(field));
        Assert.Throws<ArgumentException>(() => new GameSettings().ValidateFor(new Playfield(300, 640, 80)));
    }

    [Fact]
    public void DefaultsProvideBothVerticalAndHorizontalBirdClearance()
    {
        var settings = new GameSettings();
        settings.ValidateFor(new Playfield());
        Assert.True(settings.PipeGapHeight > settings.BirdHeight + 2 * settings.BirdClearance);
        Assert.True(settings.PipeSpacing - settings.PipeWidth > settings.BirdWidth + 2 * settings.BirdClearance);
        Assert.Equal(600, settings.Gravity);
        Assert.Equal(260, settings.FlapSpeed);
    }
}
