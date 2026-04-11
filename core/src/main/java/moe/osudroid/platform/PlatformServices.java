package moe.osudroid.platform;

public final class PlatformServices {
    private final AudioBackend audioBackend;
    private final StorageBackend storageBackend;
    private final HapticsBackend hapticsBackend;
    private final ExternalUiBackend externalUiBackend;

    public PlatformServices(
            AudioBackend audioBackend,
            StorageBackend storageBackend,
            HapticsBackend hapticsBackend,
            ExternalUiBackend externalUiBackend) {
        this.audioBackend = audioBackend;
        this.storageBackend = storageBackend;
        this.hapticsBackend = hapticsBackend;
        this.externalUiBackend = externalUiBackend;
    }

    public AudioBackend getAudioBackend() {
        return audioBackend;
    }

    public StorageBackend getStorageBackend() {
        return storageBackend;
    }

    public HapticsBackend getHapticsBackend() {
        return hapticsBackend;
    }

    public ExternalUiBackend getExternalUiBackend() {
        return externalUiBackend;
    }
}
