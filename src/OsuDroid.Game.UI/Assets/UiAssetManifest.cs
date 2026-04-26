namespace OsuDroid.Game.UI.Assets;

public sealed class UiAssetManifest
{
    private readonly Dictionary<string, UiAssetEntry> _entriesByName;

    public UiAssetManifest(IEnumerable<UiAssetEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _entriesByName = entries.ToDictionary(
            static entry => entry.LogicalName,
            StringComparer.Ordinal
        );
    }

    public IReadOnlyCollection<UiAssetEntry> Entries => _entriesByName.Values;

    public UiAssetEntry Get(string logicalName) => _entriesByName[logicalName];

    public bool Contains(string logicalName) => _entriesByName.ContainsKey(logicalName);
}
