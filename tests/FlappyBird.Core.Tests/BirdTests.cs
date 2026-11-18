using FlappyBird.Core.Models;

namespace FlappyBird.Core.Tests;

public sealed class BirdTests
{
    // Fixed values keep integration assertions independent of gameplay-default tuning.
    private static Bird CreateBird() => new(new Playfield(), new GameSettings(gravity: 900, flapSpeed: 320));

    [Fact]
    public void InitialBirdIsCenteredAboveGroundAndStationary()
    {
        var bird = CreateBird();
        Assert.Equal(240, bird.X + bird.Width / 2);
        Assert.True(bird.Y >= 0 && bird.Y + bird.Height < 560);
        Assert.Equal(0, bird.VerticalVelocity);
    }

    [Fact]
    public void GravityUsesUpdatedVelocityForPosition()
    {
        var bird = CreateBird();
        var startY = bird.Y;

        bird.Update(0.1);

        Assert.Equal(90, bird.VerticalVelocity, 9);
        Assert.Equal(startY + 9, bird.Y, 9);
    }

    [Fact]
    public void FlapSetsUpwardVelocityInsteadOfAccumulatingImpulse()
    {
        var bird = CreateBird();
        bird.Update(0.1);
        bird.Flap();
        bird.Flap();
        Assert.Equal(-320, bird.VerticalVelocity);
        var startY = bird.Y;
        bird.Update(0.1);
        Assert.Equal(-230, bird.VerticalVelocity, 9);
        Assert.Equal(startY - 23, bird.Y, 9);
    }

    [Fact]
    public void ZeroTimeLeavesMotionUnchanged()
    {
        var bird = CreateBird();
        bird.Flap();
        var startY = bird.Y;
        bird.Update(0);
        Assert.Equal(startY, bird.Y);
        Assert.Equal(-320, bird.VerticalVelocity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.MaxValue)]
    public void InvalidOrOverflowingTimeIsRejectedWithoutMutation(double elapsed)
    {
        var bird = CreateBird();
        var initialY = bird.Y;
        Assert.Throws<ArgumentOutOfRangeException>(() => bird.Update(elapsed));
        Assert.Equal(initialY, bird.Y);
        Assert.Equal(0, bird.VerticalVelocity);
    }

    [Fact]
    public void CeilingClampsAndClearsUpwardVelocityThenGravityResumes()
    {
        var bird = new Bird(new Playfield(), new GameSettings(gravity: 900, flapSpeed: 1000));
        bird.Flap();
        bird.Update(0.5);
        Assert.Equal(0, bird.Y);
        Assert.Equal(0, bird.VerticalVelocity);
        bird.Update(0.1);
        Assert.Equal(9, bird.Y, 9);
        Assert.Equal(90, bird.VerticalVelocity, 9);
    }

    [Fact]
    public void ResetRestoresInitialPositionAndVelocity()
    {
        var bird = CreateBird();
        var initial = (bird.X, bird.Y);
        bird.Flap();
        bird.Update(0.1);
        bird.Reset();
        Assert.Equal(initial, (bird.X, bird.Y));
        Assert.Equal(0, bird.VerticalVelocity);
    }

    [Fact]
    public void GroundDoesNotEndOrClampFlightInThisStage()
    {
        var bird = CreateBird();
        bird.Update(2);
        Assert.True(bird.Y > 640);
        Assert.True(bird.VerticalVelocity > 0);
    }

    [Theory]
    [InlineData(481, 48)]
    [InlineData(86, 561)]
    public void BirdMustFitInsideTheInitialPlayArea(double width, double height)
    {
        Assert.Throws<ArgumentException>(() => new Bird(new Playfield(),
            new GameSettings(birdWidth: width, birdHeight: height)));
    }

    [Fact]
    public void DefaultFlapStaysAloftLongerWithoutIncreasingItsHeight()
    {
        var legacy = SampleFlap(new GameSettings(gravity: 900, flapSpeed: 320));
        var current = SampleFlap(new GameSettings());

        Assert.True(current.SecondsAloft >= legacy.SecondsAloft * 1.15,
            "The tuned flap should give at least 15% more time before returning to its starting height.");
        Assert.InRange(current.SecondsAloft, 0.8, 0.9);
        Assert.InRange(current.Rise, legacy.Rise - 2, legacy.Rise + 2);
    }

    [Fact]
    public void DefaultGravityMakesAnUnassistedDescentGentler()
    {
        var legacy = CreateBird();
        var current = new Bird(new Playfield(), new GameSettings());
        var startY = current.Y;
        for (var step = 0; step < 30; step++)
        {
            legacy.Update(1d / 60);
            current.Update(1d / 60);
        }

        Assert.True(current.Y > startY);
        Assert.True(current.VerticalVelocity > 0);
        Assert.True(current.VerticalVelocity < legacy.VerticalVelocity);
        Assert.True(current.Y - startY <= (legacy.Y - startY) * 0.75,
            "Half a second of falling should cover at least 25% less distance than the original tuning.");
    }

    private static (double Rise, double SecondsAloft) SampleFlap(GameSettings settings)
    {
        var bird = new Bird(new Playfield(), settings);
        var startY = bird.Y;
        var highestY = startY;
        bird.Flap();
        for (var step = 1; step <= 120; step++)
        {
            bird.Update(1d / 60);
            highestY = Math.Min(highestY, bird.Y);
            if (bird.Y >= startY && bird.VerticalVelocity > 0)
                return (startY - highestY, step / 60d);
        }

        throw new InvalidOperationException("Flap did not return to its starting height within two seconds.");
    }
}
