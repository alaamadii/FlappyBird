using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using FlappyBird.App.Rendering;
using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

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

        RenderReady(renderer, graphics, image.Size);

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
        RenderReady(renderer, baselineGraphics, baseline.Size);
        using var image = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(image);

        RenderReady(renderer, graphics, image.Size);

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

        RenderReady(renderer, graphics, image.Size);

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

        RenderReady(renderer, graphics, new Size(width, height));

        Assert.Equal(Color.Magenta.ToArgb(), image.GetPixel(5, 5).ToArgb());
    }

    [Fact]
    public void RepeatedPaintingProducesTheSameStaticScene()
    {
        var renderer = new GameRenderer(new Playfield());
        var bird = new Bird(new Playfield(), new GameSettings());
        bird.Flap();
        bird.Update(0.1);
        var initialY = bird.Y;
        var initialVelocity = bird.VerticalVelocity;
        using var image = new Bitmap(480, 640);
        using var graphics = Graphics.FromImage(image);
        using var first = new MemoryStream();
        renderer.Render(graphics, image.Size, bird, GameState.Running, CreatePipes(), 0);
        image.Save(first, System.Drawing.Imaging.ImageFormat.Png);

        for (var paint = 0; paint < 20; paint++)
        {
            renderer.Render(graphics, image.Size, bird, GameState.Running, CreatePipes(), 0);
        }

        using var last = new MemoryStream();
        image.Save(last, System.Drawing.Imaging.ImageFormat.Png);
        Assert.Equal(first.ToArray(), last.ToArray());
        Assert.Equal(initialY, bird.Y);
        Assert.Equal(initialVelocity, bird.VerticalVelocity);
    }

    [Fact]
    public void RenderingFollowsTheBirdAndRemovesTheReadyPanelWhenRunning()
    {
        var playfield = new Playfield();
        var bird = new Bird(playfield, new GameSettings());
        var renderer = new GameRenderer(playfield);
        using var image = new Bitmap(480, 640);
        using var graphics = Graphics.FromImage(image);
        renderer.Render(graphics, image.Size, bird, GameState.Ready, CreatePipes(), 0);
        var readyPanel = image.GetPixel(100, 410);

        bird.Flap();
        bird.Update(0.1);
        renderer.Render(graphics, image.Size, bird, GameState.Running, CreatePipes(), 0);

        var body = image.GetPixel(240, (int)(bird.Y + bird.Height / 2));
        Assert.True(body.R > 200 && body.G > 150 && body.B < 100);
        Assert.NotEqual(readyPanel, image.GetPixel(100, 410));
    }

    [Fact]
    public void OffScreenBirdIsSafeToRenderWithoutChangingItsPosition()
    {
        var playfield = new Playfield();
        var bird = new Bird(playfield, new GameSettings());
        bird.Update(100);
        var initialY = bird.Y;
        using var image = new Bitmap(480, 640);
        using var graphics = Graphics.FromImage(image);

        new GameRenderer(playfield).Render(graphics, image.Size, bird, GameState.Running, CreatePipes(), 0);

        Assert.Equal(initialY, bird.Y);
        Assert.Equal(255, image.GetPixel(240, 500).A);
    }

    private static void RenderReady(GameRenderer renderer, Graphics graphics, Size size) =>
        renderer.Render(graphics, size, new Bird(new Playfield(), new GameSettings()), GameState.Ready, CreatePipes(), 0);

    private static IReadOnlyList<PipePair> CreatePipes() =>
        new PipeSystem(new Playfield(), new GameSettings(), new RandomGapSource(new Random(1))).Pairs;

    [Theory]
    [InlineData(480, 640)]
    [InlineData(600, 800)]
    [InlineData(720, 960)]
    public void ActualPipeGeometryRendersAtAllSceneScalesWithoutMutation(int width, int height)
    {
        var source = new CountingGapSource();
        var playfield = new Playfield();
        var engine = new GameEngine(playfield, new GameSettings(), source);
        engine.Flap();
        for (var step = 0; step < 3; step++) engine.Pipes.Update(1);
        engine.Bird.Reset();
        var snapshot = engine.Pipes.Pairs.Select(p => (p.X, p.GapTop)).ToArray();
        var calls = source.Calls;
        var renderer = new GameRenderer(playfield);
        using var image = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(image);
        for (var paint = 0; paint < 20; paint++)
        {
            renderer.Render(graphics, image.Size, engine.Bird, engine.State, engine.Pipes.Pairs, engine.Score);
        }

        var scale = width / 480f;
        foreach (var pair in engine.Pipes.Pairs)
        {
            var x = (int)((pair.X + pair.Width / 2) * scale);
            var upper = image.GetPixel(x, (int)(80 * scale));
            var lower = image.GetPixel(x, (int)((pair.Lower.Y + 60) * scale));
            var gap = image.GetPixel(x, (int)((pair.GapTop + pair.GapHeight * 0.85) * scale));
            Assert.True(upper.G > upper.R && upper.G > upper.B && upper.R < 140);
            Assert.True(lower.G > lower.R && lower.G > lower.B && lower.R < 140);
            Assert.NotEqual(upper, gap);
            var soil = image.GetPixel(x, (int)((playfield.GroundTop + 45) * scale));
            Assert.True(soil.R > soil.G && soil.G > soil.B);
        }

        Assert.Equal(snapshot, engine.Pipes.Pairs.Select(p => (p.X, p.GapTop)).ToArray());
        Assert.Equal(calls, source.Calls);
    }

    private sealed class CountingGapSource : IGapSource
    {
        public int Calls { get; private set; }
        public double NextGapTop(double minimumInclusive, double maximumExclusive)
        {
            Calls++;
            return (minimumInclusive + maximumExclusive) / 2;
        }
    }

    [Fact]
    public void ScoreAndGameOverPresentationChangeOnlyTheRenderedOutput()
    {
        var playfield = new Playfield();
        var bird = new Bird(playfield, new GameSettings());
        var pipes = CreatePipes();
        var renderer = new GameRenderer(playfield);
        using var image = new Bitmap(480, 640);
        using var graphics = Graphics.FromImage(image);
        byte[] Snapshot(GameState state, int score)
        {
            renderer.Render(graphics, image.Size, bird, state, pipes, score);
            using var data = new MemoryStream();
            image.Save(data, System.Drawing.Imaging.ImageFormat.Png);
            return data.ToArray();
        }

        var running = Snapshot(GameState.Running, 0);
        var scored = Snapshot(GameState.Running, 7);
        var over = Snapshot(GameState.GameOver, 7);
        Assert.False(running.SequenceEqual(scored));
        Assert.False(scored.SequenceEqual(over));
        Assert.Equal(over, Snapshot(GameState.GameOver, 7));
        Assert.Equal(0, bird.VerticalVelocity);
        Assert.All(pipes, pair => Assert.False(pair.HasScored));
    }

    [Theory]
    [InlineData(480, 640)]
    [InlineData(600, 800)]
    [InlineData(720, 960)]
    public void BestPauseAndStorageWarningAreRenderedWithoutChangingGameplay(int width, int height)
    {
        var engine = new GameEngine(new Playfield(), new GameSettings());
        engine.Flap();
        var pipes = engine.Pipes.Pairs.Select(p => (p.X, p.GapTop, p.HasScored)).ToArray();
        var bird = (engine.Bird.Y, engine.Bird.VerticalVelocity);
        var renderer = new GameRenderer(new Playfield());
        using var image = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(image);
        byte[] Snapshot(int best, bool paused = false, string? warning = null)
        {
            renderer.Render(graphics, image.Size, engine.Bird, engine.State, engine.Pipes.Pairs,
                engine.Score, best, paused, warning);
            using var data = new MemoryStream();
            image.Save(data, System.Drawing.Imaging.ImageFormat.Png);
            return data.ToArray();
        }
        var ordinary = Snapshot(0);
        Assert.False(ordinary.SequenceEqual(Snapshot(int.MaxValue)));
        Assert.False(ordinary.SequenceEqual(Snapshot(0, paused: true)));
        Assert.False(ordinary.SequenceEqual(Snapshot(0, warning: "Best score couldn't be saved.")));
        Assert.Equal(ordinary, Snapshot(0));
        Assert.Equal(bird, (engine.Bird.Y, engine.Bird.VerticalVelocity));
        Assert.Equal(pipes, engine.Pipes.Pairs.Select(p => (p.X, p.GapTop, p.HasScored)).ToArray());
        Assert.Equal(0, engine.Score);
        Assert.Equal(GameState.Running, engine.State);
    }
}
