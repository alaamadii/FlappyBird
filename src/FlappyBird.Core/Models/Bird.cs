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
        var next = Predict(elapsedSeconds);
        Y = next.Y;
        VerticalVelocity = next.Velocity;
    }

    internal (double Y, double Velocity) Predict(double elapsedSeconds)
    {
        ValidateElapsed(elapsedSeconds);
        if (elapsedSeconds == 0)
        {
            return (Y, VerticalVelocity);
        }

        // Semi-implicit Euler: update velocity first, then position using that velocity.
        var velocity = VerticalVelocity + settings.Gravity * elapsedSeconds;
        if (!double.IsFinite(velocity))
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Update would overflow bird motion.");
        }
        velocity = Math.Min(velocity, settings.MaxFallSpeed);
        var position = Y + velocity * elapsedSeconds;
        if (!double.IsFinite(position))
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Update would overflow bird motion.");
        }

        return (Math.Max(0, position), position <= 0 && velocity < 0 ? 0 : velocity);
    }

    internal void StopAt(double y)
    {
        Y = y;
        VerticalVelocity = 0;
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
