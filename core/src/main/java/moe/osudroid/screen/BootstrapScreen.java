package moe.osudroid.screen;

import com.badlogic.gdx.ScreenAdapter;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.utils.ScreenUtils;

import moe.osudroid.assets.BootstrapAssets;
import moe.osudroid.platform.PlatformServices;

public final class BootstrapScreen extends ScreenAdapter {
    private final AssetManager assetManager;
    private final PlatformServices platformServices;
    private final SpriteBatch spriteBatch;

    private Texture bootstrapPixel;

    public BootstrapScreen(AssetManager assetManager, PlatformServices platformServices) {
        this.assetManager = assetManager;
        this.platformServices = platformServices;
        this.spriteBatch = new SpriteBatch();
    }

    @Override
    public void show() {
        if (!assetManager.isLoaded(BootstrapAssets.BOOTSTRAP_PIXEL, Texture.class)) {
            assetManager.load(BootstrapAssets.BOOTSTRAP_PIXEL, Texture.class);
        }
    }

    @Override
    public void render(float delta) {
        ScreenUtils.clear(0.05f, 0.08f, 0.12f, 1f);

        if (bootstrapPixel == null && assetManager.update()) {
            bootstrapPixel = assetManager.get(BootstrapAssets.BOOTSTRAP_PIXEL, Texture.class);
            bootstrapPixel.setFilter(Texture.TextureFilter.Nearest, Texture.TextureFilter.Nearest);
        }

        if (bootstrapPixel == null) {
            return;
        }

        float width = 320f;
        float height = 160f;
        float x = 48f;
        float y = 48f;

        spriteBatch.begin();
        spriteBatch.setColor(0.16f, 0.72f, 0.98f, 1f);
        spriteBatch.draw(bootstrapPixel, x, y, width, height);
        spriteBatch.setColor(Color.WHITE);
        spriteBatch.draw(bootstrapPixel, x + 24f, y + 24f, width - 48f, 8f);
        spriteBatch.draw(bootstrapPixel, x + 24f, y + 56f, width - 96f, 8f);
        spriteBatch.draw(bootstrapPixel, x + 24f, y + 88f, width - 128f, 8f);
        spriteBatch.end();
    }

    @Override
    public void dispose() {
        spriteBatch.dispose();
        if (assetManager.isLoaded(BootstrapAssets.BOOTSTRAP_PIXEL, Texture.class)) {
            assetManager.unload(BootstrapAssets.BOOTSTRAP_PIXEL);
        }
    }

    public PlatformServices getPlatformServices() {
        return platformServices;
    }
}
