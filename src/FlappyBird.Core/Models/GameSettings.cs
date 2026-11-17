namespace FlappyBird.Core.Models;

/// <summary>Immutable flight and obstacle tuning in logical units and seconds.</summary>
public sealed class GameSettings
{
    public GameSettings(double gravity = 900, double flapSpeed = 320,
        double birdWidth = 86, double birdHeight = 48,
        double pipeSpeed = 140, double pipeWidth = 64, double pipeGapHeight = 180,
        double pipeSpacing = 260, double initialPipeOffset = 40,
        double gapMargin = 40, double birdClearance = 12)
    {
        RequirePositiveFinite(gravity, nameof(gravity));
        RequirePositiveFinite(flapSpeed, nameof(flapSpeed));
        RequirePositiveFinite(birdWidth, nameof(birdWidth));
        RequirePositiveFinite(birdHeight, nameof(birdHeight));
        RequirePositiveFinite(pipeSpeed, nameof(pipeSpeed));
        RequirePositiveFinite(pipeWidth, nameof(pipeWidth));
        RequirePositiveFinite(pipeGapHeight, nameof(pipeGapHeight));
        RequirePositiveFinite(pipeSpacing, nameof(pipeSpacing));
        RequirePositiveFinite(initialPipeOffset, nameof(initialPipeOffset));
        RequirePositiveFinite(gapMargin, nameof(gapMargin));
        RequirePositiveFinite(birdClearance, nameof(birdClearance));
        Gravity = gravity;
        FlapSpeed = flapSpeed;
        BirdWidth = birdWidth;
        BirdHeight = birdHeight;
        PipeSpeed = pipeSpeed;
        PipeWidth = pipeWidth;
        PipeGapHeight = pipeGapHeight;
        PipeSpacing = pipeSpacing;
        InitialPipeOffset = initialPipeOffset;
        GapMargin = gapMargin;
        BirdClearance = birdClearance;
    }

    public double Gravity { get; }
    public double FlapSpeed { get; }
    public double BirdWidth { get; }
    public double BirdHeight { get; }
    public double PipeSpeed { get; }
    public double PipeWidth { get; }
    public double PipeGapHeight { get; }
    /// <summary>Distance between adjacent pair left edges, not empty space.</summary>
    public double PipeSpacing { get; }
    public double InitialPipeOffset { get; }
    public double GapMargin { get; }
    public double BirdClearance { get; }

    public void ValidateFor(Playfield playfield)
    {
        ArgumentNullException.ThrowIfNull(playfield);
        if (BirdWidth > playfield.Width || BirdHeight > playfield.GroundTop
            || PipeGapHeight <= BirdHeight + 2 * BirdClearance
            || PipeGapHeight + 2 * GapMargin >= playfield.GroundTop
            || PipeSpacing <= PipeWidth + BirdWidth + 2 * BirdClearance
            || PipeSpacing + PipeWidth > playfield.Width)
        {
            throw new ArgumentException("Settings must provide valid gaps, bird clearance, and room for two visible pairs.");
        }

        // One supported one-second update may cross at most one two-pair cycle.
        var firstX = playfield.Width + InitialPipeOffset;
        var secondX = firstX + PipeSpacing;
        if (!double.IsFinite(2 * PipeSpacing) || PipeSpeed > 2 * PipeSpacing
            || !double.IsFinite(secondX + PipeWidth) || secondX <= firstX || secondX + PipeWidth <= secondX)
        {
            throw new ArgumentException("Pipe speed or initial positions exceed supported update bounds.");
        }
    }

    private static void RequirePositiveFinite(double value, string parameter)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameter, "Must be finite and positive.");
        }
    }
}
