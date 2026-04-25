using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Assets;

public static partial class DroidAssets
{
    private static readonly HashSet<string> s_startupAssetNames = new(StringComparer.Ordinal)
    {
        Loading,
        LoadingTitle,
        Welcome,
    };

    public static UiAssetManifest StartupManifest { get; } = new(s_assetCatalog.Where(entry => s_startupAssetNames.Contains(entry.LogicalName)));

    public static UiAssetManifest MainMenuManifest { get; } = new(s_assetCatalog);

    private static UiAssetEntry Texture(string logicalName, string packagePath, float width, float height) =>
        new(logicalName, ToContentName(packagePath), UiAssetKind.Texture, UiAssetProvenance.OsuDroid, new UiSize(width, height));

    private static string ToContentName(string packagePath)
    {
        const string prefix = "assets/";
        string contentName = packagePath.StartsWith(prefix, StringComparison.Ordinal)
            ? packagePath[prefix.Length..]
            : packagePath;
        string extension = Path.GetExtension(contentName);
        return extension.Length == 0 ? contentName : contentName[..^extension.Length];
    }
}
