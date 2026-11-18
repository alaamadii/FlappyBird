using FlappyBird.Core.Models;

namespace FlappyBird.App.Animation;

/// <summary>App-only presentation time. Advancing it never changes the game model.</summary>
public sealed class SceneAnimation
{
    private readonly double groundSpeed;
    private double wingTime;
    private double idleTime;
    private double groundOffset;
    private double backdropOffset;

    public SceneAnimationFrame Frame { get; private set; } = SceneAnimationFrame.Still;

    public SceneAnimation(GameSettings? settings = null) => groundSpeed = (settings ?? new GameSettings()).PipeSpeed;

    public void Advance(double seconds, GameState state, double verticalVelocity)
    {
        if (!double.IsFinite(seconds) || seconds < 0 || seconds > 1)
            throw new ArgumentOutOfRangeException(nameof(seconds));
        if (!double.IsFinite(verticalVelocity)) throw new ArgumentOutOfRangeException(nameof(verticalVelocity));
        if (seconds == 0 || state == GameState.GameOver) return;

        wingTime = (wingTime + seconds) % 0.4;
        var phase = (int)((wingTime + 1e-10) / 0.1) % 4;
        var wing = phase == 3 ? 1 : phase;
        if (state == GameState.Ready)
        {
            idleTime = (idleTime + seconds) % 1.8;
            Frame = new(wing, 0, (float)(Math.Sin(idleTime / 1.8 * Math.Tau) * 3),
                (float)groundOffset, (float)backdropOffset);
            return;
        }

        groundOffset = (groundOffset + groundSpeed * seconds) % 24;
        backdropOffset = (backdropOffset + 10 * seconds) % 480;
        var tilt = Math.Clamp(verticalVelocity / 20, -8, 12);
        Frame = new(wing, (float)tilt, 0, (float)groundOffset, (float)backdropOffset);
    }

    public void Flap()
    {
        wingTime = 0;
        Frame = Frame with { WingFrame = 0, TiltDegrees = -8, BobOffset = 0 };
    }

    public void Reset()
    {
        wingTime = idleTime = groundOffset = backdropOffset = 0;
        Frame = SceneAnimationFrame.Still;
    }
}

/// <summary>A deterministic snapshot supplied to paint, including frozen/paused scenes.</summary>
public readonly record struct SceneAnimationFrame(int WingFrame, float TiltDegrees,
    float BobOffset, float GroundOffset, float BackdropOffset)
{
    public static SceneAnimationFrame Still => new(1, 0, 0, 0, 0);
}
