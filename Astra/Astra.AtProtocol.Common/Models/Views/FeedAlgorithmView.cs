namespace Astra.AtProtocol.Common.Models.Views;

public class FeedAlgorithmView(string displayName, string displayIcon, string algorithm)
{
    public string DisplayName { get; } = displayName;
    public string DisplayIcon { get; } = displayIcon;
    public string Algorithm { get; } = algorithm;
}