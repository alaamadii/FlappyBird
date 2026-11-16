using FlappyBird.Core.Models;

namespace FlappyBird.Core.Tests;

public sealed class BirdTests
{
    private static Bird CreateBird() => new(new Playfield(), new GameSettings());

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
        var bird = new Bird(new Playfield(), new GameSettings(flapSpeed: 1000));
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
}
