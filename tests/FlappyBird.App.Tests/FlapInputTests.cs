using FlappyBird.App.Input;

namespace FlappyBird.App.Tests;

public sealed class FlapInputTests
{
    [Fact]
    public void HoldAndAutoRepeatProduceOnlyOnePressUntilRelease()
    {
        var input = new FlapInput();
        Assert.True(input.Press());
        Assert.False(input.Press());
        Assert.False(input.Press());
        input.Release();
        Assert.True(input.Press());
    }

    [Fact]
    public void SpaceHeldAcrossFocusRequiresReleaseBeforeAnotherFlap()
    {
        var input = new FlapInput();
        input.Press();
        input.Suspend();
        Assert.False(input.Press());
        input.Resume(spaceHeld: true);
        Assert.False(input.Press());
        Assert.False(input.Press());
        input.Release();
        Assert.True(input.Press());
    }

    [Fact]
    public void ReleasedWhileAwayAllowsTheNextFreshPress()
    {
        var input = new FlapInput();
        input.Press();
        input.Suspend();
        input.Resume(spaceHeld: false);
        Assert.True(input.Press());
        Assert.False(input.Press());
    }
}
