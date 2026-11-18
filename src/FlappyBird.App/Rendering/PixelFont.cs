namespace FlappyBird.App.Rendering;

/// <summary>Original five-by-seven block glyphs for arcade headings; no font asset required.</summary>
internal static class PixelFont
{
    private static readonly IReadOnlyDictionary<char, byte[]> Glyphs = new Dictionary<char, byte[]>
    {
        ['A'] = [14, 17, 17, 31, 17, 17, 17],
        ['B'] = [30, 17, 17, 30, 17, 17, 30],
        ['C'] = [14, 17, 16, 16, 16, 17, 14],
        ['D'] = [30, 17, 17, 17, 17, 17, 30],
        ['E'] = [31, 16, 16, 30, 16, 16, 31],
        ['F'] = [31, 16, 16, 30, 16, 16, 16],
        ['G'] = [14, 17, 16, 23, 17, 17, 14],
        ['H'] = [17, 17, 17, 31, 17, 17, 17],
        ['I'] = [31, 4, 4, 4, 4, 4, 31],
        ['J'] = [7, 2, 2, 2, 18, 18, 12],
        ['K'] = [17, 18, 20, 24, 20, 18, 17],
        ['L'] = [16, 16, 16, 16, 16, 16, 31],
        ['M'] = [17, 27, 21, 21, 17, 17, 17],
        ['N'] = [17, 25, 21, 19, 17, 17, 17],
        ['O'] = [14, 17, 17, 17, 17, 17, 14],
        ['P'] = [30, 17, 17, 30, 16, 16, 16],
        ['Q'] = [14, 17, 17, 17, 21, 18, 13],
        ['R'] = [30, 17, 17, 30, 20, 18, 17],
        ['S'] = [15, 16, 16, 14, 1, 1, 30],
        ['T'] = [31, 4, 4, 4, 4, 4, 4],
        ['U'] = [17, 17, 17, 17, 17, 17, 14],
        ['V'] = [17, 17, 17, 17, 17, 10, 4],
        ['W'] = [17, 17, 17, 21, 21, 21, 10],
        ['X'] = [17, 17, 10, 4, 10, 17, 17],
        ['Y'] = [17, 17, 10, 4, 4, 4, 4],
        ['Z'] = [31, 1, 2, 4, 8, 16, 31],
        ['0'] = [14, 17, 19, 21, 25, 17, 14],
        ['1'] = [4, 12, 4, 4, 4, 4, 14],
        ['2'] = [14, 17, 1, 2, 4, 8, 31],
        ['3'] = [30, 1, 1, 14, 1, 1, 30],
        ['4'] = [2, 6, 10, 18, 31, 2, 2],
        ['5'] = [31, 16, 16, 30, 1, 1, 30],
        ['6'] = [14, 16, 16, 30, 17, 17, 14],
        ['7'] = [31, 1, 2, 4, 8, 8, 8],
        ['8'] = [14, 17, 17, 14, 17, 17, 14],
        ['9'] = [14, 17, 17, 15, 1, 1, 14],
        [':'] = [0, 4, 4, 0, 4, 4, 0],
        ['.'] = [0, 0, 0, 0, 0, 4, 4],
        ['-'] = [0, 0, 0, 31, 0, 0, 0],
        ['!'] = [4, 4, 4, 4, 4, 0, 4],
        ['?'] = [14, 17, 1, 2, 4, 0, 4],
        ['/'] = [1, 2, 2, 4, 8, 8, 16],
    };

    public static void Draw(Graphics graphics, string text, float size, Color color, RectangleF bounds)
    {
        if (text.Length == 0 || bounds.Width <= 0 || bounds.Height <= 0) return;
        var columns = text.Length * 6 - 1;
        var cell = Math.Max(1, Math.Min((int)(size / 7),
            Math.Min((int)((bounds.Width - 8) / columns), (int)(bounds.Height / 7))));
        var left = (float)Math.Round(bounds.X + (bounds.Width - columns * cell) / 2);
        var top = (float)Math.Round(bounds.Y + (bounds.Height - 7 * cell) / 2);
        using var ink = new SolidBrush(color);
        for (var index = 0; index < text.Length; index++)
        {
            var character = char.ToUpperInvariant(text[index]);
            if (character == ' ') continue;
            var rows = Glyphs.TryGetValue(character, out var glyph) ? glyph : Glyphs['?'];
            for (var y = 0; y < 7; y++)
                for (var x = 0; x < 5; x++)
                    if ((rows[y] & (1 << (4 - x))) != 0)
                        graphics.FillRectangle(ink, left + (index * 6 + x) * cell, top + y * cell, cell, cell);
        }
    }
}
