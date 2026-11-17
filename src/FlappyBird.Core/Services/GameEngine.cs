using FlappyBird.Core.Models;

namespace FlappyBird.Core.Services;

/// <summary>Coordinates Ready/Running bird flight and obstacle movement.</summary>
public sealed class GameEngine
{
    public GameEngine(Playfield playfield, GameSettings settings, IGapSource? gapSource = null)
    {
        Bird = new Bird(playfield, settings);
        Pipes = new PipeSystem(playfield, settings, gapSource ?? new RandomGapSource());
    }

    public Bird Bird { get; }
    public PipeSystem Pipes { get; }
    public GameState State { get; private set; } = GameState.Ready;

    public void Flap()
    {
        State = GameState.Running;
        Bird.Flap();
    }

    public void Update(double elapsedSeconds)
    {
        Bird.ValidateElapsed(elapsedSeconds);
        if (State == GameState.Running)
        {
            PipeSystem.ValidateElapsed(elapsedSeconds);
            Bird.Update(elapsedSeconds);
            Pipes.Update(elapsedSeconds);
        }
    }

    public void Reset()
    {
        Pipes.Reset();
        Bird.Reset();
        State = GameState.Ready;
    }
}
