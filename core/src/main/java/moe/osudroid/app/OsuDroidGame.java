package moe.osudroid.app;

import com.badlogic.gdx.Game;
import com.badlogic.gdx.assets.AssetManager;

import moe.osudroid.platform.PlatformServices;
import moe.osudroid.screen.BootstrapScreen;

public final class OsuDroidGame extends Game {
    private final PlatformServices platformServices;
    private AssetManager assetManager;

    public OsuDroidGame(PlatformServices platformServices) {
        this.platformServices = platformServices;
    }

    @Override
    public void create() {
        assetManager = new AssetManager();
        setScreen(new BootstrapScreen(assetManager, platformServices));
    }

    @Override
    public void dispose() {
        super.dispose();
        if (assetManager != null) {
            assetManager.dispose();
        }
    }

    public PlatformServices getPlatformServices() {
        return platformServices;
    }
}
