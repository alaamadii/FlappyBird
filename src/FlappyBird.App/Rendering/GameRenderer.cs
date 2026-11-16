using System.Drawing.Drawing2D;
using System.Drawing.Text;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Rendering;

/// <summary>Draws a static preview in logical playfield units without changing domain state.</summary>
public sealed class GameRenderer(Playfield playfield)
{
    private readonly Playfield playfield = playfield ?? throw new ArgumentNullException(nameof(playfield));

    public void Render(Graphics graphics, Size clientSize)
    {
        ArgumentNullException.ThrowIfNull(graphics);
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
            graphics.Clear(Color.FromArgb(28, 57, 65));

            // Preserve the playfield's aspect ratio; any unused space becomes a border.
            var scale = Math.Min((float)clientSize.Width / playfield.Width,
                (float)clientSize.Height / playfield.Height);
            graphics.TranslateTransform((clientSize.Width - playfield.Width * scale) / 2,
                (clientSize.Height - playfield.Height * scale) / 2);
            graphics.ScaleTransform(scale, scale);
            graphics.SetClip(new Rectangle(0, 0, playfield.Width, playfield.Height), CombineMode.Intersect);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            DrawBackground(graphics);
            DrawHeading(graphics);
            DrawBird(graphics, playfield.Width / 2f, playfield.GroundTop * 0.51f);
            DrawReadyPanel(graphics);
            DrawGround(graphics);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private void DrawBackground(Graphics graphics)
    {
        using var sky = new LinearGradientBrush(new Point(0, 0), new Point(0, playfield.GroundTop),
            Color.FromArgb(150, 215, 228), Color.FromArgb(225, 245, 229));
        graphics.FillRectangle(sky, 0, 0, playfield.Width, playfield.GroundTop);

        DrawCloud(graphics, 40, 195, 1);
        DrawCloud(graphics, playfield.Width - 140, 240, 0.8f);
        using var distantHill = new SolidBrush(Color.FromArgb(162, 204, 174));
        using var nearHill = new SolidBrush(Color.FromArgb(129, 185, 153));
        graphics.FillEllipse(distantHill, -90, playfield.GroundTop - 55, 340, 125);
        graphics.FillEllipse(distantHill, playfield.Width - 240, playfield.GroundTop - 75, 350, 145);
        graphics.FillEllipse(nearHill, 80, playfield.GroundTop - 30, 330, 90);
    }

    private static void DrawCloud(Graphics graphics, float x, float y, float scale)
    {
        using var cloud = new SolidBrush(Color.FromArgb(180, Color.White));
        using var shape = new GraphicsPath(FillMode.Winding);
        shape.AddEllipse(x, y + 14 * scale, 56 * scale, 30 * scale);
        shape.AddEllipse(x + 25 * scale, y, 47 * scale, 47 * scale);
        shape.AddEllipse(x + 55 * scale, y + 15 * scale, 55 * scale, 28 * scale);
        graphics.FillPath(cloud, shape);
    }

    private void DrawHeading(Graphics graphics)
    {
        DrawText(graphics, "YMH219  /  NESNE TABANLI PROGRAMLAMA", 11, FontStyle.Bold,
            Color.FromArgb(46, 92, 103), new RectangleF(20, 30, playfield.Width - 40, 24));
        DrawText(graphics, "FLAPPY BIRD", 42, FontStyle.Bold,
            Color.FromArgb(29, 79, 83), new RectangleF(20, 76, playfield.Width - 40, 60));
        DrawText(graphics, "A little bird. A big adventure.", 15, FontStyle.Regular,
            Color.FromArgb(39, 85, 96), new RectangleF(20, 140, playfield.Width - 40, 28));
    }

    private static void DrawBird(Graphics graphics, float x, float y)
    {
        using var outline = new Pen(Color.FromArgb(89, 64, 41), 2.5f) { LineJoin = LineJoin.Round };
        using var body = new SolidBrush(Color.FromArgb(255, 207, 70));
        using var wing = new SolidBrush(Color.FromArgb(244, 166, 47));
        using var beak = new SolidBrush(Color.FromArgb(239, 119, 66));
        using var eye = new SolidBrush(Color.FromArgb(255, 255, 241));
        using var pupil = new SolidBrush(Color.FromArgb(50, 55, 54));
        using var shadow = new SolidBrush(Color.FromArgb(30, 44, 94, 81));

        graphics.FillEllipse(shadow, x - 32, y + 43, 64, 9);
        PointF[] tail = [new(x - 25, y - 3), new(x - 42, y - 12), new(x - 35, y + 13)];
        graphics.FillPolygon(wing, tail);
        graphics.DrawPolygon(outline, tail);
        graphics.FillEllipse(body, x - 30, y - 24, 60, 48);
        graphics.DrawEllipse(outline, x - 30, y - 24, 60, 48);
        graphics.FillEllipse(wing, x - 25, y, 28, 18);
        graphics.DrawEllipse(outline, x - 25, y, 28, 18);
        graphics.FillEllipse(eye, x + 7, y - 17, 20, 23);
        graphics.DrawEllipse(outline, x + 7, y - 17, 20, 23);
        graphics.FillEllipse(pupil, x + 19, y - 11, 6, 10);
        PointF[] beakPoints = [new(x + 24, y + 1), new(x + 43, y + 7), new(x + 24, y + 13)];
        graphics.FillPolygon(beak, beakPoints);
        graphics.DrawPolygon(outline, beakPoints);
    }

    private void DrawReadyPanel(Graphics graphics)
    {
        var panel = new RectangleF(48, playfield.GroundTop - 170, playfield.Width - 96, 122);
        using var background = new SolidBrush(Color.FromArgb(245, 255, 255, 244));
        using var border = new Pen(Color.FromArgb(191, 215, 191), 1.5f);
        graphics.FillRoundedRectangle(background, panel, new SizeF(16, 16));
        graphics.DrawRoundedRectangle(border, panel, new SizeF(16, 16));
        DrawText(graphics, "READY", 25, FontStyle.Bold, Color.FromArgb(37, 86, 76),
            new RectangleF(panel.X, panel.Y + 14, panel.Width, 36));
        DrawText(graphics, "Take a moment. Enjoy the view.", 14, FontStyle.Regular,
            Color.FromArgb(64, 99, 91), new RectangleF(panel.X + 8, panel.Y + 58, panel.Width - 16, 24));
        DrawText(graphics, "Controls arrive in Stage 02.", 12, FontStyle.Regular,
            Color.FromArgb(64, 99, 91), new RectangleF(panel.X + 8, panel.Y + 86, panel.Width - 16, 22));
    }

    private void DrawGround(Graphics graphics)
    {
        using var soil = new SolidBrush(Color.FromArgb(229, 201, 151));
        using var grass = new SolidBrush(Color.FromArgb(108, 163, 101));
        using var highlight = new SolidBrush(Color.FromArgb(173, 208, 126));
        using var stripe = new Pen(Color.FromArgb(211, 181, 132), 2);
        graphics.FillRectangle(soil, 0, playfield.GroundTop, playfield.Width, playfield.GroundHeight);
        graphics.FillRectangle(grass, 0, playfield.GroundTop, playfield.Width, 12);
        graphics.FillRectangle(highlight, 0, playfield.GroundTop, playfield.Width, 4);
        for (var x = -20; x < playfield.Width; x += 24)
        {
            graphics.DrawLine(stripe, x, playfield.GroundTop + 24, x + 14, playfield.GroundTop + 38);
        }

        DrawText(graphics, "STAGE 01  /  SCENE PREVIEW", 10, FontStyle.Bold,
            Color.FromArgb(111, 88, 58), new RectangleF(20, playfield.Height - 25, playfield.Width - 40, 18));
    }

    private static void DrawText(Graphics graphics, string text, float size, FontStyle style,
        Color color, RectangleF bounds)
    {
        // Pixel units are transformed with the scene, avoiding a second DPI/point conversion.
        using var font = new Font("Segoe UI", size, style, GraphicsUnit.Pixel);
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
