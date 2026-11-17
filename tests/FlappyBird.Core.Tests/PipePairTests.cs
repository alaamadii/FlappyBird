using FlappyBird.Core.Models;

namespace FlappyBird.Core.Tests;

public sealed class PipePairTests
{
    [Fact]
    public void GeometrySharesXAndWidthAndEndsAtGround()
    {
        var pair = new PipePair(new Playfield(), new GameSettings(), 300, 100);
        Assert.Equal(new PipeRectangle(300, 0, 64, 100), pair.Upper);
        Assert.Equal(new PipeRectangle(300, 280, 64, 280), pair.Lower);
        Assert.Equal(560, pair.Lower.Bottom);
        Assert.Equal(180, pair.Lower.Y - pair.Upper.Bottom);
        pair.Move(20);
        Assert.Equal(280, pair.Upper.X);
        Assert.Equal(344, pair.Right);
        pair.Reset(600, 200);
        Assert.Equal(600, pair.X);
        Assert.Equal(200, pair.Upper.Height);
    }

    [Fact]
    public void GapBoundsIncludeMinimumAndExcludeMaximum()
    {
        var pair = new PipePair(new Playfield(), new GameSettings(), 0, 40);
        pair.Reset(0, Math.BitDecrement(340));
        Assert.True(pair.Lower.Height >= 40);
        Assert.Throws<ArgumentOutOfRangeException>(() => pair.Reset(0, 340));
        Assert.Throws<ArgumentOutOfRangeException>(() => pair.Reset(0, 39));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidGeometryIsRejectedBeforeMutation(double invalid)
    {
        var pair = new PipePair(new Playfield(), new GameSettings(), 300, 100);
        Assert.Throws<ArgumentOutOfRangeException>(() => pair.Reset(invalid, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => pair.Reset(0, invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => pair.Move(invalid));
        Assert.Equal(300, pair.X);
        Assert.Equal(100, pair.GapTop);
    }

    [Fact]
    public void NegativeDistanceCannotReverseMovement()
    {
        var pair = new PipePair(new Playfield(), new GameSettings(), 300, 100);
        Assert.Throws<ArgumentOutOfRangeException>(() => pair.Move(-1));
    }
}
