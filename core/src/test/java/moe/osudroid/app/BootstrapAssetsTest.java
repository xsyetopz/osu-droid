package moe.osudroid.app;

import moe.osudroid.assets.BootstrapAssets;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

public final class BootstrapAssetsTest {
    @Test
    void bootstrapPathsStayStable() {
        assertEquals("bootstrap/manifest.json", BootstrapAssets.BOOTSTRAP_MANIFEST);
        assertEquals("bootstrap/pixel.png", BootstrapAssets.BOOTSTRAP_PIXEL);
    }
}
