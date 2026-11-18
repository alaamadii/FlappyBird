using FlappyBird.App.Animation;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Tests;

public sealed class SceneAnimationTests
{
    [Fact]
    public void ReadyAnimatesWingsAndBobWithoutScrollingTheWorld()
    {
        var animation = new SceneAnimation();
        animation.Advance(0.15, GameState.Ready, 0);
        Assert.NotEqual(SceneAnimationFrame.Still, animation.Frame);
        Assert.InRange(animation.Frame.BobOffset, -3, 3);
        Assert.NotEqual(0, animation.Frame.BobOffset);
        Assert.Equal(0, animation.Frame.GroundOffset);
        Assert.Equal(0, animation.Frame.BackdropOffset);
        Assert.Equal(0, animation.Frame.TiltDegrees);
    }

    [Fact]
    public void RunningScrollsWrapsAndClampsVisualTilt()
    {
        var animation = new SceneAnimation();
        animation.Advance(0.1, GameState.Running, -1000);
        Assert.Equal(14, animation.Frame.GroundOffset, 5);
        Assert.Equal(1, animation.Frame.BackdropOffset, 5);
        Assert.Equal(-8, animation.Frame.TiltDegrees);
        animation.Advance(0.1, GameState.Running, 1000);
        Assert.Equal(4, animation.Frame.GroundOffset, 5);
        Assert.Equal(2, animation.Frame.BackdropOffset, 5);
        Assert.Equal(12, animation.Frame.TiltDegrees);
        Assert.Equal(0, animation.Frame.BobOffset);
    }

    [Fact]
    public void GroundScrollUsesTheSuppliedPipeSpeed()
    {
        var animation = new SceneAnimation(new GameSettings(pipeSpeed: 90));
        animation.Advance(0.1, GameState.Running, 0);
        Assert.Equal(9, animation.Frame.GroundOffset, 5);
    }

    [Fact]
    public void FreshFlapImmediatelyRestartsWingPoseAndClearsIdleBob()
    {
        var animation = new SceneAnimation();
        animation.Advance(0.25, GameState.Ready, 0);
        Assert.Equal(2, animation.Frame.WingFrame);
        animation.Flap();
        Assert.Equal(0, animation.Frame.WingFrame);
        Assert.Equal(-8, animation.Frame.TiltDegrees);
        Assert.Equal(0, animation.Frame.BobOffset);
    }

    [Fact]
    public void GameOverFreezesBothSnapshotAndInternalPresentationTime()
    {
        var animation = new SceneAnimation();
        var reference = new SceneAnimation();
        animation.Advance(0.15, GameState.Running, 20);
        reference.Advance(0.15, GameState.Running, 20);
        var frozen = animation.Frame;
        animation.Advance(1, GameState.GameOver, 0);
        Assert.Equal(frozen, animation.Frame);
        animation.Advance(0.1, GameState.Running, 20);
        reference.Advance(0.1, GameState.Running, 20);
        Assert.Equal(reference.Frame, animation.Frame);
    }

    [Fact]
    public void ResetRestoresTheSameInitialPresentation()
    {
        var animation = new SceneAnimation();
        animation.Advance(0.35, GameState.Running, 200);
        animation.Flap();
        animation.Reset();
        Assert.Equal(SceneAnimationFrame.Still, animation.Frame);
        animation.Advance(0, GameState.Ready, 0);
        Assert.Equal(SceneAnimationFrame.Still, animation.Frame);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidElapsedIsRejectedBeforeMutation(double elapsed)
    {
        var animation = new SceneAnimation();
        animation.Advance(0.1, GameState.Ready, 0);
        var before = animation.Frame;
        Assert.Throws<ArgumentOutOfRangeException>(() => animation.Advance(elapsed, GameState.Running, 0));
        Assert.Equal(before, animation.Frame);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidVelocityIsRejectedBeforeMutation(double velocity)
    {
        var animation = new SceneAnimation();
        Assert.Throws<ArgumentOutOfRangeException>(() => animation.Advance(0.1, GameState.Running, velocity));
        Assert.Equal(SceneAnimationFrame.Still, animation.Frame);
    }
}
