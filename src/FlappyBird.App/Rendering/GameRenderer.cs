using System.Drawing.Drawing2D;
using System.Drawing.Text;
using FlappyBird.App.Animation;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Rendering;

/// <summary>Draws the current bird and state in logical units without advancing the game.</summary>
public sealed class GameRenderer(Playfield playfield)
{
    private readonly Playfield playfield = playfield ?? throw new ArgumentNullException(nameof(playfield));

    public void Render(Graphics graphics, Size clientSize, Bird bird, GameState gameState,
        IReadOnlyList<PipePair> pipes, int score, int bestScore = 0,
        bool paused = false, string? storageMessage = null, SceneAnimationFrame? animation = null)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(bird);
        ArgumentNullException.ThrowIfNull(pipes);
        if (clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            return;
        }

        var state = graphics.Save();
        try
        {
            graphics.ResetTransform();
            graphics.PageUnit = GraphicsUnit.Pixel;
            graphics.PageScale = 1;
            graphics.Clear(Color.FromArgb(8, 18, 31));

            // Preserve the playfield's aspect ratio; any unused space becomes a border.
            var viewport = SceneViewport.Create(playfield, clientSize);
            graphics.TranslateTransform(viewport.OffsetX, viewport.OffsetY);
            graphics.ScaleTransform(viewport.Scale, viewport.Scale);
            graphics.SetClip(new Rectangle(0, 0, playfield.Width, playfield.Height), CombineMode.Intersect);
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            var frame = animation ?? SceneAnimationFrame.Still;
            DrawBackground(graphics, frame.BackdropOffset);
            DrawPipes(graphics, pipes);
            DrawCurrentBird(graphics, bird, frame);
            DrawHeading(graphics, gameState, score, bestScore);
            if (gameState == GameState.Ready)
            {
                DrawReadyPanel(graphics);
            }
            DrawGround(graphics, storageMessage, frame.GroundOffset);
            if (gameState == GameState.GameOver) DrawGameOver(graphics, score, bestScore, storageMessage);
            else if (gameState == GameState.Running && paused) DrawPause(graphics);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private void DrawPipes(Graphics graphics, IReadOnlyList<PipePair> pipes)
    {
        foreach (var pair in pipes)
        {
            if (pair.Right <= 0 || pair.X >= playfield.Width)
            {
                continue;
            }

            DrawPipe(graphics, pair.Upper, upper: true);
            DrawPipe(graphics, pair.Lower, upper: false);
        }
    }

    private static void DrawPipe(Graphics graphics, PipeRectangle pipe, bool upper)
    {
        var bounds = new RectangleF((float)pipe.X, (float)pipe.Y, (float)pipe.Width, (float)pipe.Height);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var inset = bounds.Width * 0.08f;
        var capHeight = Math.Min(18, bounds.Height / 3);
        var stroke = Math.Min(2, Math.Min(bounds.Width, bounds.Height) / 8);
        using var body = new SolidBrush(Color.FromArgb(72, 152, 73));
        using var light = new SolidBrush(Color.FromArgb(151, 215, 91));
        using var dark = new SolidBrush(Color.FromArgb(36, 91, 61));
        using var outline = new Pen(Color.FromArgb(20, 49, 46), stroke);
        graphics.FillRectangle(body, bounds.X + inset, bounds.Y, bounds.Width - 2 * inset, bounds.Height);
        graphics.FillRectangle(light, bounds.X + inset + stroke, bounds.Y, Math.Max(1, bounds.Width / 5), bounds.Height);
        graphics.FillRectangle(dark, bounds.Right - inset - bounds.Width / 6, bounds.Y, bounds.Width / 6, bounds.Height);
        graphics.DrawRectangle(outline, bounds.X + inset, bounds.Y + stroke / 2,
            bounds.Width - 2 * inset, bounds.Height - stroke);
        var cap = new RectangleF(bounds.X, upper ? bounds.Bottom - capHeight : bounds.Y, bounds.Width, capHeight);
        graphics.FillRectangle(body, cap);
        graphics.FillRectangle(light, cap.X + stroke, cap.Y + stroke, cap.Width - 2 * stroke, Math.Min(4, cap.Height / 3));
        graphics.DrawRectangle(outline, cap.X + stroke / 2, cap.Y + stroke / 2,
            cap.Width - stroke, cap.Height - stroke);
    }

    private void DrawBackground(Graphics graphics, float offset)
    {
        using var sky = new SolidBrush(Color.FromArgb(16, 42, 63));
        graphics.FillRectangle(sky, 0, 0, playfield.Width, playfield.GroundTop);
        using var star = new SolidBrush(Color.FromArgb(125, 177, 182));
        for (var index = 0; index < 20; index++)
        {
            var x = ((index * 73 + 37 - offset) % playfield.Width + playfield.Width) % playfield.Width;
            graphics.FillRectangle(star, (float)Math.Floor(x), 24 + index * 43 % 340, 3, 3);
        }
        using var moon = new SolidBrush(Color.FromArgb(255, 222, 142));
        graphics.FillRectangle(moon, playfield.Width - 90, 210, 28, 28);
        graphics.FillRectangle(moon, playfield.Width - 94, 214, 36, 20);
        graphics.FillRectangle(sky, playfield.Width - 78, 206, 24, 24);
        var firstCloud = ((40 - offset) % playfield.Width + playfield.Width) % playfield.Width;
        var secondCloud = ((playfield.Width - 140 - offset) % playfield.Width + playfield.Width) % playfield.Width;
        DrawCloud(graphics, firstCloud, 195, 1);
        DrawCloud(graphics, firstCloud - playfield.Width, 195, 1);
        DrawCloud(graphics, secondCloud, 240, 0.8f);
        DrawCloud(graphics, secondCloud - playfield.Width, 240, 0.8f);
        using var distantHill = new SolidBrush(Color.FromArgb(35, 82, 91));
        using var nearHill = new SolidBrush(Color.FromArgb(47, 112, 104));
        for (var x = -240; x < playfield.Width + 480; x += 80)
        {
            var height = 36 + (x / 80 + 3) % 3 * 20;
            graphics.FillRectangle(distantHill, x - offset % 240, playfield.GroundTop - height - 36, 80, height + 36);
            graphics.FillRectangle(nearHill, x + 24 - offset * 0.5f % 240, playfield.GroundTop - height / 2, 80, height / 2);
        }
    }

    private static void DrawCloud(Graphics graphics, float x, float y, float scale)
    {
        using var cloud = new SolidBrush(Color.FromArgb(62, 113, 126));
        graphics.FillRectangle(cloud, (float)Math.Floor(x), y + 16 * scale, 104 * scale, 16 * scale);
        graphics.FillRectangle(cloud, (float)Math.Floor(x + 20 * scale), y + 8 * scale, 60 * scale, 24 * scale);
        graphics.FillRectangle(cloud, (float)Math.Floor(x + 32 * scale), y, 32 * scale, 16 * scale);
    }

    private void DrawHeading(Graphics graphics, GameState gameState, int score, int bestScore)
    {
        if (gameState != GameState.Ready)
        {
            var cardWidth = (playfield.Width - 140) / 2f;
            DrawScoreCard(graphics, "SCORE", score, new RectangleF(12, 10, cardWidth, 52));
            DrawScoreCard(graphics, "BEST", bestScore, new RectangleF(playfield.Width - cardWidth - 12, 10, cardWidth, 52));
            return;
        }

        DrawText(graphics, "8-BIT FLIGHT CLUB", 12, FontStyle.Bold,
            Color.FromArgb(155, 210, 183), new RectangleF(20, 30, playfield.Width - 40, 24));
        DrawText(graphics, "FLAPPY BIRD", 42, FontStyle.Bold,
            Color.FromArgb(255, 207, 70), new RectangleF(20, 76, playfield.Width - 40, 60));
        DrawText(graphics, "Fly through the gaps. Beat your best.", 15, FontStyle.Regular,
            Color.FromArgb(217, 238, 211), new RectangleF(20, 140, playfield.Width - 40, 28));
        DrawText(graphics, $"BEST  {bestScore}", 16, FontStyle.Bold, Color.FromArgb(155, 210, 183),
            new RectangleF(80, 176, playfield.Width - 160, 30));
    }

    private static void DrawScoreCard(Graphics graphics, string label, int score, RectangleF bounds)
    {
        using var background = new SolidBrush(Color.FromArgb(255, 255, 244));
        using var border = new Pen(Color.FromArgb(96, 144, 126), 1);
        graphics.FillRectangle(background, bounds);
        graphics.DrawRectangle(border, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        DrawText(graphics, label, 11, FontStyle.Bold, Color.FromArgb(64, 99, 91),
            new RectangleF(bounds.X + 6, bounds.Y + 3, bounds.Width - 12, 17));
        DrawText(graphics, score.ToString(), 22, FontStyle.Bold, Color.FromArgb(37, 86, 76),
            new RectangleF(bounds.X + 6, bounds.Y + 20, bounds.Width - 12, 29));
    }

    private void DrawCurrentBird(Graphics graphics, Bird bird, SceneAnimationFrame frame)
    {
        // Keep painting safe for independently supplied diagnostic model states too.
        if (bird.Y > playfield.Height)
        {
            return;
        }

        var state = graphics.Save();
        try
        {
            graphics.TranslateTransform((float)(bird.X + bird.Width / 2), (float)(bird.Y + bird.Height / 2) + frame.BobOffset);
            graphics.RotateTransform(frame.TiltDegrees);
            graphics.ScaleTransform((float)(bird.Width / 86 * 0.9), (float)(bird.Height / 48 * 0.9));
            DrawBird(graphics, frame.WingFrame);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private static void DrawBird(Graphics graphics, int wingFrame)
    {
        using var outline = new SolidBrush(Color.FromArgb(27, 36, 51));
        using var body = new SolidBrush(Color.FromArgb(255, 207, 70));
        using var wing = new SolidBrush(Color.FromArgb(244, 166, 47));
        using var beak = new SolidBrush(Color.FromArgb(239, 119, 66));
        using var eye = new SolidBrush(Color.FromArgb(255, 255, 241));
        using var light = new SolidBrush(Color.FromArgb(255, 232, 137));
        void Block(Brush brush, int x, int y, int width, int height) =>
            graphics.FillRectangle(brush, -42 + x * 4, -24 + y * 4, width * 4, height * 4);
        Block(outline, 0, 5, 4, 3);
        Block(outline, 3, 3, 14, 7);
        Block(outline, 5, 1, 10, 10);
        Block(outline, 7, 0, 6, 12);
        Block(body, 1, 6, 3, 1);
        Block(body, 4, 3, 12, 6);
        Block(body, 6, 2, 8, 8);
        Block(body, 8, 1, 4, 10);
        Block(light, 6, 2, 6, 1);
        Block(outline, 12, 2, 5, 5);
        Block(eye, 13, 3, 3, 3);
        Block(outline, 15, 3, 1, 2);
        Block(outline, 16, 5, 5, 3);
        Block(beak, 16, 6, 4, 1);
        var wingX = wingFrame == 2 ? 5 : 4;
        var wingY = wingFrame == 0 ? 1 : wingFrame == 2 ? 8 : 5;
        var wingWidth = wingFrame == 1 ? 5 : 4;
        var wingHeight = wingFrame == 0 ? 4 : 3;
        Block(outline, wingX, wingY, wingWidth, wingHeight);
        Block(wing, wingX + 1, wingY + 1, wingWidth - 2, wingHeight - 2);
    }

    private void DrawReadyPanel(Graphics graphics)
    {
        var panel = new RectangleF(48, playfield.GroundTop - 170, playfield.Width - 96, 122);
        using var background = new SolidBrush(Color.FromArgb(255, 255, 244));
        using var border = new Pen(Color.FromArgb(191, 215, 191), 1.5f);
        graphics.FillRectangle(background, panel);
        graphics.DrawRectangle(border, panel.X, panel.Y, panel.Width, panel.Height);
        DrawText(graphics, "READY TO FLY?", 25, FontStyle.Bold, Color.FromArgb(37, 86, 76),
            new RectangleF(panel.X, panel.Y + 14, panel.Width, 36));
        DrawText(graphics, "Press Space to start", 16, FontStyle.Bold,
            Color.FromArgb(64, 99, 91), new RectangleF(panel.X + 8, panel.Y + 58, panel.Width - 16, 24));
        DrawText(graphics, "Tap, release, repeat to stay in the air.", 13, FontStyle.Regular,
            Color.FromArgb(64, 99, 91), new RectangleF(panel.X + 8, panel.Y + 86, panel.Width - 16, 22));
    }

    private void DrawGround(Graphics graphics, string? storageMessage, float offset)
    {
        using var soil = new SolidBrush(Color.FromArgb(229, 201, 151));
        using var grass = new SolidBrush(Color.FromArgb(108, 163, 101));
        using var highlight = new SolidBrush(Color.FromArgb(173, 208, 126));
        using var stripe = new SolidBrush(Color.FromArgb(184, 141, 91));
        graphics.FillRectangle(soil, 0, playfield.GroundTop, playfield.Width, playfield.GroundHeight);
        graphics.FillRectangle(grass, 0, playfield.GroundTop, playfield.Width, 12);
        graphics.FillRectangle(highlight, 0, playfield.GroundTop, playfield.Width, 4);
        for (var x = -20; x < playfield.Width; x += 24)
        {
            graphics.FillRectangle(stripe, x - offset, playfield.GroundTop + 24, 12, 8);
            graphics.FillRectangle(stripe, x + 12 - offset, playfield.GroundTop + 32, 12, 6);
        }

        DrawText(graphics, "Space: flap  /  Enter: new game after a loss", 11, FontStyle.Bold,
            Color.FromArgb(91, 70, 42), new RectangleF(20, playfield.Height - 40, playfield.Width - 40, 18));
        if (storageMessage is not null)
        {
            DrawText(graphics, storageMessage, 11, FontStyle.Bold, Color.FromArgb(116, 52, 32),
                new RectangleF(20, playfield.Height - 21, playfield.Width - 40, 17));
        }
    }

    private void DrawGameOver(Graphics graphics, int score, int bestScore, string? storageMessage)
    {
        using var veil = new SolidBrush(Color.FromArgb(100, 28, 57, 65));
        graphics.FillRectangle(veil, 0, 0, playfield.Width, playfield.Height);
        var panel = SceneViewport.GameOverPanel(playfield);
        using var background = new SolidBrush(Color.FromArgb(255, 255, 244));
        using var border = new Pen(Color.FromArgb(191, 215, 191), 1.5f);
        graphics.FillRectangle(background, panel);
        graphics.DrawRectangle(border, panel.X, panel.Y, panel.Width, panel.Height);
        var ink = Color.FromArgb(37, 86, 76);
        DrawText(graphics, "GAME OVER", 28, FontStyle.Bold, ink,
            new RectangleF(panel.X, panel.Y + 18, panel.Width, 42));
        DrawText(graphics, "Final score", 15, FontStyle.Regular, ink,
            new RectangleF(panel.X, panel.Y + 78, panel.Width, 24));
        DrawText(graphics, score.ToString(), 44, FontStyle.Bold, ink,
            new RectangleF(panel.X, panel.Y + 104, panel.Width, 62));
        DrawText(graphics, $"Best: {bestScore}", 16, FontStyle.Bold, ink,
            new RectangleF(panel.X, panel.Y + 163, panel.Width, 26));
        if (storageMessage is not null)
        {
            DrawText(graphics, storageMessage, 11, FontStyle.Regular, Color.FromArgb(116, 52, 32),
                new RectangleF(panel.X + 10, panel.Y + 190, panel.Width - 20, 18));
        }
        DrawText(graphics, "Press Enter or click New game.", 12, FontStyle.Regular, ink,
            new RectangleF(panel.X, panel.Bottom - 32, panel.Width, 24));
        // The native button occupies SceneViewport.RestartButton; painting does not own input.
    }

    private void DrawPause(Graphics graphics)
    {
        using var veil = new SolidBrush(Color.FromArgb(90, 28, 57, 65));
        graphics.FillRectangle(veil, 0, 0, playfield.Width, playfield.Height);
        var panel = new RectangleF(48, 230, playfield.Width - 96, 150);
        using var background = new SolidBrush(Color.FromArgb(255, 255, 244));
        graphics.FillRectangle(background, panel);
        var ink = Color.FromArgb(37, 86, 76);
        DrawText(graphics, "PAUSED", 28, FontStyle.Bold, ink, new RectangleF(panel.X, panel.Y + 16, panel.Width, 40));
        DrawText(graphics, "Return to this window to continue.", 14, FontStyle.Regular, ink,
            new RectangleF(panel.X + 10, panel.Y + 70, panel.Width - 20, 24));
        DrawText(graphics, "Release Space before your next flap.", 13, FontStyle.Regular, ink,
            new RectangleF(panel.X + 10, panel.Y + 104, panel.Width - 20, 24));
    }

    private static void DrawText(Graphics graphics, string text, float size, FontStyle style,
        Color color, RectangleF bounds)
    {
        if (style == FontStyle.Bold && size >= 16)
        {
            PixelFont.Draw(graphics, text, size, color, bounds);
            return;
        }
        // Small instructions retain a system monospace font for legibility.
        using var font = new Font("Consolas", size, style, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(color);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.EllipsisCharacter,
        };
        graphics.DrawString(text, font, brush, bounds, format);
    }
}
