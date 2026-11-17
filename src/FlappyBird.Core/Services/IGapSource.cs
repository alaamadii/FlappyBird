namespace FlappyBird.Core.Services;

/// <summary>Supplies a gap top in [minimumInclusive, maximumExclusive).</summary>
public interface IGapSource
{
    double NextGapTop(double minimumInclusive, double maximumExclusive);
}
