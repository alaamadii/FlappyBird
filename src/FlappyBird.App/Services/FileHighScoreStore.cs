using System.Globalization;
using System.Text;

namespace FlappyBird.App.Services;

/// <summary>A small UTF-8 integer file replaced only after a complete temporary write.</summary>
public sealed class FileHighScoreStore(string? filePath = null) : IHighScoreStore
{
    public static string DefaultPath
    {
        get
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(root)) throw new IOException("Local application data is unavailable.");
            return Path.Combine(root, "YMH219-FlappyBird", "high-score.txt");
        }
    }

    private string FilePath => Path.GetFullPath(filePath ?? DefaultPath);

    public int Load()
    {
        try
        {
            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: false);
            var buffer = new char[64];
            var count = reader.ReadBlock(buffer, 0, buffer.Length);
            if (reader.Read() != -1 || !int.TryParse(new string(buffer, 0, count).Trim(),
                NumberStyles.None, CultureInfo.InvariantCulture, out var score))
            {
                throw new InvalidDataException("Invalid high-score data.");
            }

            return score;
        }
        catch (FileNotFoundException) { return 0; }
        catch (DirectoryNotFoundException) { return 0; }
        catch (DecoderFallbackException exception) { throw new InvalidDataException("Invalid high-score encoding.", exception); }
    }

    public int Save(int score)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(score);
        var path = FilePath;
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        var existing = 0;
        try
        {
            existing = Load();
            if (existing >= score && File.Exists(path)) return existing;
        }
        catch (InvalidDataException) { }
        var best = Math.Max(score, existing);

        var temporary = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
                writer.WriteLine(best.ToString(CultureInfo.InvariantCulture));
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporary, path, overwrite: true);
            return best;
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (System.Security.SecurityException) { }
        }
    }
}
