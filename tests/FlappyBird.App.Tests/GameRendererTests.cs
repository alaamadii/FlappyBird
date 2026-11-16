using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using FlappyBird.App.Rendering;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Tests;

public sealed class GameRendererTests
{
    [Theory]
    [InlineData(480, 640)]
    [InlineData(600, 800)]
    [InlineData(720, 960)]
    public void GroundAndBirdRemainAlignedAtCommonDisplayScales(int width, int height)
    {
        var playfield = new Playfield();
        var renderer = new GameRenderer(playfield);
        using var image = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(image);

        renderer.Render(graphics, image.Size);

        var scale = width / (float)playfield.Width;
        var grass = image.GetPixel(width / 2, (int)((playfield.GroundTop + 6) * scale));
        var soil = image.GetPixel(width / 2, (int)((playfield.GroundTop + 45) * scale));
        var bird = image.GetPixel(width / 2, (int)(playfield.GroundTop * 0.51f * scale));
        Assert.True(grass.G > grass.R, "Grass should start at the shared ground boundary.");
        Assert.True(soil.R > soil.G && soil.G > soil.B, "Soil should remain below the grass.");
        Assert.True(bird.R > 200 && bird.G > 150 && bird.B < 100,
            "The yellow bird should stay at its logical starting position.");
        Assert.Equal(255, image.GetPixel(width - 1, height - 1).A);
    }

    [Theory]
    [InlineData(800, 800, 100, 0)]
    [InlineData(480, 800, 0, 80)]
    public void DifferentAspectRatiosCenterTheSceneWithoutStretching(
        int width, int height, int offsetX, int offsetY)
    {
        var renderer = new GameRenderer(new Playfield());
        using var baseline = new Bitmap(480, 640);
        using var baselineGraphics = Graphics.FromImage(baseline);
        renderer.Render(baselineGraphics, baseline.Size);
        using var image = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(image);

        renderer.Render(graphics, image.Size);

        var scale = Math.Min(width / 480f, height / 640f);
        Assert.Equal(image.GetPixel(0, 0), image.GetPixel(width - 1, height - 1));
        Assert.NotEqual(image.GetPixel(0, 0), image.GetPixel(width / 2, height / 2));
        // A flat area of soil must map to the same point inside the centered viewport.
        Assert.Equal(baseline.GetPixel(240, 605),
            image.GetPixel(offsetX + (int)(240 * scale), offsetY + (int)(605 * scale)));
    }

    [Fact]
    public void RenderRestoresTheCallersGraphicsState()
    {
        var renderer = new GameRenderer(new Playfield());
        using var image = new Bitmap(480, 640);
        using var graphics = Graphics.FromImage(image);
        graphics.TranslateTransform(7, 11);
        graphics.SetClip(new Rectangle(5, 6, 100, 200));
        graphics.SmoothingMode = SmoothingMode.None;
        graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
        using var originalTransform = graphics.Transform;
        var originalClip = graphics.ClipBounds;

        renderer.Render(graphics, image.Size);

        using var restoredTransform = graphics.Transform;
        Assert.Equal(originalTransform.Elements, restoredTransform.Elements);
        Assert.Equal(originalClip, graphics.ClipBounds);
        Assert.Equal(SmoothingMode.None, graphics.SmoothingMode);
        Assert.Equal(TextRenderingHint.SingleBitPerPixel, graphics.TextRenderingHint);
    }

    [Theory]
    [InlineData(0, 640)]
    [InlineData(480, 0)]
    public void EmptyClientAreaDoesNotDraw(int width, int height)
    {
        var renderer = new GameRenderer(new Playfield());
        using var image = new Bitmap(10, 10);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Magenta);

        renderer.Render(graphics, new Size(width, height));

        Assert.Equal(Color.Magenta.ToArgb(), image.GetPixel(5, 5).ToArgb());
    }

    [Fact]
    public void RepeatedPaintingProducesTheSameStaticScene()
    {
        var renderer = new GameRenderer(new Playfield());
        using var image = new Bitmap(480, 640);
        using var graphics = Graphics.FromImage(image);
        using var first = new MemoryStream();
        renderer.Render(graphics, image.Size);
        image.Save(first, System.Drawing.Imaging.ImageFormat.Png);

        for (var paint = 0; paint < 20; paint++)
        {
            renderer.Render(graphics, image.Size);
        }

        using var last = new MemoryStream();
        image.Save(last, System.Drawing.Imaging.ImageFormat.Png);
        Assert.Equal(first.ToArray(), last.ToArray());
    }
}
