namespace Astra.Gtk.Helpers;

public static class StringHelpers
{
    public static string CutWithEllipsis(this string input, int maxChars)
    {
        return input.Length < maxChars ? input : string.Concat(input.AsSpan(0, maxChars), "...");
    }
}