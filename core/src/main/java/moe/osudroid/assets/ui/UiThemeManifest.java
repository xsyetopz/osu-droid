package moe.osudroid.assets.ui;

import java.util.Collections;
import java.util.List;

public final class UiThemeManifest {
    private String name;
    private int version;
    private String brandName;
    private String brandSubtitle;
    private String defaultRoute;
    private UiPalette palette;
    private List<UiAssetEntry> assets;
    private List<UiNotice> notices;

    public String getName() {
        return name;
    }

    public int getVersion() {
        return version;
    }

    public String getBrandName() {
        return brandName;
    }

    public String getBrandSubtitle() {
        return brandSubtitle;
    }

    public String getDefaultRoute() {
        return defaultRoute;
    }

    public UiPalette getPalette() {
        return palette;
    }

    public List<UiAssetEntry> getAssets() {
        return assets == null ? Collections.<UiAssetEntry>emptyList() : assets;
    }

    public List<UiNotice> getNotices() {
        return notices == null ? Collections.<UiNotice>emptyList() : notices;
    }
}
