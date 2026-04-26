using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.UI.Assets;

public sealed record UiAssetEntry(
    string LogicalName,
    string ContentName,
    UiAssetKind Kind,
    UiAssetProvenance Provenance,
    UiSize NativeSize
);
