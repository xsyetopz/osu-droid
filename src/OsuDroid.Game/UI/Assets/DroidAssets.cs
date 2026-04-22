namespace OsuDroid.Game.UI;

public static partial class DroidAssets
{
    private static readonly HashSet<string> StartupAssetNames = new(StringComparer.Ordinal)
    {
        Loading,
        LoadingTitle,
        Welcome,
    };

    public static UiAssetManifest StartupManifest { get; } = new(AssetCatalog.Where(entry => StartupAssetNames.Contains(entry.LogicalName)));

    public static UiAssetManifest MainMenuManifest { get; } = new(AssetCatalog);

    private static UiAssetEntry Texture(string logicalName, string packagePath, float width, float height) =>
        new(logicalName, ToContentName(packagePath), UiAssetKind.Texture, UiAssetProvenance.OsuDroid, new UiSize(width, height));

    private static string ToContentName(string packagePath)
    {
        const string prefix = "assets/";
        var contentName = packagePath.StartsWith(prefix, StringComparison.Ordinal)
            ? packagePath[prefix.Length..]
            : packagePath;
        var extension = Path.GetExtension(contentName);
        return extension.Length == 0 ? contentName : contentName[..^extension.Length];
    }
}
