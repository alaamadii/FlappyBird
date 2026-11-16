using FlappyBird.App.Timing;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Tests;

public sealed class FixedStepLoopTests
{
    [Fact]
    public void FractionalTicksAccumulateIntoOneFixedUpdate()
    {
        var loop = new FixedStepLoop();
        var steps = new List<double>();
        Assert.Equal(0, loop.Advance(1d / 120, steps.Add));
        Assert.Equal(1, loop.Advance(1d / 120, steps.Add));
        Assert.Equal(1d / 60, Assert.Single(steps));
    }

    [Fact]
    public void EquivalentElapsedPartitionsProduceTheSameBirdFlight()
    {
        var first = new Bird(new Playfield(), new GameSettings());
        var second = new Bird(new Playfield(), new GameSettings());
        first.Flap();
        second.Flap();
        var slow = new FixedStepLoop();
        var fast = new FixedStepLoop();
        for (var i = 0; i < 30; i++)
        {
            slow.Advance(1d / 30, first.Update);
        }

        for (var i = 0; i < 120; i++)
        {
            fast.Advance(1d / 120, second.Update);
        }

        Assert.Equal(first.Y, second.Y, 9);
        Assert.Equal(first.VerticalVelocity, second.VerticalVelocity, 9);
    }

    [Fact]
    public void LongStallPerformsAtMostSixStepsAndDoesNotLeaveCatchUpDebt()
    {
        var loop = new FixedStepLoop();
        var steps = new List<double>();
        Assert.Equal(6, loop.Advance(30, steps.Add));
        Assert.All(steps, step => Assert.Equal(1d / 60, step));
        Assert.Equal(0, loop.Advance(0, steps.Add));
        Assert.Equal(1, loop.Advance(1d / 60, steps.Add));
        Assert.Equal(7, steps.Count);
    }

    [Fact]
    public void ResetDropsPartialTimeWhenFocusChanges()
    {
        var loop = new FixedStepLoop();
        loop.Advance(1d / 120, _ => throw new InvalidOperationException());
        loop.Reset();
        Assert.Equal(0, loop.Advance(1d / 120, _ => throw new InvalidOperationException()));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidElapsedDoesNotInvokeAnUpdate(double elapsed)
    {
        var loop = new FixedStepLoop();
        var calls = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => loop.Advance(elapsed, _ => calls++));
        Assert.Equal(0, calls);
    }
}
