namespace OsuDroid.Game.UI;

public sealed class UiAssetManifest
{
    private readonly Dictionary<string, UiAssetEntry> entriesByName;

    public UiAssetManifest(IEnumerable<UiAssetEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        entriesByName = entries.ToDictionary(static entry => entry.LogicalName, StringComparer.Ordinal);
    }

    public IReadOnlyCollection<UiAssetEntry> Entries => entriesByName.Values;

    public UiAssetEntry Get(string logicalName) => entriesByName[logicalName];

    public bool Contains(string logicalName) => entriesByName.ContainsKey(logicalName);
}
