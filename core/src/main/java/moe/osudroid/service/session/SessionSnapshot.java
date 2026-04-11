package moe.osudroid.service.session;

public final class SessionSnapshot {
    private final boolean signedIn;
    private final boolean onlineAuthenticated;
    private final String sessionId;
    private final String username;
    private final String displayName;
    private final long userId;
    private final long rank;
    private final long rankedScore;
    private final float pp;
    private final float accuracy;
    private final String avatarUrl;
    private final String statusMessage;

    public SessionSnapshot(boolean signedIn, String sessionId, String username, String displayName) {
        this(signedIn, false, sessionId, username, displayName, -1L, 0L, 0L, 0f, 0f, "", "");
    }

    public SessionSnapshot(
            boolean signedIn,
            boolean onlineAuthenticated,
            String sessionId,
            String username,
            String displayName,
            long userId,
            long rank,
            long rankedScore,
            float pp,
            float accuracy,
            String avatarUrl,
            String statusMessage) {
        this.signedIn = signedIn;
        this.onlineAuthenticated = onlineAuthenticated;
        this.sessionId = sessionId;
        this.username = username;
        this.displayName = displayName;
        this.userId = userId;
        this.rank = rank;
        this.rankedScore = rankedScore;
        this.pp = pp;
        this.accuracy = accuracy;
        this.avatarUrl = avatarUrl;
        this.statusMessage = statusMessage;
    }

    public static SessionSnapshot signedOut() {
        return signedOut("Sign in to load your online shell.");
    }

    public static SessionSnapshot signedOut(String statusMessage) {
        return new SessionSnapshot(false, false, "", "", "Guest", -1L, 0L, 0L, 0f, 0f, "", statusMessage);
    }

    public boolean isSignedIn() {
        return signedIn;
    }

    public boolean isOnlineAuthenticated() {
        return onlineAuthenticated;
    }

    public String getSessionId() {
        return sessionId;
    }

    public String getUsername() {
        return username;
    }

    public String getDisplayName() {
        return displayName;
    }

    public long getUserId() {
        return userId;
    }

    public long getRank() {
        return rank;
    }

    public long getRankedScore() {
        return rankedScore;
    }

    public float getPp() {
        return pp;
    }

    public float getAccuracy() {
        return accuracy;
    }

    public String getAvatarUrl() {
        return avatarUrl;
    }

    public String getStatusMessage() {
        return statusMessage;
    }
}
