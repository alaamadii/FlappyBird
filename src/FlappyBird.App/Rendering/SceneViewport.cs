using FlappyBird.Core.Models;

namespace FlappyBird.App.Rendering;

/// <summary>One shared logical-to-client mapping for painted content and native controls.</summary>
public readonly record struct SceneViewport(float Scale, float OffsetX, float OffsetY)
{
    public static SceneViewport Create(Playfield playfield, Size client)
    {
        if (client.Width <= 0 || client.Height <= 0) return new(0, 0, 0);
        var scale = Math.Min(client.Width / (float)playfield.Width, client.Height / (float)playfield.Height);
        return new(scale, (client.Width - playfield.Width * scale) / 2,
            (client.Height - playfield.Height * scale) / 2);
    }

    public Rectangle Map(RectangleF logical) => Rectangle.Round(new RectangleF(
        OffsetX + logical.X * Scale, OffsetY + logical.Y * Scale,
        logical.Width * Scale, logical.Height * Scale));

    public static RectangleF GameOverPanel(Playfield field) => new(48, 178, field.Width - 96, 300);
    public static RectangleF RestartButton(Playfield field) => new((field.Width - 220) / 2f, 390, 220, 48);
}
