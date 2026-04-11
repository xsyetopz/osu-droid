package moe.osudroid.service;

public final class FriendPresence {
    private final String username;
    private final String activity;
    private final boolean online;

    public FriendPresence(String username, String activity, boolean online) {
        this.username = username;
        this.activity = activity;
        this.online = online;
    }

    public String getUsername() {
        return username;
    }

    public String getActivity() {
        return activity;
    }

    public boolean isOnline() {
        return online;
    }
}
