package moe.osudroid.lwjgl3;

import com.badlogic.gdx.Gdx;

import moe.osudroid.platform.AudioBackend;
import moe.osudroid.platform.ExternalUiBackend;
import moe.osudroid.platform.HapticsBackend;
import moe.osudroid.platform.PlatformServices;
import moe.osudroid.platform.StorageBackend;

public final class DesktopPlatformServices {
    private DesktopPlatformServices() {
    }

    public static PlatformServices create() {
        AudioBackend audioBackend = new AudioBackend() {
            @Override
            public boolean isLowLatencySupported() {
                return false;
            }
        };

        StorageBackend storageBackend = new StorageBackend() {
            @Override
            public String writableRoot() {
                return System.getProperty("user.home");
            }
        };

        HapticsBackend hapticsBackend = new HapticsBackend() {
            @Override
            public void lightImpact() {
            }
        };

        ExternalUiBackend externalUiBackend = new ExternalUiBackend() {
            @Override
            public void openUri(String uri) {
                Gdx.net.openURI(uri);
            }
        };

        return new PlatformServices(audioBackend, storageBackend, hapticsBackend, externalUiBackend);
    }
}
