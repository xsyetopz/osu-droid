package moe.osudroid.service.beatmap;

public final class BeatmapCard {
    private final long beatmapSetId;
    private final long beatmapId;
    private final String title;
    private final String artist;
    private final String mapper;
    private final String difficultyName;
    private final float starRating;
    private final int bpm;
    private final String status;
    private final String source;
    private final String previewUrl;
    private final String downloadUrl;
    private final String localPath;

    public BeatmapCard(String title, String artist, String mapper, float starRating, int bpm) {
        this(0L, 0L, title, artist, mapper, "", starRating, bpm, "unknown", "local", "", "", "");
    }

    public BeatmapCard(
            long beatmapSetId,
            long beatmapId,
            String title,
            String artist,
            String mapper,
            String difficultyName,
            float starRating,
            int bpm,
            String status,
            String source,
            String previewUrl,
            String downloadUrl,
            String localPath) {
        this.beatmapSetId = beatmapSetId;
        this.beatmapId = beatmapId;
        this.title = title;
        this.artist = artist;
        this.mapper = mapper;
        this.difficultyName = difficultyName;
        this.starRating = starRating;
        this.bpm = bpm;
        this.status = status;
        this.source = source;
        this.previewUrl = previewUrl;
        this.downloadUrl = downloadUrl;
        this.localPath = localPath;
    }

    public long getBeatmapSetId() {
        return beatmapSetId;
    }

    public long getBeatmapId() {
        return beatmapId;
    }

    public String getTitle() {
        return title;
    }

    public String getArtist() {
        return artist;
    }

    public String getMapper() {
        return mapper;
    }

    public String getDifficultyName() {
        return difficultyName;
    }

    public float getStarRating() {
        return starRating;
    }

    public int getBpm() {
        return bpm;
    }

    public String getStatus() {
        return status;
    }

    public String getSource() {
        return source;
    }

    public String getPreviewUrl() {
        return previewUrl;
    }

    public String getDownloadUrl() {
        return downloadUrl;
    }

    public String getLocalPath() {
        return localPath;
    }
}
