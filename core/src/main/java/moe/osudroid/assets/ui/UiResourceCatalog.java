package moe.osudroid.assets.ui;

import java.util.EnumMap;
import java.util.List;
import java.util.Map;

import moe.osudroid.assets.BootstrapAssets;

public final class UiResourceCatalog {
    private final UiThemeManifest manifest;
    private final Map<UiAssetKey, UiAssetEntry> assetEntries;

    public UiResourceCatalog(UiThemeManifest manifest) {
        this.manifest = manifest;
        this.assetEntries = new EnumMap<UiAssetKey, UiAssetEntry>(UiAssetKey.class);
        List<UiAssetEntry> assets = manifest.getAssets();
        if (assets != null) {
            for (UiAssetEntry entry : assets) {
                assetEntries.put(entry.getKey(), entry);
            }
        }
    }

    public UiThemeManifest getManifest() {
        return manifest;
    }

    public String pathFor(UiAssetKey key) {
        UiAssetEntry entry = assetEntries.get(key);
        return entry == null ? BootstrapAssets.BOOTSTRAP_PIXEL : entry.getPath();
    }
}
