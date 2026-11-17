using FlappyBird.Core.Models;

namespace FlappyBird.Core.Services;

/// <summary>Owns two reusable pairs in left-to-right order and their gap generation.</summary>
public sealed class PipeSystem
{
    public const int PairCount = 2;
    public const double MaximumUpdateSeconds = 1;
    private readonly Playfield playfield;
    private readonly GameSettings settings;
    private readonly IGapSource gapSource;
    private readonly List<PipePair> pairs;

    public PipeSystem(Playfield playfield, GameSettings settings, IGapSource gapSource)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(gapSource);
        settings.ValidateFor(playfield);
        this.playfield = playfield;
        this.settings = settings;
        this.gapSource = gapSource;
        pairs = new List<PipePair>(PairCount);
        for (var index = 0; index < PairCount; index++)
        {
            pairs.Add(new PipePair(playfield, settings, InitialX(index), NextGap()));
        }

        Pairs = pairs.AsReadOnly();
    }

    public IReadOnlyList<PipePair> Pairs { get; }

    public void Update(double elapsedSeconds)
    {
        ValidateElapsed(elapsedSeconds);
        if (elapsedSeconds == 0)
        {
            return;
        }

        var distance = settings.PipeSpeed * elapsedSeconds;
        // Sample before movement so an invalid supplied source cannot corrupt pipe positions.
        var firstGap = pairs[0].Right - distance < 0 ? NextGap() : pairs[0].GapTop;
        var secondGap = pairs[1].Right - distance < 0 ? NextGap() : pairs[1].GapTop;
        foreach (var pair in pairs)
        {
            pair.Move(distance);
        }

        // With travel <= two spacings, each pair can recycle at most once per update.
        var recycled = 0;
        while (pairs[0].Right < 0)
        {
            var pair = pairs[0];
            // At the maximum full-cycle distance the mathematical result is >= -Width.
            // Clamp tiny floating-point undershoot so it cannot cause a third recycle.
            var nextX = Math.Max(-pair.Width, pairs[^1].X + settings.PipeSpacing);
            pair.Reset(nextX, recycled++ == 0 ? firstGap : secondGap);
            pairs.RemoveAt(0);
            pairs.Add(pair);
        }
    }

    public void Reset()
    {
        // Generate both before changing any positions if a supplied source is invalid.
        var firstGap = NextGap();
        var secondGap = NextGap();
        pairs[0].Reset(InitialX(0), firstGap);
        pairs[1].Reset(InitialX(1), secondGap);
    }

    internal static void ValidateElapsed(double elapsedSeconds)
    {
        Bird.ValidateElapsed(elapsedSeconds);
        if (elapsedSeconds > MaximumUpdateSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Obstacle updates support at most one second.");
        }
    }

    private double InitialX(int index) => playfield.Width + settings.InitialPipeOffset + index * settings.PipeSpacing;

    private double NextGap()
    {
        var minimum = settings.GapMargin;
        var maximum = playfield.GroundTop - settings.GapMargin - settings.PipeGapHeight;
        var gap = gapSource.NextGapTop(minimum, maximum);
        if (!double.IsFinite(gap) || gap < minimum || gap >= maximum)
        {
            throw new InvalidOperationException("The gap source returned a value outside its requested bounds.");
        }

        return gap;
    }
}
