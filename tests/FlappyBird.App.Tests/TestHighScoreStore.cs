using FlappyBird.App.Services;

namespace FlappyBird.App.Tests;

internal sealed class TestHighScoreStore(int initial = 0) : IHighScoreStore
{
    public int Value { get; private set; } = initial;
    public int Loads { get; private set; }
    public int Saves { get; private set; }
    public Exception? LoadFailure { get; set; }
    public Exception? SaveFailure { get; set; }

    public int Load()
    {
        Loads++;
        if (LoadFailure is not null) throw LoadFailure;
        return Value;
    }

    public int Save(int score)
    {
        Saves++;
        if (SaveFailure is not null) throw SaveFailure;
        Value = Math.Max(Value, score);
        return Value;
    }
}
