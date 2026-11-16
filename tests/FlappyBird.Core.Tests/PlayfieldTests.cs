using FlappyBird.Core.Models;

namespace FlappyBird.Core.Tests;

public sealed class PlayfieldTests
{
    [Fact]
    public void DefaultPlayfieldLeavesSpaceAboveGround()
    {
        var playfield = new Playfield();

        Assert.Equal(480, playfield.Width);
        Assert.Equal(640, playfield.Height);
        Assert.Equal(80, playfield.GroundHeight);
        Assert.Equal(560, playfield.GroundTop);
    }

    [Fact]
    public void CustomPlayfieldComputesGroundBoundary()
    {
        var playfield = new Playfield(800, 600, 100);

        Assert.Equal(800, playfield.Width);
        Assert.Equal(600, playfield.Height);
        Assert.Equal(100, playfield.GroundHeight);
        Assert.Equal(500, playfield.GroundTop);
    }

    [Theory]
    [InlineData(0, 640, 80, "width")]
    [InlineData(-1, 640, 80, "width")]
    [InlineData(480, 0, 80, "height")]
    [InlineData(480, -1, 80, "height")]
    [InlineData(480, 640, 0, "groundHeight")]
    [InlineData(480, 640, -1, "groundHeight")]
    [InlineData(480, 640, 640, "groundHeight")]
    [InlineData(480, 640, 641, "groundHeight")]
    public void InvalidDimensionsAreRejected(int width, int height, int groundHeight, string parameter)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Playfield(width, height, groundHeight));

        Assert.Equal(parameter, exception.ParamName);
    }
}
