package moe.osudroid.service.multiplayer;

public final class MultiplayerRoomSummary {
    private final String roomId;
    private final String displayName;
    private final int playerCount;
    private final int capacity;
    private final boolean open;

    public MultiplayerRoomSummary(String roomId, String displayName, int playerCount, int capacity, boolean open) {
        this.roomId = roomId;
        this.displayName = displayName;
        this.playerCount = playerCount;
        this.capacity = capacity;
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

    public boolean isOpen() {
        return open;
    }
}
