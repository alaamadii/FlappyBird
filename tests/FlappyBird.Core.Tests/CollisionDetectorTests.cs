using FlappyBird.Core.Models;
using FlappyBird.Core.Physics;

namespace FlappyBird.Core.Tests;

public sealed class CollisionDetectorTests
{
    [Theory]
    [InlineData(10, 0, true)]
    [InlineData(10, 10, true)]
    [InlineData(9, 9, true)]
    [InlineData(10.001, 0, false)]
    [InlineData(0, 10.001, false)]
    public void EdgeCornerOverlapAndClearMissUseInclusiveContact(double x, double y, bool hit)
    {
        var result = CollisionDetector.ContactFraction(new(0, 0, 10, 10), 0, 0, new(x, y, 10, 10));
        Assert.Equal(hit, result.HasValue);
        if (hit) Assert.Equal(0, result!.Value);
    }

    [Fact]
    public void SweptCheckFindsThinObstacleBetweenDisjointEndpoints()
    {
        var hit = CollisionDetector.ContactFraction(new(0, 20, 1, 1), 100, 0, new(50, 10, 0.01, 30));
        Assert.NotNull(hit);
        Assert.Equal(0.49, hit.Value, 9);
    }

    [Fact]
    public void DiagonalAxesMustOverlapAtTheSameTime()
    {
        Assert.Equal(1, CollisionDetector.ContactFraction(new(0, 0, 10, 10), 10, 10, new(20, 20, 10, 10)));
        Assert.Null(CollisionDetector.ContactFraction(new(0, 0, 10, 10), 100, 10, new(20, 20, 10, 10)));
        Assert.Null(CollisionDetector.ContactFraction(new(0, 0, 10, 10), 9.9, 9.9, new(20, 20, 10, 10)));
    }
}
