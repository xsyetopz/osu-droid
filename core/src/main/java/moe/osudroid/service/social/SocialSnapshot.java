package moe.osudroid.service.social;

import java.util.List;

import moe.osudroid.service.FriendPresence;

public final class SocialSnapshot {
    private final List<SocialNotification> notifications;
    private final List<FriendPresence> friends;

    public SocialSnapshot(List<SocialNotification> notifications, List<FriendPresence> friends) {
        this.notifications = notifications;
        this.friends = friends;
    }

    public List<SocialNotification> getNotifications() {
        return notifications;
    }

    public List<FriendPresence> getFriends() {
        return friends;
    }
}
