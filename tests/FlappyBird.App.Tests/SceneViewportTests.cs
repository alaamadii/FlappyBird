using System.Drawing;
using FlappyBird.App.Rendering;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Tests;

public sealed class SceneViewportTests
{
    [Theory]
    [InlineData(480, 640)]
    [InlineData(600, 800)]
    [InlineData(720, 960)]
    [InlineData(800, 800)]
    public void RestartButtonRemainsCenteredInsideGameOverPanel(int width, int height)
    {
        var playfield = new Playfield();
        var viewport = SceneViewport.Create(playfield, new Size(width, height));
        var button = viewport.Map(SceneViewport.RestartButton(playfield));
        var panel = viewport.Map(SceneViewport.GameOverPanel(playfield));
        Assert.True(panel.Contains(button));
        Assert.InRange(Math.Abs(button.Left + button.Width / 2f - width / 2f), 0, 1);
        Assert.True(button.Height >= 48);
    }
}
