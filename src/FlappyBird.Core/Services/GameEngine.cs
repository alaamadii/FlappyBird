using FlappyBird.Core.Models;

namespace FlappyBird.Core.Services;

/// <summary>Coordinates the currently implemented Ready/Running bird flight.</summary>
public sealed class GameEngine
{
    public GameEngine(Playfield playfield, GameSettings settings)
    {
        Bird = new Bird(playfield, settings);
    }

    public Bird Bird { get; }
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
            Bird.Update(elapsedSeconds);
        }
    }

    public void Reset()
    {
        Bird.Reset();
        State = GameState.Ready;
    }
}
