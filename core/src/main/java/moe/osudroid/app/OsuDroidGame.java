package moe.osudroid.app;

import com.badlogic.gdx.Game;
import com.badlogic.gdx.assets.AssetManager;
import com.google.gson.Gson;

import moe.osudroid.assets.ThemeResolver;
import moe.osudroid.platform.PlatformServices;
import moe.osudroid.ui.AppShellScreen;
import moe.osudroid.screen.BootstrapScreen;

public final class OsuDroidGame extends Game {
    private final PlatformServices platformServices;
    private final AppServices appServices;
    private AssetManager assetManager;

    public OsuDroidGame(PlatformServices platformServices) {
        this.platformServices = platformServices;
        this.appServices = AppServicesFactory.createDefaultServices(platformServices);
    }

    @Override
    public void create() {
        assetManager = new AssetManager();
        setScreen(new BootstrapScreen(this, assetManager, platformServices));
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

    public AppServices getAppServices() {
        return appServices;
    }

    public void showAppShell() {
        setScreen(new AppShellScreen(
                assetManager,
                appServices,
                new ThemeResolver(new Gson(), platformServices.getStorageBackend())));
    }
}
