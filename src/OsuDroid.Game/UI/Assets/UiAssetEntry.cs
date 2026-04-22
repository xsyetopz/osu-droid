namespace OsuDroid.Game.UI;

public sealed record UiAssetEntry(
    string LogicalName,
    string ContentName,
    UiAssetKind Kind,
    UiAssetProvenance Provenance,
    UiSize NativeSize);
