namespace FlappyBird.Core.Models;

/// <summary>Encapsulates the shared position and gap of an upper/lower pipe pair.</summary>
public sealed class PipePair
{
    private readonly GameSettings settings;
    private readonly int groundTop;

    public PipePair(Playfield playfield, GameSettings settings, double x, double gapTop)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.ValidateFor(playfield);
        this.settings = settings;
        groundTop = playfield.GroundTop;
        Reset(x, gapTop);
    }

    public double X { get; private set; }
    public double Width => settings.PipeWidth;
    public double Right => X + Width;
    public double GapTop { get; private set; }
    public double GapHeight => settings.PipeGapHeight;
    public PipeRectangle Upper => new(X, 0, Width, GapTop);
    public PipeRectangle Lower => new(X, GapTop + GapHeight, Width, groundTop - GapTop - GapHeight);

    public void Move(double distance)
    {
        if (!double.IsFinite(distance) || distance < 0 || !double.IsFinite(X - distance))
        {
            throw new ArgumentOutOfRangeException(nameof(distance));
        }

        X -= distance;
    }

    public void Reset(double x, double gapTop)
    {
        if (!double.IsFinite(x) || !double.IsFinite(x + Width))
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }

        if (!double.IsFinite(gapTop) || gapTop < settings.GapMargin
            || gapTop >= groundTop - settings.GapMargin - GapHeight)
        {
            throw new ArgumentOutOfRangeException(nameof(gapTop));
        }

        X = x;
        GapTop = gapTop;
    }
}
