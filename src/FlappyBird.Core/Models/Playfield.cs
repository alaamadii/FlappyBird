namespace FlappyBird.Core.Models;

/// <summary>Immutable logical dimensions shared by the game and its presentation.</summary>
public sealed class Playfield
{
    public const int DefaultWidth = 480;
    public const int DefaultHeight = 640;
    public const int DefaultGroundHeight = 80;

    public Playfield(
        int width = DefaultWidth,
        int height = DefaultHeight,
        int groundHeight = DefaultGroundHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groundHeight);
        if (groundHeight >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(groundHeight),
                "The ground must leave space above it for the play area.");
        }

        Width = width;
        Height = height;
        GroundHeight = groundHeight;
    }

    public int Width { get; }
    public int Height { get; }
    public int GroundHeight { get; }
    public int GroundTop => Height - GroundHeight;
}
