using FlappyBird.Core.Models;
using FlappyBird.Core.Physics;

namespace FlappyBird.Core.Services;

/// <summary>Owns motion, contact-before-score ordering, game states, and clean-run reset.</summary>
public sealed class GameEngine
{
    public const double MaximumRuleStepSeconds = 1d / 60;
    private readonly Playfield playfield;
    private readonly GameSettings settings;

    public GameEngine(Playfield playfield, GameSettings settings, IGapSource? gapSource = null)
    {
        Bird = new Bird(playfield, settings);
        Pipes = new PipeSystem(playfield, settings, gapSource ?? new RandomGapSource());
        this.playfield = playfield;
        this.settings = settings;
    }

    public Bird Bird { get; }
    public PipeSystem Pipes { get; }
    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }

    public void Flap()
    {
        if (State == GameState.GameOver) return;
        State = GameState.Running;
        Bird.Flap();
    }

    public void Update(double elapsedSeconds)
    {
        PipeSystem.ValidateElapsed(elapsedSeconds);
        if (State != GameState.Running || elapsedSeconds == 0) return;

        var steps = (int)Math.Ceiling(elapsedSeconds / MaximumRuleStepSeconds);
        var step = elapsedSeconds / steps;
        for (var index = 0; index < steps && State == GameState.Running; index++)
        {
            AdvanceRules(step);
        }
    }

    private void AdvanceRules(double step)
    {
        var next = Bird.Predict(step);
        var deltaY = next.Y - Bird.Y;
        var travel = settings.PipeSpeed * step;
        var bounds = new PipeRectangle(Bird.X, Bird.Y, Bird.Width, Bird.Height);
        double? contact = bounds.Bottom >= playfield.GroundTop ? 0 : null;
        if (bounds.Bottom < playfield.GroundTop && deltaY > 0 && next.Y + Bird.Height >= playfield.GroundTop)
        {
            contact = Math.Clamp((playfield.GroundTop - bounds.Bottom) / deltaY, 0, 1);
        }

        foreach (var pair in Pipes.Pairs)
        {
            var upper = CollisionDetector.ContactFraction(bounds, travel, deltaY, pair.Upper);
            var lower = CollisionDetector.ContactFraction(bounds, travel, deltaY, pair.Lower);
            if (upper.HasValue) contact = Math.Min(contact ?? 1, upper.Value);
            if (lower.HasValue) contact = Math.Min(contact ?? 1, lower.Value);
        }

        if (contact.HasValue)
        {
            Pipes.MoveWithoutRecycling(step * contact.Value);
            Bird.StopAt(Math.Clamp(Bird.Y + deltaY * contact.Value, 0, playfield.GroundTop - Bird.Height));
            State = GameState.GameOver;
            return;
        }

        Bird.Update(step);
        Pipes.MoveWithoutRecycling(step);
        foreach (var pair in Pipes.Pairs)
        {
            if (!pair.HasScored && pair.Right < Bird.X)
            {
                Score++;
                pair.MarkScored();
            }
        }

        // Score is evaluated against the departing pair, before its eligibility is reset.
        Pipes.RecycleExited();
    }

    public void Reset()
    {
        Pipes.Reset();
        Bird.Reset();
        Score = 0;
        State = GameState.Ready;
    }
}
