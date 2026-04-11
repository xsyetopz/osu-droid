package moe.osudroid.assets;

import java.io.Reader;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.files.FileHandle;
import com.google.gson.Gson;

import moe.osudroid.assets.ui.UiAssets;
import moe.osudroid.assets.ui.UiResourceCatalog;
import moe.osudroid.assets.ui.UiThemeManifest;
import moe.osudroid.platform.StorageBackend;

public final class ThemeResolver {
    private final Gson gson;
    private final StorageBackend storageBackend;

    public ThemeResolver(Gson gson, StorageBackend storageBackend) {
        this.gson = gson;
        this.storageBackend = storageBackend;
    }

    public UiResourceCatalog resolve() {
        UiThemeManifest manifest = loadUserTheme();
        if (manifest == null) {
            manifest = loadBuiltTheme();
        }
        if (manifest == null) {
            manifest = fallbackManifest();
        }
        return new UiResourceCatalog(manifest);
    }

    private UiThemeManifest loadUserTheme() {
        String writableRoot = storageBackend.writableRoot();
        if (writableRoot == null || writableRoot.isEmpty()) {
            return null;
        }
        FileHandle userTheme = Gdx.files.absolute(writableRoot + "/user-data/themes/ui/theme-manifest.json");
        return parse(userTheme);
    }

    private UiThemeManifest loadBuiltTheme() {
        return parse(Gdx.files.internal(UiAssets.THEME_MANIFEST));
    }

    private UiThemeManifest parse(FileHandle fileHandle) {
        if (fileHandle == null || !fileHandle.exists()) {
            return null;
        }
        try {
            Reader reader = fileHandle.reader("UTF-8");
            try {
                return gson.fromJson(reader, UiThemeManifest.class);
            } finally {
                reader.close();
            }
        } catch (Exception e) {
            return null;
        }
    }

    private UiThemeManifest fallbackManifest() {
        String fallbackJson = "{"
                + "\"name\":\"fallback-ui\","
                + "\"version\":1,"
                + "\"brandName\":\"osu!droid\","
                + "\"brandSubtitle\":\"Fallback shell\","
                + "\"defaultRoute\":\"LOGIN\","
                + "\"palette\":{"
                + "\"background\":\"#101821\","
                + "\"panel\":\"#162433\","
                + "\"panelAlt\":\"#1B3044\","
                + "\"accent\":\"#48C0F7\","
                + "\"accentSoft\":\"#2A6F91\","
                + "\"textPrimary\":\"#F5FBFF\","
                + "\"textMuted\":\"#9FB2C4\","
                + "\"success\":\"#58D68D\","
                + "\"danger\":\"#FF6F91\""
                + "},"
                + "\"assets\":[{\"key\":\"CHROME_PIXEL\",\"path\":\"bootstrap/pixel.png\",\"origin\":\"FALLBACK\"}]"
                + "}";
        return gson.fromJson(fallbackJson, UiThemeManifest.class);
    }
}
