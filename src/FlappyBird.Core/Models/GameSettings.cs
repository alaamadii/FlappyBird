namespace FlappyBird.Core.Models;

/// <summary>Immutable physics tuning in logical units and seconds.</summary>
public sealed class GameSettings
{
    public GameSettings(double gravity = 900, double flapSpeed = 320,
        double birdWidth = 86, double birdHeight = 48)
    {
        RequirePositiveFinite(gravity, nameof(gravity));
        RequirePositiveFinite(flapSpeed, nameof(flapSpeed));
        RequirePositiveFinite(birdWidth, nameof(birdWidth));
        RequirePositiveFinite(birdHeight, nameof(birdHeight));
        Gravity = gravity;
        FlapSpeed = flapSpeed;
        BirdWidth = birdWidth;
        BirdHeight = birdHeight;
    }

    public double Gravity { get; }
    public double FlapSpeed { get; }
    public double BirdWidth { get; }
    public double BirdHeight { get; }

    private static void RequirePositiveFinite(double value, string parameter)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameter, "Must be finite and positive.");
        }
    }
}
