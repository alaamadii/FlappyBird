using System.Security;

namespace FlappyBird.App.Services;

/// <summary>Keeps a session best usable even when local persistence is unavailable.</summary>
public sealed class HighScoreService
{
    private readonly IHighScoreStore store;
    private int persistedBest;

    public HighScoreService(IHighScoreStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        this.store = store;
        try
        {
            BestScore = store.Load();
            if (BestScore < 0) throw new InvalidDataException("Invalid stored score.");
            persistedBest = BestScore;
        }
        catch (Exception exception) when (IsStorageError(exception))
        {
            BestScore = 0;
            StatusMessage = "Saved best score unavailable.";
        }
    }

    public int BestScore { get; private set; }
    public string? StatusMessage { get; private set; }

    public void RecordCompletedRun(int score)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(score);
        BestScore = Math.Max(BestScore, score);
        if (BestScore <= persistedBest) return;
        try
        {
            var saved = store.Save(BestScore);
            if (saved < BestScore) throw new InvalidDataException("Store did not retain the best score.");
            BestScore = saved;
            persistedBest = saved;
            StatusMessage = null;
        }
        catch (Exception exception) when (IsStorageError(exception))
        {
            StatusMessage = "Best score couldn't be saved.";
        }
    }

    private static bool IsStorageError(Exception exception) =>
        exception is IOException or InvalidDataException or UnauthorizedAccessException or SecurityException;
}
