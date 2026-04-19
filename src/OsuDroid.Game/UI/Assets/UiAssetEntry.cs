namespace OsuDroid.Game.UI;

public sealed record UiAssetEntry(
    string LogicalName,
    string PackagePath,
    UiAssetKind Kind,
    UiAssetProvenance Provenance,
    UiSize NativeSize);
