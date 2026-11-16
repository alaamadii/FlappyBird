namespace FlappyBird.App.Timing;

/// <summary>Bounds catch-up work while translating real elapsed time into fixed updates.</summary>
public sealed class FixedStepLoop
{
    public const double StepSeconds = 1d / 60;
    public const double MaximumElapsedSeconds = 0.1;
    public const int MaximumUpdatesPerTick = 6;
    private double accumulator;

    public int Advance(double elapsedSeconds, Action<double> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        }

        accumulator += Math.Min(elapsedSeconds, MaximumElapsedSeconds);
        var updates = 0;
        while (accumulator + 1e-12 >= StepSeconds && updates < MaximumUpdatesPerTick)
        {
            update(StepSeconds);
            accumulator = Math.Max(0, accumulator - StepSeconds);
            updates++;
        }

        // Drop whole overdue steps after the work cap; retain only a fractional step.
        if (accumulator >= StepSeconds)
        {
            accumulator %= StepSeconds;
        }

        return updates;
    }

    public void Reset() => accumulator = 0;
}
