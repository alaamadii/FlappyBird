namespace FlappyBird.Core.Models;

/// <summary>Owns bird motion. X/Y identify the top-left of the logical bird rectangle.</summary>
public sealed class Bird
{
    private readonly GameSettings settings;
    private readonly double initialX;
    private readonly double initialY;

    public Bird(Playfield playfield, GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(playfield);
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.BirdWidth > playfield.Width || settings.BirdHeight > playfield.GroundTop)
        {
            throw new ArgumentException("Bird dimensions must fit above the ground.", nameof(settings));
        }

        this.settings = settings;
        initialX = (playfield.Width - Width) / 2;
        initialY = Math.Clamp(playfield.GroundTop * 0.51 - Height / 2, 0, playfield.GroundTop - Height);
        Reset();
    }

    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width => settings.BirdWidth;
    public double Height => settings.BirdHeight;
    public double VerticalVelocity { get; private set; }

    public void Flap() => VerticalVelocity = -settings.FlapSpeed;

    public void Update(double elapsedSeconds)
    {
        ValidateElapsed(elapsedSeconds);
        if (elapsedSeconds == 0)
        {
            return;
        }

        // Semi-implicit Euler: update velocity first, then position using that velocity.
        var velocity = VerticalVelocity + settings.Gravity * elapsedSeconds;
        var position = Y + velocity * elapsedSeconds;
        if (!double.IsFinite(velocity) || !double.IsFinite(position))
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Update would overflow bird motion.");
        }

        Y = Math.Max(0, position);
        VerticalVelocity = position <= 0 && velocity < 0 ? 0 : velocity;
    }

    public void Reset()
    {
        X = initialX;
        Y = initialY;
        VerticalVelocity = 0;
    }

    internal static void ValidateElapsed(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Must be finite and nonnegative.");
        }
    }
}
