package moe.osudroid.android;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.VibrationEffect;
import android.os.Vibrator;

import moe.osudroid.platform.AudioBackend;
import moe.osudroid.platform.ExternalUiBackend;
import moe.osudroid.platform.HapticsBackend;
import moe.osudroid.platform.PlatformServices;
import moe.osudroid.platform.StorageBackend;

public final class AndroidPlatformServices {

    private AndroidPlatformServices() {
    }

    public static PlatformServices create(final Context context) {
        AudioBackend audioBackend = new AudioBackend() {
            @Override
            public boolean isLowLatencySupported() {
                return true;
            }
        };

        StorageBackend storageBackend = new StorageBackend() {
            @Override
            public String writableRoot() {
                return context.getFilesDir().getAbsolutePath();
            }
        };

        HapticsBackend hapticsBackend = new HapticsBackend() {
            @Override
            public void lightImpact() {
                Vibrator vibrator = (Vibrator) context.getSystemService(Vibrator.class);
                if (vibrator != null && vibrator.hasVibrator()) {
                    vibrator.vibrate(VibrationEffect.createOneShot(10L, VibrationEffect.DEFAULT_AMPLITUDE));
                }
            }
        };

        ExternalUiBackend externalUiBackend = new ExternalUiBackend() {
            @Override
            public void openUri(String uri) {
                Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(uri));
                intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                context.startActivity(intent);
            }
        };

        return new PlatformServices(audioBackend, storageBackend, hapticsBackend, externalUiBackend);
    }
}
