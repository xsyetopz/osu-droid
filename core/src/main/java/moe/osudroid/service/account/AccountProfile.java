package moe.osudroid.service.account;

public final class AccountProfile {
    private final String displayName;
    private final String status;
    private final String location;
    private final long rankedScore;
    private final float accuracy;
    private final long rank;
    private final float pp;
    private final String avatarUrl;
    private final boolean online;

    public AccountProfile(String displayName, String status, String location, long rankedScore, float accuracy) {
        this(displayName, status, location, rankedScore, accuracy, 0L, 0f, "", false);
    }

    public AccountProfile(
            String displayName,
            String status,
            String location,
            long rankedScore,
            float accuracy,
            long rank,
            float pp,
            String avatarUrl,
            boolean online) {
        this.displayName = displayName;
        this.status = status;
        this.location = location;
        this.rankedScore = rankedScore;
        this.accuracy = accuracy;
        this.rank = rank;
        this.pp = pp;
        this.avatarUrl = avatarUrl;
        this.online = online;
    }

    public static AccountProfile signedOut() {
        return new AccountProfile("Guest", "Sign in to load your online shell.", "Offline", 0L, 0f, 0L, 0f, "", false);
    }

    public String getDisplayName() {
        return displayName;
    }

    public String getStatus() {
        return status;
    }

    public String getLocation() {
        return location;
    }

    public long getRankedScore() {
        return rankedScore;
    }

    public float getAccuracy() {
        return accuracy;
    }

    public long getRank() {
        return rank;
    }

    public float getPp() {
        return pp;
    }

    public String getAvatarUrl() {
        return avatarUrl;
    }

    public boolean isOnline() {
        return online;
    }
}
