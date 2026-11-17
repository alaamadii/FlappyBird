namespace FlappyBird.Core.Services;

/// <summary>Instance-owned randomness; pass a seeded Random when repeatability is useful.</summary>
public sealed class RandomGapSource(Random? random = null) : IGapSource
{
    private readonly Random random = random ?? new Random();

    public double NextGapTop(double minimumInclusive, double maximumExclusive)
    {
        if (!double.IsFinite(minimumInclusive) || minimumInclusive < 0
            || !double.IsFinite(maximumExclusive) || maximumExclusive <= minimumInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
        }

        // Protect the exclusive endpoint against final floating-point rounding.
        return Math.Min(Math.BitDecrement(maximumExclusive),
            minimumInclusive + (maximumExclusive - minimumInclusive) * random.NextDouble());
    }
}
