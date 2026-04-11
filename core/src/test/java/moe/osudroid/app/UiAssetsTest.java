package moe.osudroid.app;

import static org.junit.jupiter.api.Assertions.assertEquals;

import org.junit.jupiter.api.Test;

import moe.osudroid.assets.ui.UiAssets;

final class UiAssetsTest {
    @Test
    void uiAssetPathsStayStable() {
        assertEquals("ui/theme-manifest.json", UiAssets.THEME_MANIFEST);
        assertEquals("ui/NOTICE-upstream.txt", UiAssets.UPSTREAM_NOTICE);
    }
}
