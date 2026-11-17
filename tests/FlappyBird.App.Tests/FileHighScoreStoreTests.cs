using FlappyBird.App.Services;

namespace FlappyBird.App.Tests;

public sealed class FileHighScoreStoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "FlappyBird.Tests", Guid.NewGuid().ToString("N"));
    private string ScorePath => Path.Combine(directory, "nested", "high-score.txt");

    [Fact]
    public void MissingFileAndDirectoryAreNormalFirstLaunchWithoutCreatingData()
    {
        Assert.Equal(0, new FileHighScoreStore(ScorePath).Load());
        Assert.False(Directory.Exists(directory));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void FreshInstancesReadSavedNonnegativeIntegers(int value)
    {
        Assert.Equal(value, new FileHighScoreStore(ScorePath).Save(value));
        Assert.Equal(value, new FileHighScoreStore(ScorePath).Load());
        Assert.Equal(value.ToString(System.Globalization.CultureInfo.InvariantCulture), File.ReadAllText(ScorePath).Trim());
        Assert.Single(Directory.GetFiles(Path.GetDirectoryName(ScorePath)!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    [InlineData("{\"score\":4}")]
    [InlineData("4\n5")]
    [InlineData("00000000000000000000000000000000000000000000000000000000000000001")]
    public void InvalidOrOversizedDataIsRejectedAndNotChangedOnLoad(string value)
    {
        Write(value);
        Assert.Throws<InvalidDataException>(() => new FileHighScoreStore(ScorePath).Load());
        Assert.Equal(value, File.ReadAllText(ScorePath));
    }

    [Fact]
    public void InvalidUtf8IsRejected()
    {
        Write("0");
        File.WriteAllBytes(ScorePath, [0xff, 0xfe, 0x31, 0]);
        Assert.Throws<InvalidDataException>(() => new FileHighScoreStore(ScorePath).Load());
    }

    [Fact]
    public void SavingRechecksExistingBestAndNeverLowersIt()
    {
        var olderInstance = new FileHighScoreStore(ScorePath);
        Assert.Equal(0, olderInstance.Load());
        new FileHighScoreStore(ScorePath).Save(20);
        Assert.Equal(20, olderInstance.Save(8));
        Assert.Equal(20, new FileHighScoreStore(ScorePath).Load());
        Assert.Equal(21, olderInstance.Save(21));
    }

    [Fact]
    public void SavingZeroOverCorruptDataProducesAValidReloadableFile()
    {
        Write("invalid");
        var store = new FileHighScoreStore(ScorePath);
        Assert.Equal(0, store.Save(0));
        Assert.Equal(0, new FileHighScoreStore(ScorePath).Load());
        Assert.Single(Directory.GetFiles(Path.GetDirectoryName(ScorePath)!));
    }

    [Fact]
    public void NewBestCanReplaceCorruptDataAfterLoadWarning()
    {
        Write("corrupt");
        var service = new HighScoreService(new FileHighScoreStore(ScorePath));
        Assert.Equal(0, service.BestScore);
        Assert.NotNull(service.StatusMessage);
        service.RecordCompletedRun(3);
        Assert.Equal(3, new FileHighScoreStore(ScorePath).Load());
        Assert.Null(service.StatusMessage);
    }

    [Fact]
    public void FailedReplacementPreservesValidFileAndCleansTemporaryWrite()
    {
        Write("7\n");
        using (var locked = new FileStream(ScorePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var service = new HighScoreService(new FileHighScoreStore(ScorePath));
            Assert.Equal(7, service.BestScore);
            service.RecordCompletedRun(9);
            Assert.Equal(9, service.BestScore);
            Assert.NotNull(service.StatusMessage);
            Assert.Equal("7\n", File.ReadAllText(ScorePath));
            Assert.Single(Directory.GetFiles(Path.GetDirectoryName(ScorePath)!));
        }
        Assert.Equal(7, new FileHighScoreStore(ScorePath).Load());
    }

    [Fact]
    public void LockedReadAndUnwritableParentAreReportedWithoutThrowingFromService()
    {
        Write("8");
        using (var locked = new FileStream(ScorePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var service = new HighScoreService(new FileHighScoreStore(ScorePath));
            Assert.Equal(0, service.BestScore);
            Assert.NotNull(service.StatusMessage);
            service.RecordCompletedRun(9);
            Assert.Equal(9, service.BestScore);
            Assert.NotNull(service.StatusMessage);
        }
        Assert.Equal("8", File.ReadAllText(ScorePath));
        // A regular file cannot be used as the parent directory. No system ACLs are changed.
        var unavailable = new HighScoreService(new FileHighScoreStore(Path.Combine(ScorePath, "child.txt")));
        unavailable.RecordCompletedRun(10);
        Assert.Equal(10, unavailable.BestScore);
        Assert.NotNull(unavailable.StatusMessage);
        Assert.Equal("8", File.ReadAllText(ScorePath));
    }

    [Fact]
    public void NegativeSaveIsRejectedBeforeCreatingData()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FileHighScoreStore(ScorePath).Save(-1));
        Assert.False(Directory.Exists(directory));
    }

    private void Write(string value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ScorePath)!);
        File.WriteAllText(ScorePath, value);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
    }
}
