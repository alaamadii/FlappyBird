using System.Security;
using FlappyBird.App.Services;

namespace FlappyBird.App.Tests;

public sealed class HighScoreServiceTests
{
    [Fact]
    public void LoadsOnceAndSavesOnlyAnImprovedCompletedRun()
    {
        var store = new TestHighScoreStore(5);
        var service = new HighScoreService(store);
        Assert.Equal(5, service.BestScore);
        Assert.Null(service.StatusMessage);
        foreach (var score in new[] { 0, 3, 5, 8, 6, 8, 10, 9 }) service.RecordCompletedRun(score);
        Assert.Equal(10, service.BestScore);
        Assert.Equal(10, store.Value);
        Assert.Equal(1, store.Loads);
        Assert.Equal(2, store.Saves);
        Assert.Equal(10, new HighScoreService(store).BestScore);
    }

    public static TheoryData<Exception> StorageFailures => new()
    {
        new IOException("private path"), new UnauthorizedAccessException("private path"),
        new SecurityException("private path"), new InvalidDataException("private contents"),
    };

    [Theory]
    [MemberData(nameof(StorageFailures))]
    public void ReadAndWriteFailuresKeepSessionBestWithoutExposingTechnicalDetails(Exception failure)
    {
        var store = new TestHighScoreStore { LoadFailure = failure, SaveFailure = failure };
        var service = new HighScoreService(store);
        Assert.Equal(0, service.BestScore);
        Assert.Equal("Saved best score unavailable.", service.StatusMessage);
        service.RecordCompletedRun(7);
        Assert.Equal(7, service.BestScore);
        Assert.Equal("Best score couldn't be saved.", service.StatusMessage);
        Assert.Equal(1, store.Saves);
        store.SaveFailure = null;
        service.RecordCompletedRun(2); // Retry the unsaved best only when another run finishes.
        Assert.Equal(7, service.BestScore);
        Assert.Equal(7, store.Value);
        Assert.Null(service.StatusMessage);
        Assert.Equal(2, store.Saves);
        service.RecordCompletedRun(7);
        Assert.Equal(2, store.Saves);
    }

    [Fact]
    public void NegativeStoredValueIsTreatedAsInvalidAndNegativeRunIsRejected()
    {
        var store = new TestHighScoreStore(-1);
        var service = new HighScoreService(store);
        Assert.Equal(0, service.BestScore);
        Assert.NotNull(service.StatusMessage);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.RecordCompletedRun(-1));
        Assert.Equal(0, store.Saves);
    }

    [Fact]
    public void SavingCanDiscoverANewerBestAlreadyOnDisk()
    {
        var store = new TestHighScoreStore(4);
        var service = new HighScoreService(store);
        store.Save(12);
        service.RecordCompletedRun(5);
        Assert.Equal(12, service.BestScore);
        Assert.Null(service.StatusMessage);
    }
}
