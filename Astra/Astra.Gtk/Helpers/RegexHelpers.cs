using System.Text.RegularExpressions;

namespace Astra.Gtk.Helpers;

public static partial class RegexHelpers
{
    [GeneratedRegex(@"(?m)http(?:s?):\/\/.*?([^\.\/]+?\.[^\.]+?)(?:\/|$)")]
    public static partial Regex DomainStripRegex();

    public static string ParseRegex(this string input, Regex regex) => regex.Match(input).ToString();
}