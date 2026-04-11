package moe.osudroid.service.multiplayer;

import java.util.List;

public final class MultiplayerRoomDetails {
    private final String roomId;
    private final String displayName;
    private final int playerCount;
    private final int capacity;
    private final List<String> players;
    private final boolean open;

    public MultiplayerRoomDetails(
            String roomId,
            String displayName,
            int playerCount,
            int capacity,
            List<String> players,
            boolean open) {
        this.roomId = roomId;
        this.displayName = displayName;
        this.playerCount = playerCount;
        this.capacity = capacity;
        this.players = players;
        this.open = open;
    }

    public String getRoomId() {
        return roomId;
    }

    public String getDisplayName() {
        return displayName;
    }

    public int getPlayerCount() {
        return playerCount;
    }

    public int getCapacity() {
        return capacity;
    }

    public List<String> getPlayers() {
        return players;
    }

    public boolean isOpen() {
        return open;
    }
}
