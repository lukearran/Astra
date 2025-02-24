using System.Text.RegularExpressions;

namespace Astra.Gtk.Helpers;

public static partial class UrlHelpers
{
    public static string StripUrlToDomain(string url)
    {
        var match = DomainStripRegex().Match(url);

        return match.ToString();
    }

    [GeneratedRegex(@"(?m)http(?:s?):\/\/.*?([^\.\/]+?\.[^\.]+?)(?:\/|$)")]
    private static partial Regex DomainStripRegex();
}