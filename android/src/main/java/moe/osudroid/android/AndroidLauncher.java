package moe.osudroid.android;

import android.os.Bundle;

import com.badlogic.gdx.backends.android.AndroidApplication;
import com.badlogic.gdx.backends.android.AndroidApplicationConfiguration;

import moe.osudroid.app.OsuDroidGame;

public final class AndroidLauncher extends AndroidApplication {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        AndroidApplicationConfiguration configuration = new AndroidApplicationConfiguration();
        configuration.useImmersiveMode = true;
        configuration.useCompass = false;
        configuration.useAccelerometer = false;

        initialize(new OsuDroidGame(AndroidPlatformServices.create(this)), configuration);
    }
}
