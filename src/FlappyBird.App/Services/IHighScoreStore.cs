namespace FlappyBird.App.Services;

/// <summary>Loads a best score and returns the best value actually saved.</summary>
public interface IHighScoreStore
{
    int Load();
    int Save(int score);
}
